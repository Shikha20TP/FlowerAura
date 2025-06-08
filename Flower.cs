using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerAura.Models
{
    public class Flowers
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Flower name is required")]
        [StringLength(100, ErrorMessage = "Flower name cannot exceed 100 characters")]
        public string FlowerName { get; set; }

        
        public string? Image { get; set; }

        
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 100000, ErrorMessage = "Amount must be between 1 and 100000")]
        public string Amount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
    }
}