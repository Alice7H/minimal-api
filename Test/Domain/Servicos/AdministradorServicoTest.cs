using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            var configuration = builder.Build();
            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestarSalvarAdministrador()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(contexto);

            // Act
            administradorServico.Incluir(adm);
            // Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestarBuscarAdministradorPorId()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(contexto);

            // Act
            administradorServico.Incluir(adm);
            var admEncontrado = administradorServico.BuscaPorId(adm.Id);

            // Assert
            Assert.AreEqual(1, admEncontrado?.Id);
        }

        [TestMethod]
        public void TestarBuscarTodosAdministradores()
        {
            // Arrange
            var contexto = CriarContextoDeTeste();
            contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            var adm2 = new Administrador();
            adm2.Id = 2;
            adm2.Email = "fulano@teste.com";
            adm2.Senha = "fulano123";
            adm2.Perfil = "Editor";

            var administradorServico = new AdministradorServico(contexto);

            // Act
            administradorServico.Incluir(adm);
            administradorServico.Incluir(adm2);
            var admsEncontrados = administradorServico.Todos(1);

            // Assert
            Assert.AreEqual(2, admsEncontrados.Count);
        }
    }
}