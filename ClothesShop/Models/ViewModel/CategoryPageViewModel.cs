using ClothesShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.ViewModel
{
    public class CategoryPageViewModel
    {
        public List<ProductCategory> productCategory { get; set; }
        public List<Color> color { get; set; }
        public List<Size> size { get; set; }
    }
}