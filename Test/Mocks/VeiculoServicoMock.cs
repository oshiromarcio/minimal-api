using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;

namespace Test.Mocks
{
    public class VeiculoServicoMock : IVeiculoServico
    {
        private static List<Veiculo> veiculos = new List<Veiculo>() {
            new Veiculo {
                Id = 1,
                Nome = "Passat",
                Marca = "Volkswagen",
                Ano = 1982
            },
            new Veiculo {
                Id = 2,
                Nome = "Corolla",
                Marca = "Toyota",
                Ano = 2022
            }
        };

        public void Apagar(Veiculo veiculo)
        {
            veiculos.Remove(veiculo);
        }

        public void Atualizar(Veiculo veiculo)
        {
            var index = veiculos.FindIndex(a => a.Id == veiculo.Id);
            veiculos[index] = veiculo;
        }

        public Veiculo? BuscaPorId(int id)
        {
            return veiculos.Find(a => a.Id == id);
        }

        public Veiculo Incluir(Veiculo veiculo)
        {
            veiculo.Id = veiculos.Count() + 1;
            veiculos.Add(veiculo);
            return veiculo;
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            return veiculos;
        }
    }
}