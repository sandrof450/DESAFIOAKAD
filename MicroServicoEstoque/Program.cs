using MicroServicoEstoque.Repositories;
using MicroServicoEstoque.Repositories.Context;
using MicroServicoEstoque.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MicroServicoEstoque.Interfaces;
using MicroServicoEstoque.RabbitMQ.Publishers;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key is not configured.");

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

// A autenticação principal deste microserviço é realizada pela API Gateway, portanto, a configuração de autenticação local não é estritamente necessária neste momento.
// No entanto, para reforçar a segurança, será implementada uma validação adicional para verificar o perfil do usuário que está acessando o serviço.
// Essa validação garantirá que apenas usuários com perfis autorizados possam acessar determinados endpoints, mesmo após a autenticação feita pelo Gateway.
#region Configuração de autenticação JWT no microserviço de estoque.
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

builder.Services.AddDbContext<EstoqueContext>(
    //options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ProdutoRepository>();

builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();
