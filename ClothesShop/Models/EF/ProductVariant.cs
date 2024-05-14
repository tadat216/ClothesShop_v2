using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("ProductVariant")]
    public class ProductVariant
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ProductVariant() 
        { 
            Id = IdGenerator.GetId(db.ProductVariants);
            this.VariantSizes = new List<VariantSize>();
            this.ImageLists = new List<ImageList>();    
        }
        [Key]
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ColorId { get; set; }
        public bool IsDefault { get; set; } //la bien the duoc hien thi o 
        public virtual Product Product { get; set; }
        public virtual Color Color { get; set; }
        public virtual ICollection<VariantSize> VariantSizes { get; set; }
        public virtual ICollection<ImageList> ImageLists { get; set; }

    }
}