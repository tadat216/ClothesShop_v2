using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ClothesShop.Models.Common;

namespace ClothesShop.Models.EF
{
    [Table("ImageList")]
    public class ImageList
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ImageList() { this.Id = IdGenerator.GetId(db.ImageLists);  }
        [Key]
        public string Id { get; set; }
        public string ProductVariantId { get; set; }
        public bool IsDefault { get; set; }
        public string ImageUrl { get; set; }

        // Navigation property
        public virtual ProductVariant ProductVariant { get; set; }
    }
}