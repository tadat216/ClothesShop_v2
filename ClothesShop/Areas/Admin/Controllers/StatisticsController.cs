//using ClothesShop.Models;
//using ClothesShop.Models.EF;
//using ClothesShop.Models.ViewModel;
//using PagedList;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace ClothesShop.Areas.Admin.Controllers
//{
//    public class StatisticsController : Controller
//    {
//        private ApplicationDbContext db = new ApplicationDbContext();
//        // GET: Admin/Statistics
//        public ActionResult Index()
//        {
//            return View();
//        }
//        public ActionResult ShowSoldQuantity(DateTime? fromDate, DateTime? toDate)
//        {
//            var items = db.Products.ToList();
//            List<ProductTKViewModel> ptk = new List<ProductTKViewModel>();
//            ViewBag.ValueFromDate = fromDate?.ToString("yyyy-MM-dd");
//            ViewBag.ValueToDate = toDate?.ToString("yyyy-MM-dd");
//            foreach (var item in items)
//            {
//                int q = GetQuantity(item.Id, fromDate, toDate);
//                if (q > 0)
//                {
//                    ptk.Add(new ProductTKViewModel
//                    {
//                        ProductId = item.Id,
//                        Title = item.Title,
//                        Alias = item.Alias,
//                        ProductImage = item.Image,
//                        SoldQuantity = q
//                    });
//                }
//            }

//            ptk = ptk.OrderByDescending(p => p.SoldQuantity).ToList();
//            return View(ptk);
//        }
//        public int GetQuantity(int id, DateTime? fromDate, DateTime? toDate)
//        {
//            var items = db.OrderDetails.Where(x => x.ProductId == id);
//            if (fromDate != null) items = items.Where(x => x.Order.CreatedDate >= fromDate);
//            if (toDate != null) items = items.Where(x => x.Order.CreatedDate <= toDate);
//            return items.Any() ? items.Sum(x => x.Quantity) : 0;
//        }
//        public ActionResult SaleReport(DateTime? fromDate, DateTime? toDate, int? page)
//        {

//            ViewBag.ValueFromDate = fromDate?.ToString("yyyy-MM-dd");
//            ViewBag.ValueToDate = toDate?.ToString("yyyy-MM-dd");

//            IEnumerable<Order> items = db.Orders.OrderByDescending(x => x.CreatedDate);
//            if (fromDate != null)
//            {
//                items = items.Where(x => x.CreatedDate >= fromDate);
//            }
//            if (toDate != null)
//            {
//                items = items.Where(x => x.CreatedDate <= toDate);
//            }
//            if (page == null)
//            {
//                page = 1;
//            }
//            var pageNumber = page ?? 1;
//            var pageSize = 10;
//            ViewBag.PageSize = pageSize;
//            ViewBag.PageNumber = pageNumber;
//            ViewBag.ToltalQuantity = items.Sum(x => x.Quantity);
//            ViewBag.TotalAmount = items.Sum(x => x.TotalAmount);
//            return View(items.ToPagedList(pageNumber, pageSize));
//        }
//        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
//        {
//            ViewBag.ValueFromDate = fromDate;
//            ViewBag.ValueToDate = toDate;
//            return View();
//        }

//        [HttpGet]
//        public ActionResult GetStatistical()
//        {
//            DateTime? fromDate = ViewBag.ValueFromDate;
//            DateTime? toDate = ViewBag.ValueToDate;
//            var query = from o in db.Orders
//                        join od in db.OrderDetails
//                        on o.Id equals od.OrderId
//                        join p in db.Products
//                        on od.ProductId equals p.Id
//                        select new
//                        {
//                            CreatedDate = o.CreatedDate,
//                            Quantity = od.Quantity,
//                            Price = od.Price,
//                            OriginalPrice = p.OriginalPrice,
//                            Paid = o.Paid
//                        };
//            query = query.Where(x => x.Paid == true);
//            if (fromDate != null)
//            {
//                query = query.Where(x => x.CreatedDate >= fromDate);
//            }
//            if (toDate != null)
//            {
//                query = query.Where(x => x.CreatedDate <= toDate);
//            }

//            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(x => new
//            {
//                Date = x.Key.Value,
//                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
//                TotalSell = x.Sum(y => y.Quantity * y.Price)
//            }).Select(x => new
//            {
//                Date = x.Date,
//                DoanhThu = x.TotalSell,
//                LoiNhuan = x.TotalSell - x.TotalBuy
//            });

//            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
//        }
//    }
//}