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
    [Table("Rate")]
    public class Rate
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Rate() { Id = IdGenerator.GetId(db.Rates); Comment = ""; }
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ProductVariantId { get; set; }
        public float? RateValue { get; set; }
        [StringLength (1000)]
        public string Comment { get; set; }
        public bool Rated { get; set; } = false;
        public bool CanRate { get; set; } = false;
        public DateTime? RatedDate { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }
    }
}