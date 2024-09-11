using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
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
        public async Task DesconhecidoBuscaVeiculos()
        {
            // Act
            var response = await Setup.client.GetAsync("/veiculos");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task DesconhecidoBuscaVeiculosPorId()
        {
            // Arrange
            var id = 1;

            // Act
            var response = await Setup.client.GetAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task DesconhecidoAdicionaVeiculo()
        {
            // Arrange
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/veiculos", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task DesconhecidoAtualizaVeiculos()
        {
            // Arrange
            var id = 1;
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PutAsync($"/veiculos/{id}", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task DesconhecidoRemoveVeiculos()
        {
            // Arrange
            var id = 1;

            // Act
            var response = await Setup.client.DeleteAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }
        #endregion

        #region Editor
        [TestMethod]
        public async Task EditorBuscaVeiculos()
        {
            // Arrange
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync("/veiculos");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var veiculos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Veiculo>>(responseData);
            Console.WriteLine(veiculos);
            Assert.AreEqual(2, veiculos?.Count);
        }

        [TestMethod]
        public async Task EditorBuscaVeiculosPorId()
        {
            var id = 1;
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(veiculo);
            Assert.AreEqual(1, veiculo?.Id);
            Assert.AreEqual("Civic", veiculo?.Nome);
            Assert.AreEqual("Honda", veiculo?.Marca);
            Assert.AreEqual(2023, veiculo?.Ano);
        }

        [TestMethod]
        public async Task EditorAdicionaVeiculo()
        {
            // Arrange
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PostAsync("/veiculos", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var veiculoResult = await response.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(veiculoResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(veiculo);
            Assert.AreEqual(3, veiculo?.Id);
            Assert.AreEqual("Uno", veiculo?.Nome);
            Assert.AreEqual("Fiat", veiculo?.Marca);
            Assert.AreEqual(2021, veiculo?.Ano);
        }

        [TestMethod]
        public async Task EditorAtualizaVeiculos()
        {
            // Arrange
            var id = 1;
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");

            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PutAsync($"/veiculos/{id}", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }

        [TestMethod]
        public async Task EditorRemoveVeiculos()
        {
            // Arrange
            var id = 1;
            var token = await LogarComoEditor();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.DeleteAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseData);
            Assert.AreEqual("", responseData);
        }
        #endregion

        #region Administrador
        [TestMethod]
        public async Task AdministradorBuscaVeiculos()
        {
            // Arrange
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Act
            var response = await Setup.client.GetAsync("/veiculos");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var veiculos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Veiculo>>(responseData);
            Console.WriteLine(veiculos);
            Assert.AreEqual(2, veiculos?.Count);
        }

        [TestMethod]
        public async Task AdministradorBuscaVeiculosPorId()
        {
            var token = await LogarComoAdministrador();
            var id = 1;
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.GetAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(veiculo);
            Assert.AreEqual(1, veiculo?.Id);
            Assert.AreEqual("Civic", veiculo?.Nome);
            Assert.AreEqual("Honda", veiculo?.Marca);
            Assert.AreEqual(2023, veiculo?.Ano);
        }

        [TestMethod]
        public async Task AdministradorAdicionaVeiculo()
        {
            // Arrange
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PostAsync("/veiculos", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var veiculoResult = await response.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(veiculoResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(veiculo);
            Assert.AreEqual(3, veiculo?.Id);
            Assert.AreEqual("Uno", veiculo?.Nome);
            Assert.AreEqual("Fiat", veiculo?.Marca);
            Assert.AreEqual(2021, veiculo?.Ano);
        }

        [TestMethod]
        public async Task AdministradorAtualizaVeiculos()
        {
            // Arrange
            var id = 1;
            var veiculoDTO = new VeiculoDTO { Nome = "Uno", Marca = "Fiat", Ano = 2021 };
            var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");

            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.PutAsync($"/veiculos/{id}", veiculoContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var veiculoResult = await response.Content.ReadAsStringAsync();
            var veiculo = JsonSerializer.Deserialize<Veiculo>(veiculoResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(veiculo);
            Assert.AreEqual(1, veiculo?.Id);
            Assert.AreEqual("Uno", veiculo?.Nome);
            Assert.AreEqual("Fiat", veiculo?.Marca);
            Assert.AreEqual(2021, veiculo?.Ano);

            var response2 = await Setup.client.GetAsync("/veiculos");
            var result2 = await response2.Content.ReadAsStringAsync();
            var veiculos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Veiculo>>(result2);
            Console.WriteLine(veiculos);
            Assert.AreEqual(2, veiculos?.Count);
        }

        [TestMethod]
        public async Task AdministradorRemoveVeiculos()
        {
            // Arrange
            var id = 1;
            var token = await LogarComoAdministrador();
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Setup.client.DeleteAsync($"/veiculos/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

            var response2 = await Setup.client.GetAsync("/veiculos");
            var result2 = await response2.Content.ReadAsStringAsync();
            var veiculos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Veiculo>>(result2);
            Console.WriteLine(veiculos);
            Assert.AreEqual(1, veiculos?.Count);
        }
        #endregion

    }
}