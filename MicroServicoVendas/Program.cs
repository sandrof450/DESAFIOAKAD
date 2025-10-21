using MicroServicoVendas.Clients;
using MicroServicoVendas.Repositories;
using MicroServicoVendas.Repositories.Context;
using MicroServicoVendas.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MicroServicoVendas.Interfaces;
using MicroServicoVendas.RabbitMQ.Publishers;
using MicroServicoVendas.RabbitMQ.Consumers;

#region Builder
var builder = WebApplication.CreateBuilder(args);

#region Configuração de autenticação JWT no microserviço de estoque.
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
});
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<VendaContext>(
    //options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<EstoqueClient>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMQPublisher>();

//Configuração do HttpClient para comunicação com o microserviço de estoque via API Gateway ambiente local
builder.Services.AddHttpClient<EstoqueClient>(
    client => client.BaseAddress = new Uri("https://akad-gateway.onrender.com")//http://apigateway:8080 uri do API GATEWAY em ambiente local
);

//Configuração do HttpClient para comunicação com o microserviço de estoque via API Gateway ambiente PRD
// builder.Services.AddHttpClient<EstoqueClient>(
//     client => client.BaseAddress = new Uri("")//TODO: COLOCAR URL DO API GATEWAY AQUI
// );

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();
#endregion