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

        public ActionResult ProductByCategory(string cateId, List<string> ColorId, List<string> sizeId)
        {
            var items = db.Products.ToList(); 
            return PartialView("_ProductByCategory", items);
        }
    }
}