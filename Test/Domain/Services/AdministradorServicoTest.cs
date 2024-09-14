
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Db;

namespace Test.Domain.Services
{
    [TestClass]
    public class AdministradorServicoTest
    {
        private MyDbContext CriarContextoDeTeste()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            return new MyDbContext(configuration);
        }

        [TestMethod]
        public void AdministradorSaveTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador {
                Id = 1,
                Email = "test@test.com",
                Senha = "teste",
                Perfil = "Adm"
            };
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);

            // Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count());
        }

        [TestMethod]
        public void AdministradorFindIdTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            int idTeste = 2;
            var adm = new Administrador {
                Id = idTeste,
                Email = "test@test.com",
                Senha = "teste",
                Perfil = "Adm"
            };
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);
            var admBanco = administradorServico.BuscaPorId(idTeste);

            // Assert
            Assert.IsNotNull(admBanco);
            if (admBanco != null)
            {
                Assert.AreEqual(idTeste, admBanco.Id);
            }
        }

        [TestMethod]
        public void AdministradorUpdateTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            string emailBefore = "email1@test.com";
            string emailAfter = "email2@test.com";
            var adm = new Administrador {
                Id = 1,
                Email = emailBefore,
                Senha = "teste",
                Perfil = "Adm"
            };
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);
            int qtdeIncluir = administradorServico.Todos(1).Count();
            var admBanco = administradorServico.BuscaPorId(1) ?? adm;
            int qtdeBuscarPorId = admBanco == null ? 0 : 1;
            if (admBanco != null)
            {
                admBanco.Email = emailAfter;
                administradorServico.Atualizar(admBanco);
            }
            var admBancoUpdated = administradorServico.BuscaPorId(1);

            // Assert
            Assert.AreEqual(1, qtdeIncluir);
            Assert.AreEqual(1, qtdeBuscarPorId);
            Assert.AreEqual(emailAfter, admBancoUpdated?.Email);
        }

        [TestMethod]
        public void AdministradorDeleteTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            int idTeste = 1;
            var adm = new Administrador {
                Id = idTeste,
                Email = "test@test.com",
                Senha = "teste",
                Perfil = "Adm"
            };
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);
            int qtdeIncluir = administradorServico.Todos(1).Count();

            administradorServico.Apagar(adm);
            int qtdeExcluir = administradorServico.Todos(1).Count();

            // Assert
            Assert.AreEqual(1, qtdeIncluir);
            Assert.AreEqual(0, qtdeExcluir);
        }
    }
}