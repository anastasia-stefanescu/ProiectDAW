using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NumeComplet { get; set; }

        public string? Rol { get; set; }

        public byte[]? PozaProfil { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}