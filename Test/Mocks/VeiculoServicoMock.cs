using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;

namespace Test.Mocks
{
    public class VeiculoServicoMock : IVeiculoServico
    {
        private static List<Veiculo> veiculos = new List<Veiculo>(){
            new Veiculo {
                Id = 1,
                Nome = "Civic",
                Marca = "Honda",
                Ano = 2023,
            },
              new Veiculo {
                Id = 2,
                Nome = "Corolla",
                Marca = "Toyota",
                Ano = 2024,
            }
        };

        public void Apagar(Veiculo veiculo)
        {
            var vehicle = veiculos.Find(v => v.Id == veiculo.Id);
            if (vehicle != null) veiculos.Remove(veiculo);
        }

        public void Atualizar(Veiculo veiculo)
        {
            var vehicle = veiculos.Find(v => v.Id == veiculo.Id);
            if (vehicle != null)
            {
                vehicle.Id = veiculo.Id;
                vehicle.Nome = veiculo.Nome;
                vehicle.Marca = veiculo.Marca;
                vehicle.Ano = veiculo.Ano;
            }
        }

        public Veiculo? BuscaPorId(int id)
        {
            return veiculos.Find(a => a.Id == id);
        }

        public void Incluir(Veiculo veiculo)
        {
            veiculo.Id = veiculos.Count() + 1;
            veiculos.Add(veiculo);
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var lista = new List<Veiculo>();
            if (nome == null && marca == null) lista.AddRange(veiculos);
            if (!string.IsNullOrEmpty(nome)) lista.AddRange(veiculos.FindAll(v => v.Nome.Contains(nome)));
            if (!string.IsNullOrEmpty(marca)) lista.AddRange(veiculos.FindAll(v => v.Marca.Contains(marca)));
            return lista;
        }

    }
}