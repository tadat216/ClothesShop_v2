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

namespace ClothesShop.Controllers
{
    //[Authorize(Roles = "Admin, Customer, Employee")]
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
            var cart = db.Carts.FirstOrDefault(x=>x.UserId == user.Id);
            var cartDetails = db.CartDetails.Where(x=>x.CartId == cart.Id).ToList();
            return View(cartDetails);
        }
        //new  [AllowAnonymous] cho checkout, checkoutsuccess
//        [AllowAnonymous]

//        public ActionResult CheckOut()
//        {
//            ShoppingCart cart = (ShoppingCart)Session["Cart"];
//            if (cart != null && cart.Items.Any())
//            {
//                ViewBag.CheckCart = cart;
//            }
//            return View();
//        }
//        [AllowAnonymous]

//        public ActionResult CheckOutSuccess()
//        {
//            return View();
//        }
//        //[AllowAnonymous]
//        //public ActionResult Partial_Item_ThanhToan()
//        //{
//        //    ShoppingCart cart = (ShoppingCart)Session["Cart"];
//        //    if (cart != null && cart.Items.Any())
//        //    {
//        //        return PartialView(cart.Items);
//        //    }
//        //    return PartialView();
//        //}
//        [AllowAnonymous]
//        public ActionResult PartialItemCartCheckOut()
//        {
//            ShoppingCart cart = (ShoppingCart)Session["Cart"];
//            if (cart != null && cart.Items.Any())
//            {
//                return PartialView(cart.Items);
//            }
//            return PartialView();
//        }
//        [AllowAnonymous]

//        public ActionResult PartialItemCart()
//        {
//            ShoppingCart cart = (ShoppingCart)Session["Cart"];
//            if (cart != null)
//            {
//                return PartialView(cart.Items);
//            }
//            return PartialView();
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        public ActionResult ShowCount()
//        {
//            ShoppingCart cart = (ShoppingCart)Session["Cart"];
//            if (cart != null)
//            {
//                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult CheckOut(OrderViewModel req)
//        {
//            var code = new { Success = false, Code = -1 };
//            if (ModelState.IsValid)
//            {
//                ShoppingCart cart = (ShoppingCart)Session["Cart"];
//                if (cart != null)
//                {
//                    Order order = new Order();
//                    order.CustomerName = req.CustomerName;
//                    order.Phone = req.Phone;
//                    order.Email = req.Email;
//                    order.Address = req.Address;
//                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
//                    {
//                        ProductId = x.ProductId,
//                        Quantity = x.Quantity,
//                        //if(issale price=pricesale
//                        Price = x.Price,
//                    }));
//                    order.Quantity = cart.Items.Sum(x => x.Quantity);
//                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
//                    order.PaymentType = req.PaymentType;
//                    order.CreatedDate = DateTime.Now;
//                    order.ModifiedDate = DateTime.Now;
//                    //order.CreatedBy = req.Phone;

//                    if (User.Identity.IsAuthenticated)
//                        order.CustomerId = User.Identity.GetUserId();
//                    Random rd = new Random();
//                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
//                    db.Orders.Add(order);
//                    db.SaveChanges();
//                    foreach (var item in order.OrderDetails)
//                    {
//                        Product product = db.Products.Find(item.ProductId);
//                        product.Quantity = product.Quantity - item.Quantity;
//                        if (product.Quantity <= 0)
//                        {
//                            QLTiemBanCay.Common.SendMail("ShopOnline", "Thông báo số lượng sản phẩm " + product.Title + " còn thấp", "Số lượng sản phẩm " + product.Title + " còn " + product.Quantity + ".Vui lòng cập nhật thêm sản phẩm", ConfigurationManager.AppSettings["EmailAdmin"]);
//                        }

//                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
//                        db.SaveChanges();
//                    }
//                    //send mail cho khach hang
//                    var strSanPham = "";
//                    var thanhtien = decimal.Zero;
//                    var TongTien = decimal.Zero;
//                    foreach (var sp in cart.Items)
//                    {
//                        strSanPham += "<tr>";
//                        strSanPham += "<td>" + sp.ProductName + "</td>";
//                        strSanPham += "<td>" + sp.Quantity + "</td>";
//                        strSanPham += "<td>" + QLTiemBanCay.Common.FormatNumber(sp.PriceTotal, 0) + "</td>";
//                        strSanPham += "<tr>";

//                        thanhtien += sp.Price * sp.Quantity;
//                    }
//                    TongTien = thanhtien;

