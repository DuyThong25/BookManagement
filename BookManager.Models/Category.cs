using Newtonsoft.Json.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookManager.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng không được để trống")]
        [MaxLength(30, ErrorMessage = "Không được quá 30 ký tự")]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Giá trị phù hợp từ 0 đến 100")]
        public int? DisplayOrder { get; set; }

    }
}
