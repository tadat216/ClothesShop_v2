using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothesShop.Models.Common;

namespace ClothesShop.Models.EF
{
    public class News
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public News()
        {
            this.Id = IdGenerator.GetId(db.Newes);
           
        }
        [Key]
        
        public string Id { get; set; }
        [Required(ErrorMessage = "Tiêu đề tin tức không được để trống!")]
        [StringLength(150)]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        public int ViewCount { get; set; }
        public string Image { get; set; }
        //public string ProductCategoryId { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string SeoKeywords { get; set; }
        public bool IsActive { get; set; }
        //public virtual ProductCategory ProductCategory { get; set; }
        public string CreatedUserId { get; set; }
        public virtual ApplicationUser CreatedUser { get; set; }
        public string ModifiedUserId { get; set; }
        public virtual ApplicationUser ModifiedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate{get; set;}
    }
}