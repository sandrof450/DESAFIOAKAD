using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.DTOs;
using APIGateway.Interfaces;
using APIGateway.Models;
using APIGateway.Repositories;
using Microsoft.AspNetCore.Identity;

namespace APIGateway.Services
{
    public class AdministradorService : IAdministradorService
    {
        private readonly AdministradorRepository _administradorRepository;

        public AdministradorService(AdministradorRepository administradorRepository)
        {
            _administradorRepository = administradorRepository;
        }

        public Administrador? BuscarPorId(int id)
        {
            var administrador = _administradorRepository.BuscarPorId(id);
            return administrador;
        }

        public Administrador IncluirUsuarioAdministrador(Administrador administrador)
        {
            if (administrador == null)
            {
                throw new ArgumentNullException(nameof(administrador), "The provided Administrator object cannot be null.");
            }

            string administradorEmail = administrador.Email;
            var emailExists = _administradorRepository.EmailExists(administradorEmail);
            if (emailExists)
            {
                throw new InvalidOperationException($"An administrator with the email '{administradorEmail}' already exists.");
            }

            var passwordHash = new PasswordHasher<Administrador>();
            administrador.Senha = passwordHash.HashPassword(administrador, administrador.Senha);

            var administradorSave = _administradorRepository.IncluirUsuarioAdministrador(administrador);

            return administradorSave;
        }

        public async Task<Administrador?> Login(LoginDTO loginDTO)
        {
            var administrador = await _administradorRepository.Login(loginDTO);
            if (administrador == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password. ponto1");
            }

            var passwordHash = new PasswordHasher<Administrador>();
            var passwordVerificationResult = passwordHash.VerifyHashedPassword(administrador, administrador.Senha, loginDTO.Senha);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password. ponto2");
            }

            return administrador;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _administradorRepository.Todos(pagina).AsQueryable();

            int itensPorPagina = 10;
            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina)
                            .Take(itensPorPagina);

            }
            return query.ToList();
        }
  }
}