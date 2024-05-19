using ClothesShop.Models.EF;
using ClothesShop.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Security.Policy;
using System.Drawing;

namespace ClothesShop.Controllers
{
    [Authorize(Roles = "Admin, Customer, Employee")]
    public class ShoppingCartController : Controller
    {


        private ApplicationDbContext db = new ApplicationDbContext();
        //new
        private ApplicationSignInManager _signInManager;
        private UserManager _userManager;

        public ShoppingCartController()
        {
        }

        public ShoppingCartController(UserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: ShoppingCart
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var cartDetails = db.CartDetails.Where(x => x.CartId == cart.Id).ToList();
            var totalMoney = 0;
            int count = 0;
            foreach (var p in cart.CartDetails)
            {
                if (p.Quantity > p.VariantSize.Amount)
                {
                    p.Quantity = p.VariantSize.Amount;
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (p.Selected)
                {
                    totalMoney += p.Quantity * p.VariantSize.ProductVariant.Product.PriceSale;
                    count++;
                }
            }
            ViewBag.totalMoney = totalMoney;
            if (count == cart.CartDetails.Count) ViewBag.selectAll = true;
            else ViewBag.selectAll = false;
            return View(cartDetails);
        }
        //new  [AllowAnonymous] cho checkout, checkoutsuccess
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddToCart(string variantSizeId, int quantity)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            VariantSize variantSize = db.VariantSizes.Find(variantSizeId);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            if (cart == null)
            {
                cart = new Cart();
                cart.UserId = user.Id;
                //cart.User = user;
                db.Carts.Add(cart);
                db.SaveChanges();
            }
            CartDetail cartDetail = db.CartDetails.FirstOrDefault(x => x.CartId == cart.Id && x.VariantSizeId == variantSizeId);
            if (cartDetail != null)
            {
                cartDetail.Quantity += quantity;
                db.Entry(cartDetail).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                cartDetail = new CartDetail();
                cartDetail.Quantity = quantity;
                cartDetail.VariantSizeId = variantSizeId;
                cartDetail.CartId = cart.Id;
                cartDetail.Cart = cart;
                cartDetail.VariantSize = variantSize;
                db.CartDetails.Add(cartDetail);
                db.SaveChanges();
            }
            return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng." });
        }


        public async Task<ActionResult> CheckOut()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            if (cart == null)
            {
                ViewBag.Message = "Không tìm thấy giỏ hàng";
                return View();
            }
            var addresses = db.Addresses.Where(x => x.UserId == user.Id).ToList();
            ViewBag.Addresses = null;
            if (addresses != null) ViewBag.Addresses = addresses;
            var cartDetails = db.CartDetails.Where(x => x.CartId == cart.Id).ToList();
            if (cartDetails != null && cartDetails.Any())
            {
                cartDetails = cartDetails.Where(x => x.Selected).ToList();
                if (cartDetails.Count > 0)
                {
                    return View(cartDetails);
                }

            }

            return View();
        }

