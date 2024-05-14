using ClothesShop.Models;
using ClothesShop.Models.Common;
using ClothesShop.Models.EF;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ClothesShop.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private UserManager _userManager;

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
        public AccountController() { }
        public AccountController(ApplicationSignInManager signInManager, UserManager userManager)
        {
            SignInManager = signInManager;
            UserManager = userManager;
        }

        public ActionResult SearchId(string username, string phonenumer , string role, DateTime? from, DateTime? to, int? page, int? size, string confirm = "")
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<ApplicationUser> items = db.Users.OrderByDescending(x => x.Id);
            //int count = items.Count();
            if (!string.IsNullOrEmpty(username))
                items = items.Where(x =>
                    x.UserName.IndexOf(username, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.Email.IndexOf(username, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(phonenumer))
                items = items.Where(x =>
                    x.Phone.IndexOf(phonenumer, StringComparison.OrdinalIgnoreCase) >= 0 );
            if (!string.IsNullOrEmpty(role))
            {
                var roleEntity = db.Roles.Include(r => r.Users).FirstOrDefault(r => r.Id == role);
                if(roleEntity != null)
                {
                    var userIds = roleEntity.Users.Select(u => u.UserId).ToList();
                    var users = db.Users.Where(u => userIds.Contains(u.Id));
                    items = users.AsEnumerable();
                }
                    
            }

            if (confirm == "yes")
                items = items.Where(x => x.EmailConfirmed);
            if (confirm == "no")
                items = items.Where(x => !x.EmailConfirmed);

            items = items.OrderBy(x => x.Id).ToPagedList(pageIndex, pageSize);
            ViewBag.ListRole = db.Roles.OrderByDescending(x => x.Id).ToList();
            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;

            return View(items);

        }


        public ActionResult Index(string Searchtext, string role, DateTime? from, DateTime? to, int? page, int? size, string confirm = "")
        {
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = size.HasValue ? Convert.ToInt32(size) : 5;
            IEnumerable<ApplicationUser> items = db.Users.OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items
                   .Where(
                   x => x.Id == Searchtext);

                if (items.Count() == 0)
                {
                    items = db.Users.OrderByDescending(x => x.Id);
                    items = items.Where(x =>
                    x.UserName.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.Email.IndexOf(Searchtext, StringComparison.OrdinalIgnoreCase) >= 0);
                }

            }
            if (!string.IsNullOrEmpty(role))
            {
                var roleEntity = db.Roles.Include(r => r.Users).FirstOrDefault(r => r.Id == role);
                if (roleEntity != null)
                {
                    var userIds = roleEntity.Users.Select(u => u.UserId).ToList();
                    var users = db.Users.Where(u => userIds.Contains(u.Id));
                    items = users.AsEnumerable();
                }

            }
            if (confirm == "yes")
                items = items.Where(x => x.EmailConfirmed);
            if (confirm == "no")
                items = items.Where(x => !x.EmailConfirmed);

            items = items.OrderBy(x => x.Id).ToPagedList(pageIndex, pageSize); 
            ViewBag.ListRole = db.Roles.OrderByDescending(x => x.Id).ToList();

            ViewBag.pageSize = pageSize;
            ViewBag.pageIndex = pageIndex;
            
            return View(items);


        }
        [HttpPost]
        public void SetTempData(string data)
        {
            TempData["id"] = data;
        }
       
        
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
             ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                TempData["message"] = new XMessage("danger", "Mẫu tin không tồn tại");
                return RedirectToAction("Index");
            }
            CreateAccountViewModel viewModel = new CreateAccountViewModel();
            viewModel.UserName = user.UserName;
            viewModel.Phone = user.PhoneNumber;
            viewModel.Email = user.Email;
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View(viewModel);
        }

        // POST: Admin/Colors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = new XMessage("success", "Cập nhật mẫu tin thành công");
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var item = db.Users.Find(id);
            if (item != null)
            {
                db.Users.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.Phone };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Account");
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult IsActive(string id)
        {
            var item = db.Users.Find(id);
            if (item != null)
            {
                item.EmailConfirmed = !item.EmailConfirmed;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.EmailConfirmed });
            }
            return Json(new { success = false });
        }
    }
}
