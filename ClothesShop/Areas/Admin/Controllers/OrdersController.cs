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
using PagedList;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Orders

        public ActionResult SearchId(string username, string fullname, string email, string phone, int? page, int? size)
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<ApplicationUser> items = db.Users.OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(username))
                items = items.Where(x =>
                    (x.Id != null && x.Id.IndexOf(username, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.UserName != null && x.UserName.IndexOf(username, StringComparison.OrdinalIgnoreCase) >= 0));
            if (!string.IsNullOrEmpty(fullname))
                items = items.Where(x => x.FullName != null && x.FullName.IndexOf(fullname, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(email))
                items = items.Where(x => x.Email != null && x.Email.IndexOf(email, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(phone))
                items = items.Where(x => x.Phone != null && x.Phone.IndexOf(phone, StringComparison.OrdinalIgnoreCase) >= 0);
            
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            return View(items);

        }


        public ActionResult Index(string userId, DateTime? from, DateTime? to, int? page, int? size, string isPaid = "", string isVerified = "")
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<Order> items = db.Orders.OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(userId))
                items = items
                       .Where(
                       x => x.UserId == userId);
            if (isPaid == "yes")
                items = items.Where(x => x.IsPaid);
            if (isPaid == "no")
                items = items.Where(x => !x.IsPaid);
            if (isVerified == "yes")
                items = items.Where(x => x.IsVerified);
            if (isVerified == "no")
                items = items.Where(x => !x.IsVerified);
            if (from != null)
                items = items.Where(x => x.OrderedDate >= from);
            if (to != null)
                items = items.Where(x => x.OrderedDate <= to);
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            return View(items);


        }
        [HttpPost]
        public void SetTempData(string data)
        {
            TempData["id"] = data;
        }


        // GET: Admin/Orders/Details/5 /Admin/Orders/Detail/DH1
        //[Authorize(Roles = "Admin, Employee")]
        [HttpGet]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            return View(order);
        }
        [HttpPost]
        public ActionResult IsVerified(string id)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                item.IsVerified = !item.IsVerified;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isVerified = item.IsVerified });
            }
            return Json(new { success = false });
        }
    }
}
//public ActionResult Index()
//{
//    var orders = db.Orders.Include(o => o.PaymentMethod).Include(o => o.User);
//    return View(orders.ToList());
//}
// GET: Admin/Orders/Create
//public ActionResult Create()
//{
//    ViewBag.PaymentMethodId = new SelectList(db.PaymentMethods, "Id", "Name");
//    ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FullName");
//    return View();
//}

//// POST: Admin/Orders/Create
//// To protect from overposting attacks, enable the specific properties you want to bind to, for 
//// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[ValidateAntiForgeryToken]
//public ActionResult Create([Bind(Include = "Id,UserId,Address,Phone,IsPaid,PaymentMethodId,OrderedDate")] Order order)
//{
//    if (ModelState.IsValid)
//    {
//        db.Orders.Add(order);
//        db.SaveChanges();
//        return RedirectToAction("Index");
//    }

//    ViewBag.PaymentMethodId = new SelectList(db.PaymentMethods, "Id", "Name", order.PaymentMethodId);
//    ViewBag.UserId = new SelectList(db.ApplicationUsers, "Id", "FullName", order.UserId);
//    return View(order);
//}