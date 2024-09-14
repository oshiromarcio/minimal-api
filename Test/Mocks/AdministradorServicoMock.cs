using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;

namespace Test.Mocks
{
    public class AdministradorServicoMock : IAdministradorServico
    {
        private static List<Administrador> administradores = new List<Administrador>() {
            new Administrador {
                Id = 1,
                Email = "test@test.com",
                Senha = "test",
                Perfil = "Adm"
            },
            new Administrador {
                Id = 2,
                Email = "vendedor@test.com",
                Senha = "vendedor",
                Perfil = "Vendedor"
            }
        };

        public void Apagar(Administrador administrador)
        {
            administradores.Remove(administrador);
        }

        public void Atualizar(Administrador administrador)
        {
            var admFind = administradores.Find(a => a.Id == administrador.Id);
            admFind = administrador;
        }

        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(a => a.Id == id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count() + 1;
            administradores.Add(administrador);
            return administrador;
        }

        public Administrador? Login(LoginDTO login)
        {
            return administradores.Find(x => x.Email == login.Email && x.Senha == login.Senha);
        }

        public List<Administrador> Todos(int? pagina = 1, string? email = null, string? perfil = null)
        {
            return administradores;
        }
    }
}