using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("PaymentMethod")]
    public class PaymentMethod
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public PaymentMethod() 
        {
            this.Id = IdGenerator.GetId(db.PaymentMethods);
            Orders = new HashSet<Order>();            
        }
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
    }
}