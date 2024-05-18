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
using System.Web.Management;
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
        DateTime toDefault = DateTime.Now;
        [HttpGet]               
      
        //theo tháng trong năm
        public ActionResult IncomeStatisticsIndex()
        { 
            int year = DateTime.Now.Year;
            ViewBag.years = Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).OrderByDescending(y => y).ToList();
            ViewBag.DefaultYear = year;
            return View();
        }
        [HttpPost]
        public ActionResult GetIncomeStatistics(int year)
        {
            var months = Enumerable.Range(1, 12).Select(m => new { Month = m }).ToList();

            var monthlyMoneys = months.GroupJoin(
                    db.Orders.Where(o => o.OrderedDate.Year == year).SelectMany(o => o.OrderDetails).Select(d => new { Month = d.Order.OrderedDate.Month, Money = d.Price * d.Quantity }),
                    m => m.Month,
                    d => d.Month,
                    (month, sales) => new { Month = month.Month, TotalMoney = sales.Sum(s => s.Money) }
                ).OrderBy(m => m.Month).ToList();

            return Json(new {data = monthlyMoneys });
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
                string path = Path.Combine(Server.MapPath("~/ReportData"), fileName);

                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

        }
        //từ ngày đến ngày
        public ActionResult FromToIncomeStatisticsIndex()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetFromToIncomeStatistics(DateTime? from, DateTime? to)
        {
            try
            {
                if (!from.HasValue)
                    from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!to.HasValue)
                    to = DateTime.Now;

                if (from > to)
                {
                    return Json(new { error = "Ngày bắt đầu không thể lớn hơn ngày kết thúc được." });
                }

                List<DateTime> dateList = new List<DateTime>();
                DateTime tempFrom = from.Value;
                while (tempFrom <= to.Value)
                {
                    dateList.Add(tempFrom);
                    tempFrom = tempFrom.AddDays(1);
                }

                var dailyMoneys = dateList.Select(d => new { Date = d })
                    .GroupJoin(
                        db.Orders
                            .Where(o => DbFunctions.TruncateTime(o.OrderedDate) >= DbFunctions.TruncateTime(from.Value)
                                     && DbFunctions.TruncateTime(o.OrderedDate) <= DbFunctions.TruncateTime(to.Value))
                            .SelectMany(o => o.OrderDetails)
                            .Select(od => new { Date = DbFunctions.TruncateTime(od.Order.OrderedDate), Money = od.Price * od.Quantity }),
                        d => d.Date.Date,
                        od => od.Date,
                        (date, money) => new { Date = date.Date.ToString("dd/MM/yyyy"), TotalMoney = money.Sum(s => s.Money) }
                    )
                    .OrderBy(d => d.Date)
                    .ToList();

                return Json(new { data = dailyMoneys });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult FromToExportExcel(DateTime from, DateTime to)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                
                //DateTime ko trả về dd/mm/yyyy mà là 1 chuỗi số=> muốn dd/mm/yyy thì phải toString
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê doanh thu từ " + from.ToString("dd/MM/yyyy") + "đến" + to.ToString("dd/MM/yyyy"));
                worksheet.Cells["A1"].Value = "Ngày";
                worksheet.Cells["B1"].Value = "Doanh thu";
                int rowStart = 2;
                List<DateTime> dateList = new List<DateTime>();
                DateTime tempFrom = from;
                while (tempFrom < to)
                {
                    dateList.Add(tempFrom);
                    tempFrom = tempFrom.AddDays(1);
                }
                var dates = dateList.Select(d => new { Date = d }).ToList();

                var dailyMoneys = dates.GroupJoin(
                    db.Orders.Where(o => o.OrderedDate >= from && o.OrderedDate <= to)
                             .SelectMany(o => o.OrderDetails)
                             .Select(od => new { Date = DbFunctions.TruncateTime(od.Order.OrderedDate), Money = od.Price * od.Quantity }),
                    d => d.Date.Date,//chỉ lấy phần ngày, không lấy phần thời gian
                    od => od.Date,
                    (date, money) => new { Date = date.Date.ToString("dd/MM/yyyy"), TotalMoney = money.Sum(s => s.Money) }
                ).OrderBy(d => d.Date).ToList();
                foreach (var item in dailyMoneys)
                {
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = item.Date;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = item.TotalMoney;
                    rowStart++;
                }
                string fileName = "ThongKeDoanhThuTu" + from.ToString("dd/MM/yyyy") + "đến" + to.ToString("dd/MM/yyyy") + ".xlsx";
                string path = Path.Combine(Server.MapPath("~/ReportData"), fileName);

                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                //return Json(new { Success = true });
            }

        }

        //sản phẩm bán chạy
        //public ActionResult HotProductStatistics(int year)
        //{

        //}

    }
}