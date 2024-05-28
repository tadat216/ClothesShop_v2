using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Models.EF
{
    [Table("Product")]
    public class Product
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Product() 
        {
            Id = IdGenerator.GetId(db.Products);
            ProductVariants = new HashSet<ProductVariant>();
        }
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống!")]
        [StringLength(250)]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        [Required]
        [StringLength(250)]
        public string ProductCode { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        public int Price { get; set; }
        public int PriceSale { get; set; }
        public bool IsHome { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }
        public bool IsHot { get; set; }
        public string ProductCategoryId { get; set; }
        public bool IsActive { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        
        public int CalStar() {
            float val = db.Rates.Average(x => x.RateValue)??0;
            return (int)Math.Ceiling(val);
        }
    }
}