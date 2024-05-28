using ClothesShop.Models;
using PagedList;
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

        public ActionResult ProductRating(int? page, string ProductId)
        {
            var rates = db.Rates.Where(x => x.ProductVariant.ProductId.Equals(ProductId)).ToList();
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            ViewBag.productId = ProductId;
            return PartialView("_ProductRating", rates.ToPagedList(pageNumber, pageSize));
        }
    }
}