using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Domain.Entities;

namespace Test.Domain
{
    [TestClass]
    public class VeiculosTest
    {
        [TestMethod]
        public void GetSetPropertiesTest()
        {
            // Arrange
            var veiculo = new Veiculo();

            // Act
            veiculo.Id = 1;
            veiculo.Nome = "Gol";
            veiculo.Marca = "Volkswagen";
            veiculo.Ano = 1989;

            // Assert
            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("Gol", veiculo.Nome);
            Assert.AreEqual("Volkswagen", veiculo.Marca);
            Assert.AreEqual(1989, veiculo.Ano);
        }
    }
}