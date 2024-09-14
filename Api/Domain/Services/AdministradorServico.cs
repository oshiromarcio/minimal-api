using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly MyDbContext _context;

        public AdministradorServico(MyDbContext context)
        {
            _context = context;
        }

        public void Apagar(Administrador administrador)
        {
            _context.Administradores.Remove(administrador);
            _context.SaveChanges();
            return;
        }

        public void Atualizar(Administrador administrador)
        {
            _context.Administradores.Update(administrador);
            _context.SaveChanges();
            return;
        }

        public Administrador? BuscaPorId(int id)
        {
            return _context.Administradores.Find(id);

        }

        public Administrador Incluir(Administrador administrador)
        {
            _context.Administradores.Add(administrador);
            _context.SaveChanges();
            return administrador;
        }

        public Administrador? Login(LoginDTO login)
        {
            return _context.Administradores.Where(a => a.Email == login.Email && a.Senha == login.Senha).FirstOrDefault();
        }

        public List<Administrador> Todos(int? pagina = 1, string? email = null, string? perfil = null)
        {
            var query = _context.Administradores.AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(x => EF.Functions.Like(x.Email.ToUpper(), $"%{email}%".ToUpper()));
            if (!string.IsNullOrEmpty(perfil))
                query = query.Where(x => EF.Functions.Like(x.Perfil, $"%{perfil}%"));

            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}