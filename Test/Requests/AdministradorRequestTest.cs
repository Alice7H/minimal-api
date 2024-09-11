using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Routing.Constraints;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.ModelViews;
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

        private async Task<string?> PegarToken(LoginDTO loginDTO)
        {
            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");
            var responseLogin = await Setup.client.PostAsync("/administradores/login", content);
            var result = await responseLogin.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return admLogado?.Token;
        }

        private async Task<string?> LogarComoAdministrador()
        {
            return await PegarToken(new LoginDTO { Email = "adm@teste.com", Senha = "123456" });
        }

        private async Task<string?> LogarComoEditor()
        {
            return await PegarToken(new LoginDTO { Email = "editor@teste.com", Senha = "editor1234" });
        }

        #region Unknown
        [TestMethod]
        public async Task TestarLogin()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");

            Console.WriteLine(admLogado?.Token);
        }

        [TestMethod]
        public async Task DesconhecidoBuscaTodosAdministradores()
        {
            // Act
            var response = await Setup.client.GetAsync("/administradores");
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DesconhecidoBuscaAdministradorPorId()
        {
            // Arrange
            var id = 1;

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task DesconhecidoIncluiAdministrador()
        {
            // Arrange
            var admDTO = new AdministradorDTO { Email = "editor2@teste.com", Senha = "editor321", Perfil = Perfil.Editor };
            var content = new StringContent(JsonSerializer.Serialize(admDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            Assert.AreEqual("", result);
        }
        #endregion

        #region Editor
        [TestMethod]
        public async Task EditorBuscaTodosAdministradores()
        {
            // Arrange
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync("/administradores");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task EditorBuscaAdministradorPorId()
        {
            // Arrange
            var id = 1;
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task EditorIncluiAdministrador()
        {
            // Arrange
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var admDTO = new AdministradorDTO { Email = "editor2@teste.com", Senha = "editor321", Perfil = Perfil.Editor };
            var admContent = new StringContent(JsonSerializer.Serialize(admDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores", admContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var admResult = await response.Content.ReadAsStringAsync();
            var adm = JsonSerializer.Deserialize<AdministradorModelView>(admResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(adm);
            Assert.AreEqual(3, adm?.Id);
            Assert.AreEqual("editor2@teste.com", adm?.Email);
            Assert.AreEqual("Editor", adm?.Perfil);
        }
        #endregion

        #region Administrador
        [TestMethod]
        public async Task AdministradorBuscaTodosAdministradores()
        {
            // Arrange
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync("/administradores");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var adms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdministradorModelView>>(responseData);
            Assert.AreEqual(2, adms?.Count);
            Console.WriteLine(adms);
        }

        [TestMethod]
        public async Task AdministradorBuscaAdministradorPorId()
        {
            // Arrange
            var id = 1;
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var adms = JsonSerializer.Deserialize<AdministradorModelView>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(adms);
            Assert.AreEqual(1, adms?.Id);
            Assert.AreEqual("Adm", adms?.Perfil);
            Assert.AreEqual("adm@teste.com", adms?.Email);
        }

        [TestMethod]
        public async Task AdministradorBuscaAdmPorIdSemRetorno()
        {
            // Arrange
            var id = 10;
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task AdministradorIncluiAdministrador()
        {
            // Arrange
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var admDTO = new AdministradorDTO { Email = "editor2@teste.com", Senha = "editor321", Perfil = Perfil.Editor };
            var admContent = new StringContent(JsonSerializer.Serialize(admDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores", admContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var admResult = await response.Content.ReadAsStringAsync();
            var adm = JsonSerializer.Deserialize<AdministradorModelView>(admResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(adm);
            Assert.AreEqual(3, adm?.Id);
            Assert.AreEqual("editor2@teste.com", adm?.Email);
            Assert.AreEqual("Editor", adm?.Perfil);
        }

        [TestMethod]
        public async Task AdministradorIncluiAdministradorSemEmailESenha()
        {
            // Arrange
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var admDTO = new AdministradorDTO { Email = "", Senha = "", Perfil = Perfil.Editor };
            var admContent = new StringContent(JsonSerializer.Serialize(admDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores", admContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var admResult = await response.Content.ReadAsStringAsync();
            var erros = JsonSerializer.Deserialize<ErrosDeValidacao>(admResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(2, erros.Mensagens.Count);
            Console.WriteLine(erros);
        }
        #endregion

    }
}