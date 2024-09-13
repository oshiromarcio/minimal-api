using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Enums;

namespace minimal_api.Domain.DTOs
{
    public class AdministradorDTO
    {
        public required string Email { get; set; } = default!;

        public required string Senha { get; set; } = default!;

        public required Perfil Perfil { get; set; } = default!;
    }
}