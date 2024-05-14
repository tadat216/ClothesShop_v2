using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace ClothesShop.Models.ViewModel
{
    public class ProductCreateViewModel
    {
        // Product Info

        public string Title { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public string Detail { get; set; }
        public int Price { get; set; }
        public int PriceSale { get; set; }
        public string ProductCategoryId { get; set; }
        public bool IsHome { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }
        public bool IsHot { get; set; }
        public bool IsActive { get; set; } = true;

        // Product Variants
        public List<ProductVariantViewModel> Variants { get; set; }
        // Dropdown Lists
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Colors { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }

    }
}