using ClothesShop.Models;
using ClothesShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Controllers
{
    [Authorize(Roles = "Admin, Customer, Employee")]
    public class RatingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Ratings
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RatingFormPartial(string userId, string productId)
        {
            if (User.Identity.Name == "")
            {
                return PartialView("_RatingFormPartial", null);
            }
            var user = db.Users.Find(userId); // synchronous method
            if (user == null)
            {
                return PartialView("_RatingFormPartial", null);
            }
            var rating = db.Rates.Where(x => x.UserId == user.Id);
            if (rating == null)
            {
                return PartialView("_RatingFormPartial", null);
            }
            Rate item = rating.Where(x => x.ProductVariantId == productId).FirstOrDefault();
            return PartialView("_RatingFormPartial", item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitRating(Rate model)
        {
            if (!ModelState.IsValid)
            {
                // Trả về view với model hiện tại nếu có lỗi xảy ra
                return RedirectToAction("ShoppingHistory", "Account");
            }

            // Tìm đánh giá hiện tại nếu có
            var existingRate = db.Rates.FirstOrDefault(r => r.UserId == model.UserId && r.ProductVariantId == model.ProductVariantId);
            if (existingRate != null)
            {
                // Cập nhật đánh giá hiện có
                existingRate.RateValue = model.RateValue;
                existingRate.Comment = model.Comment;
                existingRate.RatedDate = DateTime.Now;
                existingRate.CanRate = false;
                existingRate.Rated = true;
            }
            else
            {
                // Nếu không có đánh giá nào, tạo mới

                model.RatedDate = DateTime.Now;
                model.CanRate = false;
                model.Rated = true;
                db.Rates.Add(model);
            }

            // Lưu thay đổi vào database
            db.SaveChanges();
            // Redirect người dùng về trang cụ thể, ví dụ trang thông tin sản phẩm
            return RedirectToAction("ShoppingHistory", "Account");
        }


    }
}