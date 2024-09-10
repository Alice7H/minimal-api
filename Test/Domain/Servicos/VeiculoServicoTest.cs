using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class VeiculoServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            // configurar o CofigurationBuilder
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            var configuration = builder.Build();
            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestarSalvarVeiculo()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos"); // <- limpando a tabela.

            var veiculo = new Veiculo();
            veiculo.Id = 1;
            veiculo.Nome = "Accord";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2024;

            var veiculoServico = new VeiculoServico(contexto);

            // Act
            veiculoServico.Incluir(veiculo);

            // Assert
            Assert.AreEqual(1, veiculoServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestarBuscarVeiculoPorId()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos"); // <- limpando a tabela.

            var veiculo = new Veiculo();
            veiculo.Id = 1;
            veiculo.Nome = "Accord";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2024;

            var veiculoServico = new VeiculoServico(contexto);
            veiculoServico.Incluir(veiculo);

            // Act
            var veiculoEncontrado = veiculoServico.BuscaPorId(veiculo.Id);

            // Assert
            Assert.AreEqual(1, veiculoEncontrado?.Id);
        }

        [TestMethod]
        public void TestarBuscarTodosVeiculos()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos"); // <- limpando a tabela.

            var veiculo1 = new Veiculo();
            veiculo1.Id = 1;
            veiculo1.Nome = "Accord";
            veiculo1.Marca = "Honda";
            veiculo1.Ano = 2024;

            var veiculo2 = new Veiculo();
            veiculo2.Id = 2;
            veiculo2.Nome = "Q5";
            veiculo2.Marca = "Audi";
            veiculo2.Ano = 2023;

            var veiculoServico = new VeiculoServico(contexto);
            veiculoServico.Incluir(veiculo1);
            veiculoServico.Incluir(veiculo2);

            // Act
            var veiculosEncontrados = veiculoServico.Todos(1);

            // Assert
            Assert.AreEqual(2, veiculosEncontrados.Count);
        }

        [TestMethod]
        public void TestarAtualizarVeiculo()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos"); // <- limpando a tabela.

            var veiculo = new Veiculo();
            veiculo.Id = 1;
            veiculo.Nome = "Accord";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2024;

            var veiculoServico = new VeiculoServico(contexto);
            veiculoServico.Incluir(veiculo);

            // Act
            var veiculoEncontrado = veiculoServico.BuscaPorId(veiculo.Id);
            if (veiculoEncontrado != null)
            {
                veiculoEncontrado.Nome = "308";
                veiculoEncontrado.Marca = "Peugeot";
                veiculoEncontrado.Ano = 2022;
                veiculoServico.Atualizar(veiculoEncontrado);
            }

            // Assert
            Assert.IsNotNull(veiculoEncontrado);
            var veiculoAtualizado = veiculoServico.BuscaPorId(veiculoEncontrado.Id);
            Assert.IsNotNull(veiculoAtualizado);
            Assert.AreEqual("308", veiculoAtualizado.Nome);
            Assert.AreEqual("Peugeot", veiculoAtualizado.Marca);
            Assert.AreEqual(2022, veiculoAtualizado.Ano);
        }

        [TestMethod]
        public void TestarRemoverVeiculo()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos"); // <- limpando a tabela.

            var veiculo = new Veiculo();
            veiculo.Id = 1;
            veiculo.Nome = "Accord";
            veiculo.Marca = "Honda";
            veiculo.Ano = 2024;

            var veiculoServico = new VeiculoServico(contexto);
            veiculoServico.Incluir(veiculo);

            // Act
            var veiculoEncontrado = veiculoServico.BuscaPorId(veiculo.Id);
            if (veiculoEncontrado != null) veiculoServico.Apagar(veiculoEncontrado);

            // Assert
            Assert.AreEqual(0, veiculoServico.Todos().Count);
        }
    }
}