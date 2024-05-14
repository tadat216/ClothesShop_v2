using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("Order")]
    public class Order
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Order() 
        {
            this.Id = IdGenerator.GetId(db.Orders);
            OrderDetails = new HashSet<OrderDetail>();
        }
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsPaid { get; set; }
        public bool IsVerified { get; set; }
        public string PaymentMethodId { get; set; }
        public DateTime OrderedDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}