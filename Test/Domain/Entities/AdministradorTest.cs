using minimal_api.Domain.Entities;

namespace Test.Domain
{
    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void GetSetPropertiesTest()
        {
            // Arrange
            var adm = new Administrador {
                Id = 1,
                Email = "test@test.com",
                Senha = "teste",
                Perfil = "Adm"
            };

            // Act

            // Assert
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("test@test.com", adm.Email);
            Assert.AreEqual("teste", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);
        }
    }
}