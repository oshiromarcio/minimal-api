using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Enums;

namespace minimal_api.Domain.ModelViews
{
    public record AdministradorModelView
    {
        public required int Id { get; set; } = default!;

        public required string Email { get; set; } = default!;

        public required string Perfil { get; set; } = default!;
    }
}