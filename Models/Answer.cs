using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Proiect.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul raspunsului este obligatoriu")]
        [StringLength(700, ErrorMessage = "Raspunsul nu poate contine mai mult de 700 de caractere")]
        [MinLength(10, ErrorMessage = "Raspunsul trebuie sa aiba mai mult de 10 caractere")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        //public virtual ICollection<Tag>? Tags { get; set; }

        public int? SubjectId { get; set; }

        public virtual Subject? Subject { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } // un comentariu apartine unui singur utilizator
    }
}