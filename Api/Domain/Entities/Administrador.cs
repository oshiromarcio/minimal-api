using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Enums;

namespace minimal_api.Domain.Entities
{
    public class Administrador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string Email { get; set; }

        [Required]
        [StringLength(50)]
        public required string Senha { get; set; }

        [Required]
        [StringLength(10)]
        public required string Perfil { get; set; }
    }
}