using System.ComponentModel.DataAnnotations;

namespace FlowerAura.Models
{
    public class FileUploadViewModel
    {
        internal string FlowerName;
        internal string Image;
        internal decimal Amount;
        internal string Category;
        internal string Description;

        [Required]
        public IFormFile CsvFile { get; set; }
    }
}
