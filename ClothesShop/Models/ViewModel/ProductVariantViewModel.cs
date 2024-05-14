using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.ViewModel
{
    public class ProductVariantViewModel
    {
        public string ColorId { get; set; }
        public List<string> SizeId { get; set; }
        public List<int> Amount { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Images { get; set; }
    }
}