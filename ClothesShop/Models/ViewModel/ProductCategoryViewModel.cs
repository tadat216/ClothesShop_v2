using ClothesShop.Models.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.ViewModel
{
    public class ProductCategoryViewModel
    {
        public string IdParent { get; set; }
        [Required]
        [StringLength(150, ErrorMessage = "Tên danh mục sản phẩm không được để trống")]
        public string Title { get; set; }
        [StringLength(150)]
        public string Alias { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
    }
}