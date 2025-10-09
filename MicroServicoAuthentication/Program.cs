using System.Text;
using APIGateway.Interfaces;
using APIGateway.Repositories;
using APIGateway.Repositories.Contexts;
using APIGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT jey is not configured.");

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
        Description = "JWT Authorization header using the Bearer scheme.",
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

builder.Services.AddDbContext<GatewayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<AdministradorRepository>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();

builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
)
.AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    }
);

builder.Services.AddAuthorization();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
    // options.ListenAnyIP(5279);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/login"))
    {
        var key = Environment.GetEnvironmentVariable("GATEWAY_SECRET");
        if (string.IsNullOrEmpty(key)) key = builder.Configuration["ApiKey:Key"];

        var expectedSecret = key;
        var receivedSecret = context.Request.Headers["X-Internal-Secret"].FirstOrDefault();

        if (receivedSecret != expectedSecret)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access denied: Unauthorized gateway.");
            return;
        }
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAuthorization();

app.MapControllers();

app.Run();
