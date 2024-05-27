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
using Microsoft.Owin;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using System.Web.Helpers;
namespace ClothesShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
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
                    db.Orders.Where(o => o.OrderedDate.Year == year && o.IsPaid).SelectMany(o => o.OrderDetails).Select(d => new { Month = d.Order.OrderedDate.Month, Money = d.Price * d.Quantity }),
                    m => m.Month,
                    d => d.Month,
                    (month, sales) => new { Month = month.Month, TotalMoney = sales.Sum(s => s.Money) }
                ).OrderBy(m => m.Month).ToList();

            return Json(new { data = monthlyMoneys });
        }
        public ActionResult ExportExcel(string year)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê doanh thu năm " + year);

                int rowStart = 3;
                var months = Enumerable.Range(1, 12).Select(m => new { Month = m }).ToList();
                int yearValue = int.Parse(year);
                var monthlyMoneys = months.GroupJoin(
                        db.Orders.Where(o => o.OrderedDate.Year == yearValue).SelectMany(o => o.OrderDetails).Select(d => new { Month = d.Order.OrderedDate.Month, Money = d.Price * d.Quantity }),
                        m => m.Month,
                        d => d.Month,
                        (month, sales) => new { Month = month.Month, TotalMoney = sales.Sum(s => s.Money) }
                    ).OrderBy(m => m.Month).ToList();

                int sum = 0;
                foreach (var item in monthlyMoneys)
                {
                    sum += item.TotalMoney;
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = item.Month;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = item.TotalMoney;
                    rowStart++;
                }
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1"].Value = "Thống kê doanh thu năm " + year;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1:B15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:B15"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2"].Value = "Tháng";
                worksheet.Cells["B2"].Value = "Doanh thu";
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Style.Font.Bold = true;
                worksheet.Cells["A2:B15"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:B15"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:B15"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A2:B15"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Cells["A15"].Value = "Tổng cộng";
                worksheet.Cells["B15"].Value = sum;
                worksheet.Cells["B15"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B15"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 224, 224));

                // Thêm biểu đồ cột
                var chart = worksheet.Drawings.AddChart("columnChart", eChartType.ColumnClustered);
                chart.Title.Text = "Biểu đồ doanh thu năm " + year;
                chart.SetPosition(0, 0, 3, 0);
                chart.SetSize(600, 300);

                // Thêm series cho biểu đồ
                var series = chart.Series.Add(worksheet.Cells["B2:B14"], worksheet.Cells["A2:A14"]);
                series.Header = "Doanh thu (VNĐ)";
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
                                     && DbFunctions.TruncateTime(o.OrderedDate) <= DbFunctions.TruncateTime(to.Value) && o.IsPaid)
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
        //ajax cần đúng kiểu dl, a href thì ko cần=>tất cả là string
        public ActionResult FromToExportExcel(DateTime from, DateTime to)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //DateTime ko trả về dd/mm/yyyy mà là 1 chuỗi số=> muốn dd/mm/yyy thì phải toString
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê doanh thu từ " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy"));

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

                int rowStart = 3;
                int sum = 0;
                foreach (var item in dailyMoneys)
                {
                    sum += item.TotalMoney;
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = item.Date;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = item.TotalMoney;
                    rowStart++;
                }
                int count = dailyMoneys.Count;
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1"].Value = "Thống kê doanh thu từ  " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy");
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A1:B{0}", count + 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[string.Format("A1:B{0}", count + 2)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2"].Value = "Ngày";
                worksheet.Cells["B2"].Value = "Doanh thu";
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A2:B{0}", count + 2)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:B{0}", count + 2)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:B{0}", count + 2)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:B{0}", count + 2)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 30;
                worksheet.Cells[string.Format("A{0}", count + 2)].Value = "Tổng cộng";
                worksheet.Cells[string.Format("B{0}", count + 2)].Value = sum;
                worksheet.Cells[string.Format("B{0}", count + 2)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[string.Format("B{0}", count + 2)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 224, 224));

                // Thêm biểu đồ cột
                var chart = worksheet.Drawings.AddChart("columnChart", eChartType.ColumnClustered);
                chart.Title.Text = "Biểu đồ doanh doanh thu từ  " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy");
                chart.SetPosition(0, 0, 3, 0);
                chart.SetSize(800, 400);

                // Thêm series cho biểu đồ
                var series = chart.Series.Add(worksheet.Cells[string.Format("B2:B{0}", count + 1)], worksheet.Cells[string.Format("A2:A{0}", count + 1)]);
                series.Header = "Doanh thu (VNĐ)";

                string fileName = "ThongKeDoanhThuTu" + from.ToString("dd-MM-yyyy") + "Den" + to.ToString("dd-MM-yyyy") + ".xlsx";
                string path = Path.Combine(Server.MapPath("~/ReportData"), fileName);

                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                //return Json(new { Success = true });
            }

        }

        //sản phẩm bán được trong năm
        public ActionResult ProductQuantityStatisticsIndex()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetProductQuantityStatistics(DateTime? from, DateTime? to)
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
                var productQuantities = db.Orders.Where(x => DbFunctions.TruncateTime(x.OrderedDate) >= DbFunctions.TruncateTime(from.Value)
                  && DbFunctions.TruncateTime(x.OrderedDate) <= DbFunctions.TruncateTime(to.Value) && x.IsPaid == true)
                    .SelectMany(o => o.OrderDetails).GroupBy(od => new { ProductId = od.VariantSize.ProductVariant.Product.Id, ProductName = od.VariantSize.ProductVariant.Product.Title })
                    .Select(g => new { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, TotalQuantity = g.Sum(od => od.Quantity) })
                    .OrderByDescending(g => g.TotalQuantity).ToList();
                var totalQuantitySum = productQuantities.Sum(g => g.TotalQuantity);
                var percentageList = productQuantities.Select(g => Math.Round((double)g.TotalQuantity / totalQuantitySum * 100, 2)).ToList();

                return Json(new { productQuantities = productQuantities, percentageList = percentageList });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        public ActionResult ProductQuantityExportExcel(DateTime from, DateTime to)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //DateTime ko trả về dd/mm/yyyy mà là 1 chuỗi số=> muốn dd/mm/yyy thì phải toString
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê sản phẩm bán được từ " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy"));

                int rowStart = 3;
                var productQuantities = db.Orders.Where(x => DbFunctions.TruncateTime(x.OrderedDate) >= DbFunctions.TruncateTime(from)
                 && DbFunctions.TruncateTime(x.OrderedDate) <= DbFunctions.TruncateTime(to) && x.IsPaid == true)
                   .SelectMany(o => o.OrderDetails).GroupBy(od => new { ProductId = od.VariantSize.ProductVariant.Product.Id, ProductName = od.VariantSize.ProductVariant.Product.Title })
                   .Select(g => new { ProductId = g.Key.ProductId, ProductName = g.Key.ProductName, TotalQuantity = g.Sum(od => od.Quantity) })
                   .OrderByDescending(g => g.TotalQuantity).ToList();
                var totalQuantitySum = productQuantities.Sum(g => g.TotalQuantity);
                var percentageList = productQuantities.Select(g => Math.Round((double)g.TotalQuantity / totalQuantitySum * 100, 2)).ToList();
                int count = productQuantities.Count();
                int sum = 0;
                for (int i = 0; i < productQuantities.Count; i++)
                {
                    sum += productQuantities[i].TotalQuantity;
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = i + 1;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = productQuantities[i].ProductId;
                    worksheet.Cells[string.Format("C{0}", rowStart)].Value = productQuantities[i].ProductName;
                    worksheet.Cells[string.Format("D{0}", rowStart)].Value = productQuantities[i].TotalQuantity;
                    worksheet.Cells[string.Format("E{0}", rowStart)].Value = percentageList[i];
                    rowStart++;
                }

                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A1"].Value = "Thống kê sản phẩm bán được từ  " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy");
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A1:E{0}", count + 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[string.Format("A1:E{0}", count + 2)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Mã sản phẩm";
                worksheet.Cells["C2"].Value = "Tên sản phẩm";
                worksheet.Cells["D2"].Value = "Số lượng bán";
                worksheet.Cells["E2"].Value = "Tỉ lệ";
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Style.Font.Bold = true;
                worksheet.Cells["C2"].Style.Font.Bold = true;
                worksheet.Cells["D2"].Style.Font.Bold = true;
                worksheet.Cells["E2"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A2:E{0}", count + 2)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:E{0}", count + 2)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:E{0}", count + 2)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:E{0}", count + 2)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Column(1).Width = 5;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 10;
                worksheet.Cells[string.Format("A{0}:B{0}", count + 2)].Merge = true;
                worksheet.Cells[string.Format("A{0}", count + 2)].Value = "Tổng cộng";
                worksheet.Cells[string.Format("B{0}", count + 2)].Value = count;
                worksheet.Cells[string.Format("C{0}", count + 2)].Value = count;
                worksheet.Cells[string.Format("D{0}", count + 2)].Value = sum;
                worksheet.Cells[string.Format("E{0}", count + 2)].Value = "100";

                // Thêm biểu đồ cột
                var chart = worksheet.Drawings.AddChart("pieChart", eChartType.Pie);
                chart.Title.Text = "Biểu đồ thống kê sản phẩm bán được từ  " + from.ToString("dd-MM-yyyy") + " đến " + to.ToString("dd-MM-yyyy");
                chart.SetPosition(0, 0, 6, 0);
                chart.SetSize(500, 300);

                // Thêm series cho biểu đồ
                var series = chart.Series.Add(worksheet.Cells[string.Format("E2:E{0}", count + 1)], worksheet.Cells[string.Format("B2:B{0}", count + 1)]);
                series.Header = "Tỉ lệ (%)";

                string fileName = "ThongKeSanPhamBanDuoc" + from.ToString("dd-MM-yyyy") + "Den" + to.ToString("dd-MM-yyyy") + ".xlsx";
                string path = Path.Combine(Server.MapPath("~/ReportData"), fileName);

                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }

        }
        public ActionResult InstockProductStatistics(int? page, int? size)
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            IEnumerable<VariantSize> instockProducts = db.Products.Where(x => x.IsActive).SelectMany(x => x.ProductVariants.SelectMany(y => y.VariantSizes)).OrderBy(x => x.ProductVariant.ProductId).OrderBy(x => x.ProductVariantId);
            instockProducts = instockProducts.ToPagedList(pageIndex, pageSize);

            return View(instockProducts);
        }
        public ActionResult InStockProductExportExcel()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                DateTime date = DateTime.Now;
                //DateTime ko trả về dd/mm/yyyy mà là 1 chuỗi số=> muốn dd/mm/yyy thì phải toString
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Thống kê sản phẩm tồn kho ngày" + date.ToString("dd-MM-yyyy"));

                int rowStart = 3;
                var instockProducts = db.Products.Where(x => x.IsActive).SelectMany(x => x.ProductVariants.SelectMany(y => y.VariantSizes)).OrderBy(x => x.ProductVariant.ProductId).OrderBy(x => x.ProductVariantId).ToList();
                int count = instockProducts.Count();
                for (int i = 0; i < count; i++)
                {
                    worksheet.Cells[string.Format("A{0}", rowStart)].Value = i + 1;
                    worksheet.Cells[string.Format("B{0}", rowStart)].Value = instockProducts[i].Id;
                    worksheet.Cells[string.Format("C{0}", rowStart)].Value = instockProducts[i].ProductVariant.Product.Title;
                    worksheet.Cells[string.Format("D{0}", rowStart)].Value = instockProducts[i].ProductVariant.Color.Name;
                    worksheet.Cells[string.Format("E{0}", rowStart)].Value = instockProducts[i].Size.Name;
                    worksheet.Cells[string.Format("F{0}", rowStart)].Value = instockProducts[i].Amount;
                    rowStart++;
                }

                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1"].Value = "Thống kê sản phẩm tồn kho ngày" + date.ToString("dd-MM-yyyy");
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A1:F{0}", count + 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[string.Format("A1:F{0}", count + 2)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Mã";
                worksheet.Cells["C2"].Value = "Tên sản phẩm";
                worksheet.Cells["D2"].Value = "Màu sắc";
                worksheet.Cells["E2"].Value = "Kích cỡ";
                worksheet.Cells["F2"].Value = "Số lượng";
                worksheet.Cells["A2"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Style.Font.Bold = true;
                worksheet.Cells["C2"].Style.Font.Bold = true;
                worksheet.Cells["D2"].Style.Font.Bold = true;
                worksheet.Cells["E2"].Style.Font.Bold = true;
                worksheet.Cells["F2"].Style.Font.Bold = true;
                worksheet.Cells[string.Format("A2:F{0}", count + 2)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:F{0}", count + 2)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:F{0}", count + 2)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[string.Format("A2:F{0}", count + 2)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Column(1).Width = 5;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 10;
                string fileName = "ThongKeSanPhamTonKhoNgay" + date.ToString("dd - MM - yyyy") + ".xlsx";
                string path = Path.Combine(Server.MapPath("~/ReportData"), fileName);
                pck.SaveAs(new FileInfo(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
        }
    }
}