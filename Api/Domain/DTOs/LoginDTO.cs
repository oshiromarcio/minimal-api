using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Domain.DTOs
{
    public class LoginDTO
    {
            public required string Email { get; set; }
            public required string Senha { get; set; }
    }
}