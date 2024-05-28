using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ClothesShop.Models;
using System.Configuration;
using System.IO;

namespace ClothesShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BackupAndRestoreController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-NJF4LHP; Initial Catalog=ClothesShop; Integrated Security=True; MultipleActiveResultSets=True";
        //private string connectionString = "Data Source=DESKTOP-NJF4LHP;  Initial Catalog=ClothesShop; Integrated Security=True; MultipleActiveResultSets=True\" providerName=\"System.Data.SqlClient";
        ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/BackupAndRestore
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult BackupDatabase()
        {
            string backupPath = db.Parameters.FirstOrDefault(x => x.Name == "BackupPath").Value;
            string backupFileName = $@"{backupPath}\backup_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bak";

            using (var connection = new SqlConnection(connectionString))
            {
                var query = $"BACKUP DATABASE [ClothesShop] TO DISK='{backupFileName}'";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            
            return Json(new {Success = "Sao lưu thành công"});
        }
        
        //trong index có phần lấy file ra, nếu như sử dụng ajax sẽ gây lỗi ko thể sd vì file đang được dùng
        [HttpPost]
        public ActionResult RestoreDatabase(HttpPostedFileBase backupFile)
        {
            if (backupFile != null && backupFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(backupFile.FileName);
                if (extension.ToLower() == ".bak")
                {
                    string folderPath = Server.MapPath("~/App_Data/Backups");
                    string filePath = Path.Combine(folderPath, backupFile.FileName);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    backupFile.SaveAs(filePath);

                    string connectionString = ConfigurationManager.ConnectionStrings["MasterConnectionString"].ConnectionString;
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        // Đặt cơ sở dữ liệu đang hoạt động sang SINGLE_USER để ngắt kết nối tất cả các phiên khác
                        using (var command = new SqlCommand($"ALTER DATABASE [ClothesShop] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        using (var command = new SqlCommand($"RESTORE DATABASE [ClothesShop] FROM DISK='{filePath}' WITH REPLACE", connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        // Đặt lại cơ sở dữ liệu sang MULTI_USER
                        using (var command = new SqlCommand($"ALTER DATABASE [ClothesShop] SET MULTI_USER", connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        connection.Close();
                        ViewBag.TB = "Phục hồi thành công";
                    }
                    return View("Index");
                }
                else
                {
                    ViewBag.TB = "Hãy chọn file .bak";
                    return View("Index");
                }
            }
            else
            {
                ViewBag.TB = "Hãy chọn file";
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult AutoBackupDatabase()
        {
            int currentDayOfWeek = (int)DateTime.Now.DayOfWeek;
            string scheduledDays = db.Parameters.FirstOrDefault(x => x.Name == "BackupDays").Value;
            List<int> daysToBackup = scheduledDays.Split(',').Select(int.Parse).ToList();
            if (daysToBackup.Contains(currentDayOfWeek))
            {
                Task.Run(() =>
                {
                    string backupPath = db.Parameters.FirstOrDefault(x => x.Name == "BackupPath").Value;
                    string backupFileName = $@"{backupPath}\backup_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bak";

                    using (var connection = new SqlConnection(connectionString))
                    {
                        var query = $"BACKUP DATABASE [ClothesShop] TO DISK='{backupFileName}'";
                        using (var command = new SqlCommand(query, connection))
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                }); 
            }
            return new EmptyResult();
        }
    }
}
