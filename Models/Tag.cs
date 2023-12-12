using System;
using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele tagului este obligatoriu")]
        public string TagName { get; set; }

        //public virtual ICollection<Subject>? Subjects { get; set; }
    }
}