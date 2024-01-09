using Microsoft.AspNetCore.Identity;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NumeComplet { get; set; }

        public string? Rol { get; set; }

        public byte[]? PozaProfil { get; set; }
    }
}
