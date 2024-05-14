using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("Size")]
    public class Size
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Size()
        {
            Id = IdGenerator.GetId(db.Sizes);
            this.VariantSizes = new List<VariantSize>();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Name { get; set; }
        public int Order { get; set; } //Thứ tự của size
        public virtual ICollection<VariantSize> VariantSizes { get; set; }
        
    }
}