using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("VariantSize")]
    public class VariantSize
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public VariantSize() 
        {
            this.Id = IdGenerator.GetId(db.VariantSizes);
        }
        [Key]
        public string Id { get; set; }
        public string ProductVariantId {  get; set; }
        public string SizeId { get; set; }
        public int Amount { get; set; }
        public virtual Size Size { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }
    }
}