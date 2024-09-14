using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class VeiculoRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }

        private async Task<HttpResponseMessage> DoLoginAdministrador()
        {
            var loginDTO = new LoginDTO {
                Email = "test@test.com",
                Senha = "test"
            };
            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            return await Setup.client.PostAsync("/administradores/login", content);
        }

        [TestMethod]
        public async Task VeiculoFindOneTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            var responseVeiculo = await Setup.client.GetAsync("/veiculos/1");
            var resultVeiculo = await responseVeiculo.Content.ReadAsStringAsync();
            
            var veiculoJson = JsonSerializer.Deserialize<Veiculo>(resultVeiculo, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseVeiculo.StatusCode);
            Assert.IsNotNull(veiculoJson);
            Assert.AreEqual("Passat", veiculoJson.Nome);
        }

        [TestMethod]
        public async Task VeiculoGetAllTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            var responseVeiculo = await Setup.client.GetAsync("/veiculos?pagina=1");
            var resultVeiculo = await responseVeiculo.Content.ReadAsStringAsync();
            var veiculosJson = JsonSerializer.Deserialize<List<Veiculo>>(resultVeiculo, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseVeiculo.StatusCode);
            Assert.IsNotNull(veiculosJson);
            Assert.AreEqual(2, veiculosJson.Count);
        }

        [TestMethod]
        public async Task VeiculoSaveTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            var veiculo = new Veiculo() {
                Id = 0,
                Nome = "Civic",
                Marca = "Honda",
                Ano = 2007
            };

            // Act
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);
            var contentVeiculo = new StringContent(JsonSerializer.Serialize(veiculo), Encoding.UTF8, "Application/json");
            var responseVeiculo = await Setup.client.PostAsync("/veiculos", contentVeiculo);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, responseVeiculo.StatusCode);
        }

        [TestMethod]
        public async Task VeiculoUpdateTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            var responseVeiculo = await Setup.client.GetAsync("/veiculos/1");
            var resultVeiculo = await responseVeiculo.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(resultVeiculo, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            if(veiculo == null)
            {
                throw new Exception("Não retornou veículo Id = 1, verificar o mock.");
            }
            
            int anoAntes = veiculo.Ano;
            int anoDepois = 1990;

            veiculo.Ano = anoDepois;
            var contentVeiculoPut = new StringContent(JsonSerializer.Serialize(veiculo), Encoding.UTF8, "Application/json");
            var responseVeiculoPut = await Setup.client.PutAsync("/veiculos/1", contentVeiculoPut);

            var responseVeiculoPos = await Setup.client.GetAsync("/veiculos/1");
            var resultVeiculoPos = await responseVeiculoPos.Content.ReadAsStringAsync();
            var veiculoPos = JsonSerializer.Deserialize<Veiculo>(resultVeiculoPos, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseVeiculoPut.StatusCode);
            Assert.AreEqual(1982, anoAntes);
            Assert.AreEqual(veiculoPos != null ? veiculoPos.Ano : 0, anoDepois);
        }

        [TestMethod]
        public async Task VeiculoDeleteTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);
            var responseGetAll = await Setup.client.GetAsync("/veiculos?pagina=1");
            var resultGetAll = await responseGetAll.Content.ReadAsStringAsync();
            var veiculosAll = JsonSerializer.Deserialize<List<Veiculo>>(resultGetAll, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            int qtdeBefore = veiculosAll != null ? veiculosAll.Count() : 0;

            var responseDelete = await Setup.client.DeleteAsync("/veiculos/1");
            
            var responseGetAll2 = await Setup.client.GetAsync("/veiculos?pagina=1");
            var resultGetAll2 = await responseGetAll2.Content.ReadAsStringAsync();
            var veiculosAll2 = JsonSerializer.Deserialize<List<Veiculo>>(resultGetAll2, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            int qtdeAfter = veiculosAll2 != null ? veiculosAll2.Count() : 0;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseDelete.StatusCode);
            Assert.AreEqual(qtdeBefore - 1, qtdeAfter);
        }
    }
}