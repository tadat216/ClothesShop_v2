using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;

namespace ClothesShop.Areas.Admin.Controllers
{
    public class ColorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Colors
        public ActionResult Index()
        {
            return View(db.Colors.ToList());
        }

        // GET: Admin/Colors/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // GET: Admin/Colors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Colors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Color color)
        {
            if (ModelState.IsValid)
            {
                color.Alias = ClothesShop.Models.Common.Filter.FilterChar(color.Name);
                db.Colors.Add(color);
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Thêm mới mẫu tin thành công");
                return RedirectToAction("Index");
            }

            return View(color);
        }

        // GET: Admin/Colors/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // POST: Admin/Colors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Color color)
        {
            if (ModelState.IsValid)
            {
                color.Alias = ClothesShop.Models.Common.Filter.FilterChar(color.Name);
                db.Entry(color).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Cập nhật mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Admin/Colors/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // POST: Admin/Colors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Color color = db.Colors.Find(id);
            db.Colors.Remove(color);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
