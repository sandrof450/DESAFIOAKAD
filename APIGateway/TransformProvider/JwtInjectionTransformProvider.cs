using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace APIGateway.TransformProvider
{
    public class JwtInjectionTransformProvider : ITransformProvider
    {
        private readonly IConfiguration _configuration;

        public JwtInjectionTransformProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Apply(TransformBuilderContext context)
        {
            if (!context.Route.RouteId.Equals("login", StringComparison.OrdinalIgnoreCase))
                return;
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            
            context.AddRequestTransform(async transformContext =>
            {                
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials: creds,
                    claims: new[] { new Claim("internal", "true") }
                );

                var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                
                transformContext.ProxyRequest.Headers.Remove("Authorization");
                transformContext.ProxyRequest.Headers.Add("Authorization", $"Bearer {tokenValue}");                    
            });
            
      
        }
        public void ValidateCluster(TransformClusterValidationContext context){ }

        public void ValidateRoute(TransformRouteValidationContext context) { }
  }
}