using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerAura.Models
{
   
        public class FlowerViewModel
        {
        public int Id { get; set; }
        public string FlowerName { get; set; }
        public string Image { get; set; }  
        public string Category { get; set; }
        public string Amount { get; set; }

        [Column(TypeName = "NVARCHAR(MAX)")] 
        public string Description { get; set; }
    }
    }
