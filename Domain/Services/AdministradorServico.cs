using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _context;

        public AdministradorServico(DbContexto context)
        {
            _context = context;
        }

        public Administrador? Login(LoginDTO login)
        {
            return _context.Administradores.Where(a => a.Email == login.Email && a.Senha == login.Senha).FirstOrDefault();
        }
    }
}