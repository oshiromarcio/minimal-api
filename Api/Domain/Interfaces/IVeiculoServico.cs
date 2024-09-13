using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaPorId(int id);

        Veiculo Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void Apagar(Veiculo veiculo);
    }
}