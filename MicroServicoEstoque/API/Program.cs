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

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                //Impede o comportamento padrão (retornar apenas 401 sem corpo)
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var req = context.Request;
                var hostValue = req.Host.HasValue ? req.Host.Value.ToLowerInvariant() : string.Empty;
                var pathValue = req.Path.HasValue ? req.Path.Value : "/";

                // Verifica se a requisição veio do gateway local (http://localhost:5117/) ou do Render (https://desafioakad.onrender.com/)
                var isLocalGateway = req.Scheme == "http" && hostValue.StartsWith("localhost:5117");
                var isRenderGateway = req.Scheme == "https" && hostValue.StartsWith("desafioakad.onrender.com");

                if (isLocalGateway || isRenderGateway)
                    return context.Response.WriteAsync("Access denied: Unauthorized gateway.");

                return context.Response.WriteAsync("Acesso negado. O token é inválido, expirado ou não foi informado");
            }
        };
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

builder.Services.AddControllers();
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
    client => client.BaseAddress = new Uri("http://localhost:5244/")
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
