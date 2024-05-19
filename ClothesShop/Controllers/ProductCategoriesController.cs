using ClothesShop.Models;
using ClothesShop.Models.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ProductCategories
        public ActionResult Index(string categoryId, string colorIds, string sizeIds)
        {
            CategoryPageViewModel item = new CategoryPageViewModel();
            item.size = db.Sizes.ToList();
            if (String.IsNullOrEmpty(categoryId))
            {
                item.productCategory = db.ProductCategories.ToList();
            }
            item.color = db.Colors.ToList();
            return View(item);
        }

        public ActionResult NavBarProductCategory()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_NavBarProductCategory", items);
        }
        //[HttpGet]
        public ActionResult ProductByCategory(string cateId, string[] colorIds, string[] sizeIds, int priceMin=0, int priceMax=1000000000)
        {
            var pd = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(cateId))
            {
                pd = pd.Where(p => p.ProductCategoryId == cateId || p.ProductCategory.IdParent == cateId);
            }
            pd = pd.Where(p => (p.IsSale ? p.PriceSale : p.Price) >= priceMin);
            pd = pd.Where(p => (p.IsSale ? p.PriceSale : p.Price) <= priceMax);
            var variant = db.ProductVariants.AsQueryable();
            if (colorIds != null && colorIds.Any())
            {
                variant = variant.Where(p => colorIds.Contains(p.ColorId));
            }
            if (sizeIds != null && sizeIds.Any())
            {
                variant = variant.Where(p => p.VariantSizes.Any(v => sizeIds.Contains(v.SizeId)));
            }
            pd = pd.Where(p => variant.Any(v => v.ProductId == p.Id));
            return PartialView("_ProductByCategory", pd);
        }
    }
}