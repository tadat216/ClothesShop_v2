﻿using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult NavBarProductCategory()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_NavBarProductCategory", items);
        }

        public ActionResult BlogPartial()
        {
            var items = db.Newes.ToList();
            return PartialView("_BlogPartial", items);
        }
    }
}