using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Phone]
        [Display(Name ="Phone")]
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
    }
}
