using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MicroServicoEstoque.Aplication.Services.Implementations;
using MicroServicoEstoque.Aplication.IA;
using MicroServicoEstoque.Infrastructure.Repositories.Implementations;
using MicroServicoEstoque.Infrastructure.Repositories.Context;
using MicroServicoEstoque.Infrastructure.clients.Implementations;
using MicroServicoEstoque.Infrastructure.RabbitMQ.Interfaces;
using MicroServicoEstoque.Infrastructure.RabbitMQ.Publishers;
using MicroServicoEstoque.Aplication.Services.Interfaces;
using MicroServicoEstoque.Domain.Interfaces;
using MicroServicoEstoque.Infrastructure.clients.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

// A autenticação principal deste microserviço é realizada pela API Gateway, portanto, a configuração de autenticação local não é estritamente necessária neste momento.
// No entanto, para reforçar a segurança, será implementada uma validação adicional para verificar o perfil do usuário que está acessando o serviço.
// Essa validação garantirá que apenas usuários com perfis autorizados possam acessar determinados endpoints, mesmo após a autenticação feita pelo Gateway.
#region Configuração de autenticação JWT no microserviço de estoque.
if (!builder.Environment.IsDevelopment())
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key is not configured.");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };

        // Realiza a verificação da URL utilizada para acesso, caso 
        options.Events = new JwtBearerEvents
        {
            //O OnChallenge é um callback (evento) disparado automaticamente pelo middleware de autenticação JWT 
            // quando ocorre uma falha de autenticação — isto é, quando a requisição não contém um token válido 
            // ou o token está ausente/expirado.
            OnChallenge = context =>
            {
                //Impede o comportamento padrão (retornar apenas 401 sem corpo)
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var hostValue = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                                ?? context.Request.Headers["Host"].FirstOrDefault()
                                ?? context.Request.Headers["Origin"].FirstOrDefault();

                // Verifica se a requisição veio do gateway local (http://localhost:5117/) ou do Render (https://desafioakad.onrender.com/)
                var isLocalGateway = hostValue.Contains("localhost:5003");
                var isRenderGateway = hostValue.Contains("akad-gateway.onrender.com");

                if (!(isLocalGateway || isRenderGateway))
                    return context.Response.WriteAsync("Access denied: Unauthorized gateway.");

                return context.Response.WriteAsync("Acesso negado. O token é inválido, expirado ou não foi informado");
            }
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("GatewayOnly", policy =>
            policy.RequireAssertion(context =>
            {
                var mvcContext = context.Resource as AuthorizationFilterContext;
                var httpContext = mvcContext?.HttpContext;
                if (httpContext == null) return false;

                var host = httpContext.Request.Headers["Origin"].FirstOrDefault()
                ?? httpContext.Request.Headers["Referer"].FirstOrDefault()
                ?? httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault();

                //Aceita apenas requisições do gateway
                var isHostValid = host != null && (host.Contains("akad-gateway.onrender.com") || host.Contains("localhost:5003"));

                return isHostValid;
            })
        );
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireClaim("role", "Admin")
        );

        options.AddPolicy("AdminEditorOnly", policy =>
        {
            policy.RequireAssertion(context =>
            {
                var userRoler = context.User.Claims.FirstOrDefault(claim => claim.Type == "Perfil")?.Value;

                if (string.IsNullOrEmpty(userRoler)) return false;

                var rolerIsValid = userRoler == "Admin" || userRoler == "Editor";

                return rolerIsValid;
            });
        });
    });
}
#endregion

// No .NET, os valores definidos em variáveis de ambiente sobrescrevem automaticamente
// as configurações do appsettings.json durante a execução, inclusive em ambientes como o Render.
#region Define que os valores de configuração podem vir de variáveis de ambiente
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
#endregion

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter("GatewayOnly"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space]",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            new string[] { }
        }
    });
});


#region Configuração do context
builder.Services.AddDbContext<EstoqueContext>(
    //options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
#endregion

#region Configure HttpClient
builder.Services.AddHttpClient<IIAService, IAService>();

//Registro  IHttpContextAccessor necessário, se não ocorre erro de injeção de depencia
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IVendaClient, VendaClient>(
    client => client.BaseAddress = new Uri("https://akad-gateway.onrender.com")
);
#endregion

#region Inject dependencies
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IIAService, IAService>();

builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