        public async Task<ActionResult> GoToCheckOut()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            bool hasSelectedItems = cart.CartDetails.Any(x => x.Selected);
            return Json(new { HasSelectedItems = hasSelectedItems }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> OrderProcess(string receiverName, string receiverPhone, string receiverAddress)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var cartDetails = db.CartDetails.Where(x => x.CartId == cart.Id && x.Selected == true).ToList();
            Order order = new Order();
            order.UserId = user.Id;
            order.Address = receiverAddress;
            order.Phone = receiverPhone;
            order.ReceiverName = receiverName;
            order.OrderedDate = DateTime.Now;
            db.Orders.Add(order);
            db.SaveChanges();
            var strSanPham = "";
            var thanhtien = 0;
            var tongTien = 0;
            int i = 1;
            foreach (var item in cartDetails)
            {
                OrderDetail od = new OrderDetail();
                od.OrderId = order.Id;
                od.VariantSizeId = item.VariantSizeId;
                od.Price = item.VariantSize.ProductVariant.Product.PriceSale;
                od.Quantity = item.Quantity;
                var productVarientSize = db.VariantSizes.FirstOrDefault(x => x.Id == item.VariantSizeId);
                productVarientSize.Amount -= item.Quantity;
                db.Entry(productVarientSize).State = EntityState.Modified;
                db.OrderDetails.Add(od);
                var minProductQuantity = int.Parse(db.Parameters.FirstOrDefault(x => x.Name == "MinProductQuantity").Value);
                if (productVarientSize.Amount <= minProductQuantity)
                {
                    ClothesShop.Common.SendMail("ShopOnline", "Thông báo số lượng sản phẩm",
                        "Sản phẩm " + productVarientSize.ProductVariant.Product.Title 
                        + "(" + productVarientSize.ProductVariant.Color.Name + " - " + productVarientSize.Size.Name + ")" + " còn thấp. " 
                        + "Số lượng còn lại trong kho là: " +  productVarientSize.Amount 
                        + ". Vui lòng cập nhật thêm sản phẩm", ConfigurationManager.AppSettings["EmailAdmin"]);
                }
                var moneySaleItem = item.Quantity * item.VariantSize.ProductVariant.Product.PriceSale;
                var moneyItem = item.Quantity * item.VariantSize.ProductVariant.Product.Price;
                strSanPham += "<tr>";
                strSanPham += "<tr>" + i + "</td>";
                strSanPham += "<td>" + item.VariantSize.ProductVariant.Product.Title + "(" + productVarientSize.ProductVariant.Color.Name + " - " + productVarientSize.Size.Name + ")</td>";
                strSanPham += "<td>" + item.Quantity + "</td>";
                strSanPham += "<td>" + item.VariantSize.ProductVariant.Product.Price + "</td>";
                strSanPham += "<td>" + item.VariantSize.ProductVariant.Product.PriceSale + "</td>";
                strSanPham += "<td>" + item.VariantSize.ProductVariant.Product.PriceSale * item.Quantity + "</td>";
                //strSanPham += "<td>" + ClothesShop.Common.FormatNumber(moneySaleItem) + "</td>";
                strSanPham += "<tr>";

                thanhtien += moneySaleItem;
                //tongTien += moneyItem;
                i++;
                db.CartDetails.Remove(item);
            }
            db.SaveChanges();

            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
            contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Id);
            contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
            contentCustomer = contentCustomer.Replace("{{DiaChi}}", order.Address);
            contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
            contentCustomer = contentCustomer.Replace("{{NgayDat}}", order.OrderedDate.ToString());
            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.ReceiverName);
            contentCustomer = contentCustomer.Replace("{{ThanhTien}}", thanhtien.ToString());
            //contentCustomer = contentCustomer.Replace("{{TongTien}}", tongTien.ToString());
            contentCustomer = contentCustomer.Replace("{{TienBangChu}}", NumberToText(thanhtien));
            contentCustomer = contentCustomer.Replace("{{Ngay}}", order.OrderedDate.Day.ToString());
            contentCustomer = contentCustomer.Replace("{{Thang}}", order.OrderedDate.Month.ToString());
            contentCustomer = contentCustomer.Replace("{{Nam}}", order.OrderedDate.Year.ToString());

            ClothesShop.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Id, contentCustomer.ToString(), user.Email);

            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
            contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Id);
            contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
            contentAdmin = contentAdmin.Replace("{{DiaChi}}", order.Address);
            contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
            contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.ReceiverName);
            contentAdmin = contentAdmin.Replace("{{NgayDat}}", order.OrderedDate.ToString());
            contentAdmin = contentAdmin.Replace("{{ThanhTien}}", thanhtien.ToString());
            contentAdmin = contentAdmin.Replace("{{TienBangChu}}", NumberToText(thanhtien));
            contentAdmin = contentAdmin.Replace("{{Ngay}}", order.OrderedDate.Day.ToString());
            contentAdmin = contentAdmin.Replace("{{Thang}}", order.OrderedDate.Month.ToString());
            contentAdmin = contentAdmin.Replace("{{Nam}}", order.OrderedDate.Year.ToString());
            //contentAdmin = contentAdmin.Replace("{{TongTien}}", tongTien.ToString());
            ClothesShop.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Id, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);

            return Json(new { tb = "Đặt hàng thành công." });
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ChangeDefaultAddress(string id)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var addresses = db.Addresses.Where(x => x.UserId == user.Id);
            var code = new { Success = false, ReceiverName = "", ReceiverPhone = "", ReceiverAddress = "" };
            foreach (var ad in addresses)
            {
                if (ad.Id == id)
                {
                    ad.IsDefault = true;
                    db.Addresses.Attach(ad);
                    db.Entry(ad).State = EntityState.Modified;
                    code = new { Success = true, ReceiverName = ad.ReceiverName, ReceiverPhone = ad.ReceiverPhone, ReceiverAddress = ad.ReceiverAddress };
                }
                else
                {
                    ad.IsDefault = false;
                    db.Addresses.Attach(ad);
                    db.Entry(ad).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            return Json(code);
        }
        [HttpPost]
        public ActionResult AddAddress(string name, string phone, string address)
        {
            var ad = new Address();
            ad.UserId = User.Identity.GetUserId();
            ad.ReceiverName = name;
            ad.ReceiverAddress = address;
            ad.ReceiverPhone = phone;
            ad.IsDefault = false;
            db.Addresses.Add(ad);
            db.SaveChanges();
            return Json(new
            {
                IsDefault = ad.IsDefault,
                Id = ad.Id,
                ReceiverName = ad.ReceiverName,
                ReceiverPhone = ad.ReceiverPhone,
                ReceiverAddress = ad.ReceiverAddress
            });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateQuantity(string id, int quantity)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var code = new { Success = false, money = 0, totalMoney = 0, amount = db.CartDetails.FirstOrDefault(x => x.Id == id).VariantSize.Amount };
            if (cart != null)
            {
                var product = cart.CartDetails.FirstOrDefault(x => x.Id == id);
                if (product != null)
                {
                    product.Quantity = quantity;
                    db.CartDetails.Attach(product);
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    var money = quantity * product.VariantSize.ProductVariant.Product.PriceSale;
                    var totalMoney = 0;
                    foreach (var p in cart.CartDetails)
                    {
                        if (p.Selected) totalMoney += p.Quantity * p.VariantSize.ProductVariant.Product.PriceSale;
                    }
                    code = new { Success = true, money = money, totalMoney = totalMoney, amount = product.VariantSize.Amount };
                }
                return Json(code);
            }
            return Json(code);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateSelect(string id, bool selected)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var code = new { Success = false, totalMoney = 0 };
            if (cart != null)
            {
                var product = cart.CartDetails.FirstOrDefault(x => x.Id == id);
                if (product != null)
                {
                    product.Selected = selected;
                    db.CartDetails.Attach(product);
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    var totalMoney = 0;
                    foreach (var p in cart.CartDetails)
                    {
                        if (p.Selected) totalMoney += p.Quantity * p.VariantSize.ProductVariant.Product.PriceSale;
                    }
                    code = new { Success = true, totalMoney = totalMoney };
                }
                return Json(code);
            }
            return Json(code);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateSelectAll(bool selectAll)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var totalMoney = 0;
            var code = new { Success = false, totalMoney = 0 };
            if (cart != null)
            {
                foreach (var p in cart.CartDetails)
                {
                    p.Selected = selectAll;
                    db.CartDetails.Attach(p);
                    db.Entry(p).State = EntityState.Modified;
                    totalMoney += p.Quantity * p.VariantSize.ProductVariant.Product.PriceSale;
                }
                db.SaveChanges();
                if (selectAll) code = new { Success = true, totalMoney = totalMoney };
                else code = new { Success = true, totalMoney = 0 };

                return Json(code);
            }
            return Json(code);
        }


        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var cart = db.Carts.FirstOrDefault(x => x.UserId == user.Id);
            var code = new { Success = false, msg = "", totalMoney = 0, Count = 0 };

            if (cart != null)
            {
                var product = cart.CartDetails.FirstOrDefault(x => x.Id == id);
                if (product != null)
                {
                    db.CartDetails.Remove(product);
                    db.SaveChanges();
                    var totalMoney = 0;
                    foreach (var cd in cart.CartDetails)
                    {
                        if (cd.Selected) totalMoney += cd.Quantity * cd.VariantSize.ProductVariant.Product.PriceSale;
                    }
                    code = new { Success = true, msg = "Xóa sản phẩm thành công", totalMoney = totalMoney, Count = cart.CartDetails.Count };
                }
            }
            return Json(code);
        }
        public static string NumberToText(long number)
        {
            if (number == 0) return "không";
            if (number < 0) return "âm " + NumberToText(Math.Abs(number));

            string[] unitsMap = new string[] { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] tensMap = new string[] { "", "mười", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };

            string words = "";
            // Hàng tỷ
            if ((number / 1000000000) > 0)
            {
                words += NumberToText(number / 1000000000) + " tỷ ";
                number %= 1000000000;
            }
            // Hàng triệu
            if ((number / 1000000) > 0)
            {
                words += NumberToText(number / 1000000) + " triệu ";
                number %= 1000000;
            }
            // Hàng nghìn
            if ((number / 1000) > 0)
            {
                words += NumberToText(number / 1000) + " nghìn ";
                number %= 1000;
            }
            // Hàng trăm
            if ((number / 100) > 0)
            {
                words += NumberToText(number / 100) + " trăm ";
                number %= 100;
            }
            // Hàng chục
            if (number > 0)
            {
                if (number < 20) words += tensMap[number / 10] + (number % 10 > 0 ? " " + unitsMap[number % 10] : "");
                else words += tensMap[number / 10] + (number % 10 > 0 ? " " + unitsMap[number % 10] : "");
            }
            return words.Trim();
        }

        //[HttpPost]
        //public ActionResult DeleteAll(int id)
        //{
        //    ShoppingCart cart = (ShoppingCart)Session["Cart"];
        //    if (cart != null)
        //    {
        //        cart.ClearCart();
        //        return Json(new { Success = true });
        //    }
        //    return Json(new { Success = false });
        //}

        //public string UrlPayment(int TypePaymentVN, string orderCode)
        //{
        //    var urlPayment = "";
        //    var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
        //    //get Congig Info
        //    string vnp_returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];//Urrl nhan ket qua tra ve
        //    string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];//Urrl thanh toan cua VNPAY
        //    string vnp_InnCode = ConfigurationManager.AppSettings["vnp_InnCode"];//Ma dinh danh
        //    string vnp_hashSecret = ConfigurationManager.AppSettings["vnp_hashSecret"];//Scret Key
        //                                                                               //Build url for vnpay
        //    VnPayLibrary vnpay = new VnPayLibrary();
        //    var Price = (long)order.TotalAmount + 100;
        //    vnpay.AddRequestData("vn_Version",)

        //    }
    }

}