//                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
//                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
//                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
//                    contentCustomer = contentCustomer.Replace("{{Email}}", order.Email);
//                    contentCustomer = contentCustomer.Replace("{{DiaChi}}", order.Address);
//                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
//                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", order.CreatedDate.ToString());
//                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
//                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", QLTiemBanCay.Common.FormatNumber(thanhtien, 0));
//                    contentCustomer = contentCustomer.Replace("{{TongTien}}", QLTiemBanCay.Common.FormatNumber(TongTien, 0));
//                    QLTiemBanCay.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentCustomer.ToString(), order.Email);

//                    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
//                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
//                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
//                    contentAdmin = contentAdmin.Replace("{{Email}}", order.Email);
//                    contentAdmin = contentAdmin.Replace("{{DiaChi}}", order.Address);
//                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
//                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
//                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", order.CreatedDate.ToString());
//                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", QLTiemBanCay.Common.FormatNumber(thanhtien, 0));
//                    contentAdmin = contentAdmin.Replace("{{TongTien}}", QLTiemBanCay.Common.FormatNumber(TongTien, 0));
//                    QLTiemBanCay.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);


//                    cart.ClearCart();
//                    code = new { Success = true, Code = 1 };
//                    //return RedirectToAction("Index");
//                }
//            }
//            return Json(code);
//        }

//        [AllowAnonymous]
//        public ActionResult PartialCheckOut()
//        {
//            var user = UserManager.FindByNameAsync(User.Identity.Name).Result;
//            if (user != null)
//            {
//                ViewBag.User = user;
//            }
//            return PartialView();
//        }

//        [HttpPost]
//        public ActionResult AddToCart(int id, int quantity)
//        {
//            var code = new { Success = false, msg = string.Empty, code = -1, Count = 0 };
//            var db = new ApplicationDbContext();
//            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
//            if (checkProduct != null)
//            {
//                ShoppingCart cart = (ShoppingCart)Session["Cart"];
//                if (cart == null)
//                {
//                    cart = new ShoppingCart();
//                }
//                ShoppingCartItem item = new ShoppingCartItem
//                {
//                    ProductId = checkProduct.Id,
//                    ProductName = checkProduct.Title,
//                    CategoryName = checkProduct.ProductCategory.Title,
//                    Quantity = quantity,
//                    Alias = checkProduct.Alias,
//                    ProductImg = checkProduct.Image
//                };
//                item.Price = checkProduct.Price;
//                if (checkProduct.IsSale)
//                {
//                    item.Price = checkProduct.PriceSale;
//                }
//                item.PriceTotal = item.Quantity * item.Price;
//                cart.AddToCart(item, quantity);
//                Session["Cart"] = cart;
//                code = new { Success = true, msg = "Thêm vào giỏ hàng thành công", code = 1, Count = cart.Items.Count };
//            }
//            return Json(code);
//        }


//        //[HttpPost]
//        //public ActionResult Update(int id, int quantity)
//        //{
//        //    ShoppingCart cart = (ShoppingCart)Session["Cart"];
//        //    if (cart != null)
//        //    {
//        //        cart.UpdateQuantity(id, quantity);
//        //        return Json(new {Success=true});
//        //    }
//        //    return Json(new {Success=false});
//        //}


//        [HttpPost]
//        public ActionResult Delete(int id)
//        {
//            var code = new { Success = false, msg = "", code = -1, Count = 0 };
//            ShoppingCart cart = (ShoppingCart)Session["Cart"];
//            if (cart != null)
//            {
//                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
//                if (checkProduct != null)
//                {
//                    cart.Remove(id);
//                    code = new { Success = true, msg = "Xóa sản phẩm thành công", code = 1, Count = cart.Items.Count };
//                }
//            }
//            return Json(code);
//        }


//        //[HttpPost]
//        //public ActionResult DeleteAll(int id)
//        //{
//        //    ShoppingCart cart = (ShoppingCart)Session["Cart"];
//        //    if (cart != null)
//        //    {
//        //       cart.ClearCart();
//        //        return Json(new {Success=true});
//        //    }
//        //    return Json(new { Success = false });
//        //}

//        //public string UrlPayment(int TypePaymentVN, string orderCode)
//        //{
//        //    var urlPayment = "";
//        //    var order=db.Orders.FirstOrDefault(x=>x.Code==orderCode);
//        //    //get Congig Info
//        //    string vnp_returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];//Urrl nhan ket qua tra ve
//        //    string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];//Urrl thanh toan cua VNPAY
//        //    string vnp_InnCode = ConfigurationManager.AppSettings["vnp_InnCode"];//Ma dinh danh
//        //    string vnp_hashSecret = ConfigurationManager.AppSettings["vnp_hashSecret"];//Scret Key
//        //    //Build url for vnpay
//        //    VnPayLibrary vnpay=new VnPayLibrary();
//        //    var Price = (long)order.TotalAmount + 100;
//        //    vnpay.AddRequestData("vn_Version",)

//        //}
    }

}