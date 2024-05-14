using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ClothesShop.Models.Common;

namespace ClothesShop.Models.EF
{
    [Table("Color")]
    public class Color
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Color()
        {
            this.Id = IdGenerator.GetId(db.Colors);
            this.ProductVariants = new HashSet<ProductVariant>();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(50)]
        public string Alias { get; set; }
        [StringLength(20)]
        public string Code { get; set; }
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
    }
}