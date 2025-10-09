using System.ComponentModel.DataAnnotations;
using APIGateway.DTOs;
using APIGateway.Interfaces;
using APIGateway.Models;
using APIGateway.ModelsViews;
using APIGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly string _jwtKey;
        private readonly LoginService _loginService;
        private readonly IAdministradorService _administradorService;

        public LoginController(IConfiguration configuration, LoginService loginService, IAdministradorService administradorService)
        {
            _jwtKey = configuration["Jwt:Key"];
            _loginService = loginService;
            _administradorService = administradorService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var administrador = await _administradorService.Login(loginDTO);

                if (administrador != null)
                {
                    string token = _loginService.GenerateTokenJwt(administrador, _jwtKey);
                    return Results.Ok(new AdministradorLogado
                    {
                        Email = administrador.Email,
                        Perfil = administrador.Perfil,
                        Token = token,
                    });
                }
                else
                {
                    return Results.Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        [HttpPost("IncluirAdministrador")]
        public IResult Incluir(Administrador administrador)
        {
            try
            {
                var result = _administradorService.IncluirUsuarioAdministrador(administrador);
                return Results.Ok(result);
            }
            catch (ArgumentNullException ax)
            {
                return Results.Problem($"Erro de valor null. mensagem: {ax.Message}");
            }
            catch (InvalidOperationException ix)
            {
                return Results.Problem($"Erro InvalidOperationException. Mensagem {ix.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        [HttpGet]
        public IResult ListarTodos(int pagina)
        {
            try
            {
                var result = _administradorService.Todos(pagina);
                return Results.Ok(result);

            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}