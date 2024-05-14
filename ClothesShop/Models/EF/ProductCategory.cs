using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ClothesShop.Models.Common;

namespace ClothesShop.Models.EF
{
    [Table("ProductCategory")]
    public class ProductCategory
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ProductCategory()
        {
            Id = IdGenerator.GetId(db.ProductCategories);
            this.Products = new HashSet<Product>();
            this.ProductCategories = new HashSet<ProductCategory>();
        }
        [Key]
        public string Id { get; set; }
        public string IdParent { get; set; }
        [Required]
        [StringLength(150, ErrorMessage = "Tên danh mục sản phẩm không được để trống")]
        public string Title { get; set; }
        [StringLength(150)]
        public string Alias { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int Level { get; set; }
        public int IsActive { get; set; }
        //0:xóa, 1: active, 2: ko active
        public ICollection<Product> Products { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set;}
        [ForeignKey("IdParent")]
        public virtual ProductCategory ProductCategoryParent { get; set; }
    }
}