using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("CartDetail")]
    public class CartDetail
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public CartDetail() 
        {
            Id = IdGenerator.GetId(db.CartDetails);
        }
        [Key]
        public string Id { get; set; }
        public string CartId { get; set; }
        public string VariantSizeId { get; set; }
        public int Quantity { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual VariantSize VariantSize { get; set; }
    }
}