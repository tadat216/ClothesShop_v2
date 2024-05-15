using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ProductCategories
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NavBarProductCategory()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_NavBarProductCategory", items);
        }
    }
}