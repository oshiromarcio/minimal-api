using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DbContexto _context;

        public VeiculoServico(DbContexto context)
        {
            _context = context;
        }

        public void Apagar(Veiculo veiculo)
        {
            _context.Veiculos.Remove(veiculo);
            _context.SaveChanges();
            return;
        }

        public void Atualizar(Veiculo veiculo)
        {
            _context.Veiculos.Update(veiculo);
            _context.SaveChanges();
            return;
        }

        public Veiculo? BuscaPorId(int id)
        {
            var veiculo =_context.Veiculos.Find(id);
            return veiculo;
        }

        public Veiculo Incluir(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            _context.SaveChanges();
            return veiculo;
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _context.Veiculos.AsQueryable();
            if (!string.IsNullOrEmpty(nome))
                query = query.Where(x => EF.Functions.Like(x.Nome.ToUpper(), $"%{nome}%".ToUpper()));

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(x => EF.Functions.Like(x.Marca.ToUpper(), $"%{marca}%".ToUpper()));

            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}