using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Areas.Admin.Controllers
{
    
    public class NewsController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/News
        [Authorize(Roles = "Admin, Employee")]
        public ActionResult SearchId(string Searchtext, DateTime? from, DateTime? to, int? page, int? size, string active = "")
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<News> items = db.Newes.OrderByDescending(x => x.Id);
            //int count = items.Count();
            if (!string.IsNullOrEmpty(Searchtext))
                items = items.Where(x =>
                    (x.Description != null && x.Description.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.Detail != null && x.Detail.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.Title != null && x.Title.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0));

            if (from != null)
                items = items.Where(x => x.CreatedDate >= from);
            if (to != null)
                items = items.Where(x => x.ModifiedDate <= to);
            if (active == "yes")
                items = items.Where(x => x.IsActive);
            if (active == "no")
                items = items.Where(x => !x.IsActive);

            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            ViewBag.FromDate = from?.ToString("yyyy/MM/dd");
            ViewBag.ToDate = to?.ToString("yyyy/MM/dd");
            //TempData["id"] = "11";
            return View(items);

        }

        [Authorize(Roles = "Admin, Employee")]
        public ActionResult Index(string Searchtext, DateTime? from, DateTime? to, int? page, int? size, string active = "")
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<News> items = db.Newes.OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items
                   .Where(
                   x => x.Id == Searchtext);

                if (items.Count() == 0)
                {
                    items = db.Newes.OrderByDescending(x => x.Id);
                    items = items.Where(x =>
                        (x.Description != null && x.Description.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (x.Detail != null && x.Detail.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (x.Title != null && x.Title.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0));
                }

            }
            if (active=="yes")
                items = items.Where(x => x.IsActive );
            if (active == "no")
                items = items.Where(x => !x.IsActive);

            if (from != null)
                items = items.Where(x => x.CreatedDate >= from);
            if (to != null)
                items = items.Where(x => x.ModifiedDate <= to);
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            return View(items);


        }
        [HttpPost]
        [Authorize(Roles = "Admin, Employee")]
        public void SetTempData(string data)
        {
            TempData["id"] = data;
        }

        [Authorize(Roles = "Admin, Employee")]
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(News news)
        {
            if (ModelState.IsValid)
            {
                news.CreatedUserId = User.Identity.GetUserId();
                news.CreatedDate = DateTime.Now;
                news.ModifiedDate = DateTime.Now;
                news.Alias = ClothesShop.Models.Common.Filter.FilterChar(news.Title);
                db.Newes.Add(news);
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Thêm mới mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(news);
        }
        [Authorize(Roles = "Admin, Employee")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("success", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }

            var item = db.Newes.Find(id);
            if (item == null)
            {
                TempData["message"] = new XMessage("success", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News news)
        {
            if (ModelState.IsValid)
            {
                news.ModifiedDate = DateTime.Now;
                news.ModifiedUserId = User.Identity.GetUserId();

                news.Alias = ClothesShop.Models.Common.Filter.FilterChar(news.Title);
                db.Newes.Attach(news);
                db.Entry(news).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Chỉnh sửa mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(news);
        }

        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var item = db.Newes.Find(id);
            if (item != null)
            {
                db.Newes.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [Authorize(Roles = "Admin, Employee")]
        public ActionResult Detail(String id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            News news = db.Newes.Find(id);
            if (news == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            ViewBag.CreatedUser = db.Users.FirstOrDefault(x => x.Id == news.CreatedUserId).UserName;
            ViewBag.ModifiedUser = db.Users.FirstOrDefault(x => x.Id == news.ModifiedUserId).UserName;
            return View(news);

        }

        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        public ActionResult IsActive(string id)
        {
            var item = db.Newes.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }
            return Json(new { success = false });
        }
    }
}