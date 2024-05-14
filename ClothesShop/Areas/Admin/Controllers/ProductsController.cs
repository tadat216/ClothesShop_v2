using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;
using ClothesShop.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create() 
        {
            var model = new ProductCreateViewModel
            {
                Categories = db.ProductCategories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                }),
                Colors = db.Colors.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name + " #" + c.Code,
                }),
                Sizes = db.Sizes.OrderBy(s => s.Order).Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCreateViewModel model, int? isDefaultVariant)
        {
            if (model.Variants == null || !model.Variants.Any())
            {
                
                model.Categories = db.ProductCategories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                });
                model.Colors = db.Colors.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
                model.Sizes = db.Sizes.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                });
                return View(model);
            }

            if (ModelState.IsValid)
            {
                Product product = new Product();
                product.ProductCategoryId = model.ProductCategoryId;
                product.Title = model.Title;
                product.Price = model.Price;
                product.Description = model.Description;
                product.PriceSale = model.PriceSale;
                


                return RedirectToAction("Index"); // Adjust as needed
            }
            // Re-populate dropdowns if there's a validation error
            model.Categories = db.ProductCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            });
            model.Colors = db.Colors.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
            model.Sizes = db.Sizes.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
            return View(model);
        }
    }
}