using APIGateway.DTOs;
using APIGateway.Models;

namespace APIGateway.Interfaces
{
    public interface IAdministradorService
    {
        Task<Administrador?> Login(LoginDTO loginDTO);
        Administrador IncluirUsuarioAdministrador(Administrador administrador);
        Administrador? BuscarPorId(int id);
        List<Administrador> Todos(int? pagina);
        
    }
}