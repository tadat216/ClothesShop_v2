using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public OrderDetail() { Id = IdGenerator.GetId(db.OrderDetails);  }
        [Key]
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string VariantSizeId { get; set; }
        public int Price { get; set; }  
        public int Quantity { get; set; }
        public virtual Order Order { get; set; }
        public virtual VariantSize VariantSize { get; set; }
    }
}