using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index(string id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return HttpNotFound("Product not found");
            }

            return View(product);
        }

        public ActionResult ProductRating(string ProductId)
        {
            var rates = db.Rates.Where(x => x.ProductVariant.ProductId.Equals(ProductId)).ToList();
            return PartialView("_ProductRating", rates);
        }
    }
}