using ClothesShop.Models;
using ClothesShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: News
        public ActionResult Index()
        {
            var items = db.Newes.Where(x => x.IsActive).ToList();
            return View(items);
        }

        public ActionResult Detail(string Id)
        {
            News item = db.Newes.Find(Id);
            return View(item);
        }
    }
}