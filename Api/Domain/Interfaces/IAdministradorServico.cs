using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO login);

        List<Administrador> Todos(int? pagina = 1, string? email = null, string? perfil = null);

        Administrador? BuscaPorId(int id);

        Administrador Incluir(Administrador administrador);

        void Atualizar(Administrador administrador);

        void Apagar(Administrador administrador);
    }
}