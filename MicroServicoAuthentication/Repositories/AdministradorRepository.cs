using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.DTOs;
using APIGateway.Interfaces;
using APIGateway.Models;
using APIGateway.Repositories.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Repositories
{
    public class AdministradorRepository
    {
        private readonly GatewayContext _context;

        public AdministradorRepository(GatewayContext context)
        {
            _context = context;
        }
        public Administrador? BuscarPorId(int id)
        {
            var administrador = _context.Administradores.AsNoTracking().Where(v => v.Id == id).FirstOrDefault();

            return administrador;
        }

        public Administrador IncluirUsuarioAdministrador(Administrador administrador)
        {
            _context.Administradores.Add(administrador);

            var validateIfSaveInformationsInDataBase = _context.SaveChanges() > 0;
            if (!validateIfSaveInformationsInDataBase)
                throw new Exception("Don't save the information in the database");

            return administrador;
        }

        public async Task<Administrador?> Login(LoginDTO loginDTO)
        {
            var administrador = await _context.Administradores.AsNoTracking().Where(a => a.Email == loginDTO.Email).FirstOrDefaultAsync();

            return administrador;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _context.Administradores.AsQueryable();


            return query.ToList();
        }

        public bool EmailExists(string email)
        {
            var emailExists = _context.Administradores.AsNoTracking().Where(e => e.Email == email).FirstOrDefault() != null;
            return emailExists;
        }
        
    }
}