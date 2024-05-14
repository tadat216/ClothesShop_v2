using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("Cart")]
    public class Cart
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Cart()
        {
            this.Id = IdGenerator.GetId(db.Carts);
            CartDetails = new HashSet<CartDetail>();
        }
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<CartDetail> CartDetails { get; set; }
    }

}