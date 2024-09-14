
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Query;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Enums;
using minimal_api.Domain.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTest
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
        public async Task LoginTest()
        {
            // Arrange
            var loginDTO = new LoginDTO {
                Email = "test@test.com",
                Senha = "test"
            };

            // Act
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(administradorLogado?.Email);
            Assert.IsNotNull(administradorLogado?.Token);
        }

        [TestMethod]
        public async Task AdministradorFindOneTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);


            // Act
            var responseAdm = await Setup.client.GetAsync("/administradores/1");
            var resultAdm = await responseAdm.Content.ReadAsStringAsync();
            var admJson = JsonSerializer.Deserialize<AdministradorModelView>(resultAdm, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseAdm.StatusCode);
            Assert.IsNotNull(admJson);
            Assert.AreEqual(1, admJson.Id);
        }

        [TestMethod]
        public async Task AdministradorGetAllTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            var responseAdm = await Setup.client.GetAsync("/administradores?pagina=1");
            var resultAdm = await responseAdm.Content.ReadAsStringAsync();
            var admsJson = JsonSerializer.Deserialize<List<AdministradorModelView>>(resultAdm, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseAdm.StatusCode);
            Assert.IsNotNull(admsJson);
            Assert.AreEqual(2, admsJson.Count);
        }

        [TestMethod]
        public async Task AdministradorSaveTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);
            var admDTO = new AdministradorDTO {
                Email = "test@test.com",
                Senha = "test",
                Perfil = Perfil.Vendedor
            };

            // Act
            var contentAdm = new StringContent(JsonSerializer.Serialize(admDTO), Encoding.UTF8, "Application/json");
            var responseAdm = await Setup.client.PostAsync("/administradores", contentAdm);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, responseAdm.StatusCode);
        }

        [TestMethod]
        public async Task AdministradorUpdateTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            int admId = 2;
            var responseAdmPre = await Setup.client.GetAsync($"/administradores/{admId}");
            var resultAdmPre = await responseAdmPre.Content.ReadAsStringAsync();
            var administrador = JsonSerializer.Deserialize<AdministradorModelView>(resultAdmPre, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            if(administrador == null)
            {
                throw new Exception($"Não retornou administrador Id = {admId}, verificar o mock.");
            }
            string emailPre = administrador.Email;
            string emailPos = "vend@test.com";

            administrador.Email = emailPos;
            var contentAdmPut = new StringContent(JsonSerializer.Serialize(administrador), Encoding.UTF8, "Application/json");
            var responseAdmPut = await Setup.client.PutAsync($"/administradores/{admId}", contentAdmPut);
            
            var responseAdmPos = await Setup.client.GetAsync($"/administradores/{admId}");
            var resultAdmPos = await responseAdmPos.Content.ReadAsStringAsync();
            var administradorPos = JsonSerializer.Deserialize<AdministradorModelView>(resultAdmPos, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseAdmPut.StatusCode);
            Assert.AreEqual("vendedor@test.com", emailPre);
            Assert.AreEqual(administradorPos != null ? administradorPos.Email : "", emailPos);
        }

        [TestMethod]
        public async Task AdministradorDeleteTest()
        {
            // Arrange
            var response = await this.DoLoginAdministrador();
            var result = await response.Content.ReadAsStringAsync();
            var administradorLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administradorLogado?.Token);

            // Act
            var responseGetAll = await Setup.client.GetAsync("/administradores?pagina=1");
            var resultGetAll = await responseGetAll.Content.ReadAsStringAsync();
            var admsJson = JsonSerializer.Deserialize<List<AdministradorModelView>>(resultGetAll, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            int qtdeBefore = admsJson != null ? admsJson.Count() : 0;

            var responseDelete = await Setup.client.DeleteAsync("/administradores/2");
            
            var responseGetAll2 = await Setup.client.GetAsync("/administradores?pagina=1");
            var resultGetAll2 = await responseGetAll2.Content.ReadAsStringAsync();
            var admsJson2 = JsonSerializer.Deserialize<List<AdministradorModelView>>(resultGetAll2, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });
            int qtdeAfter = admsJson2 != null ? admsJson2.Count() : 0;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, responseDelete.StatusCode);
            Assert.AreEqual(qtdeBefore - 1, qtdeAfter);
        }
    }
}