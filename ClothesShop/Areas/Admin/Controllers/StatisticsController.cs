using ClothesShop.Models;
using ClothesShop.Models.EF;
using ClothesShop.Models.ViewModel;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
namespace ClothesShop.Areas.Admin.Controllers
{
    public class MonthlyMoneyStatistics
    {
        public int Month { get; set; }
        public int TotalMoney { get; set; }
    }
    public class StatisticsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [HttpGet]               
      
        
        public ActionResult IncomeStatisticsIndex()
        { 
            int year = DateTime.Now.Year;
            ViewBag.years = Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).OrderByDescending(y => y).ToList();
            ViewBag.DefaultYear = year;
            return View();
        }
        public ActionResult GetIncomeStatistics(int year)
        {
            var months = Enumerable.Range(1, 12).Select(m => new { Month = m }).ToList();

            var monthlyMoneys = months.GroupJoin(
                    db.Orders.Where(o => o.OrderedDate.Year == year).SelectMany(o => o.OrderDetails).Select(d => new { Month = d.Order.OrderedDate.Month, Money = d.Price * d.Quantity }),
                    m => m.Month,
                    d => d.Month,
                    (month, sales) => new { Month = month.Month, TotalMoney = sales.Sum(s => s.Money) }
                ).OrderBy(m => m.Month).ToList();

            return Json(new {data = monthlyMoneys }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportExcel(string year)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê doanh thu năm " + year);
                worksheet.Cells["A1"].Value = "Tháng";
                worksheet.Cells["B1"].Value = "Doanh thu";
                int rowStart = 2;
                var months = Enumerable.Range(1, 12).Select(m => new { Month = m }).ToList();
                int yearValue = int.Parse(year);
                var monthlyMoneys = months.GroupJoin(
                        db.Orders.Where(o => o.OrderedDate.Year == yearValue).SelectMany(o => o.OrderDetails).Select(d => new { Month = d.Order.OrderedDate.Month, Money = d.Price * d.Quantity }),
                        m => m.Month,
                        d => d.Month,
                        (month, sales) => new { Month = month.Month, TotalMoney = sales.Sum(s => s.Money) }
                    ).OrderBy(m => m.Month).ToList();
                foreach (var item in monthlyMoneys)
                {
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = item.Month;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = item.TotalMoney;
                    rowStart++;
                }
                string fileName = "ThongKeDoanhThuNam" + year + ".xlsx";
                string path = Path.Combine(Server.MapPath("~/ReportData"),fileName);

                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            
        }

    }
}