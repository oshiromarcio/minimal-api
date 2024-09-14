using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Db;

namespace Test.Domain.Services
{
    [TestClass]
    public class VeiculoServiceTest
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
        public void VeiculoSaveTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            var veiculo = new Veiculo {
                Id = 1,
                Nome = "Nome",
                Marca = "Marca",
                Ano = 2000
            };
            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(veiculo);

            // Assert
            Assert.AreEqual(1, veiculoServico.Todos(1).Count());
        }

        [TestMethod]
        public void VeiculoFindIdTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            int idTeste = 1;
            var veiculo = new Veiculo {
                Id = idTeste,
                Nome = "Nome",
                Marca = "Marca",
                Ano = 2000
            };
            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(veiculo);
            var veiculoBanco = veiculoServico.BuscaPorId(idTeste);

            // Assert
            Assert.IsNotNull(veiculoBanco);
            if (veiculoBanco != null)
            {
                Assert.AreEqual(idTeste, veiculoBanco.Id);
            }
        }

        [TestMethod]
        public void VeiculoUpdateTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            string nomeBefore = "Nome Antes";
            string nomeAfter = "Nome Depois";
            var veiculo = new Veiculo {
                Id = 1,
                Nome = nomeBefore,
                Marca = "Marca",
                Ano = 2000
            };
            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(veiculo);
            int qtdeIncluir = veiculoServico.Todos(1).Count();
            var veiculoBanco = veiculoServico.BuscaPorId(1) ?? veiculo;
            int qtdeBuscarPorId = veiculoBanco == null ? 0 : 1;
            if (veiculoBanco != null)
            {
                veiculoBanco.Nome = nomeAfter;
                veiculoServico.Atualizar(veiculoBanco);
            }
            var admBancoUpdated = veiculoServico.BuscaPorId(1);

            // Assert
            Assert.AreEqual(1, qtdeIncluir);
            Assert.AreEqual(1, qtdeBuscarPorId);
            Assert.AreEqual(nomeAfter, admBancoUpdated?.Nome);
        }

        [TestMethod]
        public void VeiculoDeleteTest()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

            int idTeste = 1;
            var adm = new Veiculo {
                Id = idTeste,
                Nome = "Nome",
                Marca = "Marca",
                Ano = 2000
            };
            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.Incluir(adm);
            int qtdeIncluir = veiculoServico.Todos(1).Count();

            veiculoServico.Apagar(adm);
            int qtdeExcluir = veiculoServico.Todos(1).Count();

            // Assert
            Assert.AreEqual(1, qtdeIncluir);
            Assert.AreEqual(0, qtdeExcluir);
        }
    }
}