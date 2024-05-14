using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;
using ClothesShop.Models.ViewModel;
using Microsoft.Ajax.Utilities;

namespace ClothesShop.Areas.Admin.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/ProductCategories
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/ProductCategories/Details/5
        public ActionResult Details(string id)
        {
            if (id == string.Empty)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCategory productCategory = db.ProductCategories.Find(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            return View(productCategory);
        }
        // GET: Admin/ProductCategories/Create
        public ActionResult Create(string Id)
        {
            ProductCategoryViewModel productCategoryVM = new ProductCategoryViewModel();
            productCategoryVM.IdParent = Id;
            return View(productCategoryVM);
        }

        // POST: Admin/ProductCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                ProductCategory item = new ProductCategory();
                item.Alias = (model.Alias == null ? Models.Common.Filter.FilterChar(model.Title) : model.Alias);
                item.Title = model.Title;
                item.Description = model.Description;
                item.IdParent = model.IdParent;
                if(model.IdParent == null)
                {
                    item.Level = 0;
                }
                else
                {
                    item.Level = db.ProductCategories.Find(item.IdParent).Level + 1;
                }    
                item.IsActive = 1;
                db.ProductCategories.Add(item);
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Thêm mới mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult DetailPartialChildren(string id)
        {
            var item = db.ProductCategories.Where(x => x.IdParent == id && x.IsActive != 0);
            return PartialView(item);
        }

        public ActionResult GetChildren(string parentId)
        {
            var items = db.ProductCategories.Where(x => x.IdParent == parentId && x.IsActive != 0);
            return PartialView("_ChildCategories", items);
        }

        // GET: Admin/ProductCategories/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == string.Empty)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index"); 
            }
            ProductCategory productCategory = db.ProductCategories.Find(id);
            if (productCategory == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index"); 
            }
            return View(productCategory);
        }

        // POST: Admin/ProductCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                db.ProductCategories.Attach(productCategory);
                productCategory.Alias = ClothesShop.Models.Common.Filter.FilterChar(productCategory.Title);
                productCategory.IdParent = productCategory.Id;
                db.Entry(productCategory).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Cập nhật mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(productCategory);
        }
      
        public ActionResult Delete(string id)
        {
            ProductCategory productCategory = db.ProductCategories.Find(id);
            db.ProductCategories.Attach(productCategory);
            productCategory.IsActive = 0;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public ActionResult SetIsActive(string id)
        {
            if (id == string.Empty)
            {
                TempData["message"] = new XMessage("danger", "Thay đổi trạng thái thất bại");
                return RedirectToAction("Index");
            }
            
            ProductCategory category = db.ProductCategories.Find(id);
            
            if (category == null)
            {
                TempData["message"] = new XMessage("danger", "Thay đổi trạng thái thất bại");
                return RedirectToAction("Index");
            }
            category.IsActive = (category.IsActive == 1) ? 2 : 1;
            db.Entry(category).State = EntityState.Modified;
            db.SaveChanges();
            TempData["message"] = new XMessage("success", "Thay đổi status thành công");
            return RedirectToAction("Index");

        }
        //Trash 
        public ActionResult Trash()
        {
            return View(db.ProductCategories.Where(c => c.IsActive == 0).ToList());
        }
        public ActionResult Undo(int? id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("danger", "Không tìm thấy mẫu tin");
                return RedirectToAction("Index");
            }
            ProductCategory productCategory = db.ProductCategories.Find(id);
            if (productCategory == null)
            {
                TempData["message"] = new XMessage("danger", "Không tìm thấy mẫu tin");
                return RedirectToAction("Index");
            }

            productCategory.IsActive = 1;
            db.SaveChanges();
            TempData["message"] = new XMessage("success", "Phục hồi mẫu tin thành công");
            return RedirectToAction("Index");
        }
    }
}
