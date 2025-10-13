using System.Text;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

#region Configura autenticação JWT no Gateway
builder.Services.AddAuthentication("Bearer")
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "http://authentication:8080",//http://localhost:[numero da porta do microservico] -- Para teste local
        ValidAudience = "http://authentication:8080",//http://localhost:[numero da porta do microservico] -- Para teste local
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
  });

builder.Services.AddAuthorization();
#endregion

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

#region 🔑 Configuração YARP
var apiKey = Environment.GetEnvironmentVariable("GATEWAY_SECRET");
if (string.IsNullOrEmpty(apiKey)) apiKey =  builder.Configuration["ApiKey:Key"];

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key is not configured.");

builder.Services.AddReverseProxy()
    .LoadFromMemory(new[]
    {
        #region Rota para o microserviço de Login
        new RouteConfig
        {
            RouteId = "login",
            ClusterId = "loginCluster",
            Match = new RouteMatch { Path = "/api/login/{**catch-all}" },
            Transforms = new[]{
                new Dictionary<string, string>{
                    {"RequestHeader", "X-Internal-Secret"},
                    {"Set", apiKey}
                }
            }
        },
        #endregion

        #region Rota para o microserviço de Vendas
        new RouteConfig
        {
            RouteId = "vendas",
            ClusterId = "vendasCluster",
            Match = new RouteMatch { Path = "/api/pedido/{**catch-all}" }
        },
        #endregion
        
        #region Rota para o microserviço de Estoque
        new RouteConfig
        {
            RouteId = "estoque",
            ClusterId = "estoqueCluster",
            Match = new RouteMatch { Path = "/api/Produto/{**catch-all}" },
        }
        #endregion
    },
    new[]
    {
        #region Cluster do serviço de Login
        new ClusterConfig
        {
            ClusterId = "loginCluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = "https://akad-authentication.onrender.com" } }//http://localhost:[numero da porta do microservico] || http://authentication:8080 -- Para teste local
            }
        },
        #endregion

        #region Cluster do serviço de Vendas
        new ClusterConfig
        {
            ClusterId = "vendasCluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = "https://akad-vendas.onrender.com" } }//http://localhost:[numero da porta do microservico] || http://vendas:8080 -- Para teste local
            }
        },
        #endregion
        
        #region Cluster do serviço de Estoque
        new ClusterConfig
        {
            ClusterId = "estoqueCluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "dest1", new DestinationConfig { Address = "https://desafioakad.onrender.com" } }//http://localhost:[numero da porta do microservico] || http://estoque:8080 -- Para teste local
            }
        }
        #endregion
    });
#endregion 

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapGet("/", () => "Hello World!");

app.Run();
