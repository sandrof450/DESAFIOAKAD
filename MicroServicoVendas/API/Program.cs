using MicroServicoVendas.Aplication.Implementations.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MicroServicoVendas.Domain.Interfaces;
using MicroServicoVendas.Infrastructure.Repositories.Implementations;
using MicroServicoVendas.Infrastructure.Repositories.Context;
using MicroServicoVendas.Infrastructure.RabbitMQ.Interfaces;
using MicroServicoVendas.Infrastructure.RabbitMQ.Publishers;
using MicroServicoVendas.Infrastructure.Clients.Implementations;
using MicroServicoVendas.Infrastructure.RabbitMQ.Consumers;
using MicroServicoVendas.Aplication.Services.Interfaces;
using MicroServicoVendas.Infrastructure.Clients.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);

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

                var hostValue = context.Request.Headers["X-Forwarded-Host"].ToString()
                    ?? context.Request.Headers["Host"].ToString()
                    ?? context.Request.Headers["Origin"].ToString();

                // Verifica se a requisição veio do gateway local (http://localhost:5003/) ou do Render (akad-gateway.onrender.com)
                var isLocalGateway = hostValue.Contains("localhost:5003");
                var isRenderGateway = hostValue.Contains("akad-gateway.onrender.com");

                if (!(isLocalGateway || isRenderGateway))
                    return context.Response.WriteAsync("Access denied: Unauthorized gateway.");

                return context.Response.WriteAsync("Acesso negado. O token é inválido, expirado ou não foi informado");
            }
        };
    });    
    builder.Services.AddAuthorization(Options =>
    {
        Options.AddPolicy("GatewayOnly", policy =>
        {
            policy.RequireAssertion(context =>
            {
                //Tentativa de obter o AuthorizationFilterContext a partir do contexto de autorização
                //Significa: "tente tratar context.Resource como AuthorizationFilterContext; se não for compatível, mvcContext será null".
                // Diferença para cast explícito (AuthorizationFilterContext)context.Resource: o cast explícito lança InvalidCastException se a conversão falhar; as não lança, só produz null.
                var mvcContext = context.Resource as AuthorizationFilterContext;
                var httpContext = mvcContext?.HttpContext;
                if (httpContext == null) return false;

                var hasValue = httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                    ?? httpContext.Request.Headers["Host"].FirstOrDefault()
                    ?? httpContext.Request.Headers["Origin"].FirstOrDefault();

                var isHostValid = hasValue != null && (hasValue.Contains("localhost:5003") || hasValue.Contains("akad-gateway.onrender.com"));
                return isHostValid;
            });
        });

        Options.AddPolicy("AdminOnly", policy =>
        {
            policy.RequireClaim("role", "Admin");
        });

        Options.AddPolicy("AdminEditorOnly", policy =>
        {
            policy.RequireAssertion(context =>
            {
                var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "Perfil")?.Value;
                if (string.IsNullOrEmpty(userRole)) return false;

                var rolerIsValid = userRole == "Admin" || userRole == "Editor";
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
// Para depurar e verificar se as configurações estão sendo carregadas
// Console.WriteLine($"[DEBUG] RabbitMQ URI: {builder.Configuration["RabbitMQ:Uri"]}");
// Console.WriteLine($"[DEBUG] RabbitMQ Queue: {builder.Configuration["RabbitMQ:Queue"]}");
#endregion

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter("GatewayOnly"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<VendaContext>(
    //options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IPedidoService,PedidoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMQPublisher>();

//Configuração do HttpClient para comunicação com o microserviço de estoque via API Gateway ambiente local
builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(
    client => client.BaseAddress = new Uri("https://akad-gateway.onrender.com")//http://apigateway:8080 uri do API GATEWAY em ambiente local
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
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
            new string[] {}
        }
    });
});

builder.Services.AddLogging();
builder.Services.AddHostedService<RabbitMQConsumer>();
#endregion

#region App
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization(); 
}

#region Aplica migrations automaticamente ao iniciar o aplicativo
using (var scope = app.Services.CreateScope())
{
    var DbContext = scope.ServiceProvider.GetRequiredService<VendaContext>();
    Console.WriteLine($"[INFO] Aplicando migrations em {DbContext.Database.GetDbConnection().ConnectionString}");
    try
    {
        DbContext.Database.Migrate();
        Console.WriteLine("[INFO] Migrações aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao aplicar migrations: {ex.Message}");
    }
}
#endregion

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
#endregion