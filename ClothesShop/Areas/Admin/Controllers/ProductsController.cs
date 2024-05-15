using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;
using ClothesShop.Models.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Web.UI;

namespace ClothesShop.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products

        public ActionResult Index(string id, string title, string categoryId, string[] colorIds, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            // Lấy danh sách danh mục và màu sắc để hiển thị trên form
            var categories = db.ProductCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            }).ToList();

            var colors = db.Colors.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Code
            }).ToList();

            IEnumerable<Product> products = db.Products.ToList();

            if (!string.IsNullOrEmpty(id))
            {
                products = products.Where(p => p.Id == id);
            }

            // Filtering by Title
            if (!string.IsNullOrEmpty(title))
            {
                products = products.Where(p => p.Title.Contains(title));
            }

            // Lọc sản phẩm dựa trên các tiêu chí đã chọn
            if (!string.IsNullOrEmpty(categoryId))
            {
                products = products.Where(p => p.ProductCategoryId == categoryId || p.ProductCategory.IdParent == categoryId);
            }

            if (colorIds != null && colorIds.Length > 0)
            {
                if (colorIds.Length == 1 && colorIds[0].Contains(","))
                {
                    colorIds = colorIds[0].Split(',');
                }
                products = products.Where(p => p.ProductVariants.Any(v => colorIds.Contains(v.ColorId)));
            }

            // Lưu giá trị hiện tại đã chọn vào ViewBag để gửi trở lại view
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedColorIds = colorIds;
            ViewBag.Categories = categories;
            ViewBag.Id = id;
            ViewBag.PTitle = title;
            ViewBag.Colors = colors;

            return View(products.ToPagedList(pageNumber, pageSize));
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
                    Text = c.Code
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
                product.IsSale = model.IsSale;
                product.IsHot = model.IsHot;
                product.IsActive = model.IsActive;
                product.IsFeature = model.IsFeature;
                product.Detail = model.Detail;
                product.Alias = Models.Common.Filter.FilterChar(product.Title);
                product.ProductCode = product.Id;

                for(int i = 0; i < model.Variants.Count(); i++)
                {
                    var variant = model.Variants[i];
                    ProductVariant productVariant = new ProductVariant();
                    productVariant.ProductId = product.Id;
                    productVariant.ColorId = variant.ColorId;
                    productVariant.IsDefault = (isDefaultVariant == i);
                    for (int j = 0; j < variant.Images.Count(); j++)
                    {
                        string url = variant.Images[j];
                        ImageList imageList = new ImageList();
                        imageList.ProductVariantId = productVariant.Id;
                        imageList.ImageUrl = url;
                        if(j == 0) imageList.IsDefault = true;
                        else imageList.IsDefault = false;
                        db.ImageLists.Add(imageList);
                    }
                    for(int j = 0; j < variant.SizeId.Count(); j++) { 
                        string sizeId = variant.SizeId[j];
                        int amount = variant.Amount[j];
                        VariantSize variantSize = new VariantSize();
                        variantSize.ProductVariantId = productVariant.Id;
                        variantSize.SizeId = sizeId;
                        variantSize.Amount = amount;
                        db.VariantSizes.Add(variantSize);
                    }
                    db.ProductVariants.Add(productVariant);
                }
                db.Products.Add(product);
                db.SaveChanges();

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