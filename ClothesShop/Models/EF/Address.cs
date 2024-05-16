using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("Address")]
    public class Address
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Address(){ this.Id = IdGenerator.GetId(db.Addresses); }
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        [StringLength(100)]
        public string ReceiverName { get; set; }
        public string ReceiverAddress {  get; set; }
        public string ReceiverPhone { get; set; }
        public bool IsDefault { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}