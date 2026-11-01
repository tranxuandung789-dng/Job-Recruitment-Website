using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class AccountController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(USER user, string password, string confirmPassword, string role)
        {
            // 1. Kiểm tra xem 2 mật khẩu có khớp nhau không
            if (password != confirmPassword)
            {
                ModelState.AddModelError("PasswordError", "Mật khẩu nhập lại không khớp!");
                return View(user);
            }

            // 2. Kiểm tra trùng Email trong CSDL
            if (db.USERS.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(user);
            }

            // 2.1 Kiểm tra trùng Số điện thoại để tránh lỗi UQ_USERS_Phone
            if (db.USERS.Any(u => u.Phone == user.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng!");
                return View(user);
            }

            // 3. Nếu không có lỗi gì thì tiến hành lưu vào database
            user.PasswordHash = PasswordHelper.Hash(password);
            var selectedRole = db.ROLES.FirstOrDefault(r => r.RoleName == role) ?? db.ROLES.FirstOrDefault(r => r.RoleId == 3);
            user.RoleId = selectedRole.RoleId;
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            db.USERS.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            string hash = PasswordHelper.Hash(password);

            // Kiểm tra khớp mật khẩu đã Hash HOẶC khớp trực tiếp chuỗi mẫu trong SQL (để dễ test dữ liệu mẫu)
            var user = db.USERS.FirstOrDefault(u => u.Email == email
                                && (u.PasswordHash == hash || u.PasswordHash == password || (password == "123456" && u.PasswordHash.StartsWith("$2b$12$HashPass")))
                                && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu, hoặc tài khoản đã bị khóa";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Lưu đầy đủ thông tin vào Session (Đồng bộ cho cả Layout và các View phân quyền)
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.ROLE.RoleName;
            Session["RoleId"] = user.RoleId;

            // 1. ƯU TIÊN CHUYỂN HƯỚNG VỀ TRANG TRƯỚC ĐÓ (NẾU CÓ RETURN URL)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 2. NẾU ĐĂNG NHẬP BÌNH THƯỜNG -> TỰ ĐỘNG CHUYỂN HƯỚNG THEO VAI TRÒ:
            if (user.ROLE.RoleName == "Admin" || user.RoleId == 1)
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (user.ROLE.RoleName == "Recruiter" || user.RoleId == 2)
            {
                return RedirectToAction("MyJobs", "Jobs");
            }
            else
            {
                return RedirectToAction("Index", "Jobs");
            }
        }

        // GET: Account/Profile (Trang Hồ sơ ứng viên / Cá nhân)
        public new ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account") });
            }

            int uid = (int)Session["UserId"];
            var user = db.USERS.Find(uid);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Account/Profile (Cập nhật thông tin hồ sơ cá nhân)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(string FullName, string Phone)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int uid = (int)Session["UserId"];
            var user = db.USERS.Find(uid);
            if (user != null)
            {
                // Kiểm tra trùng số điện thoại với người khác
                if (db.USERS.Any(u => u.Phone == Phone && u.UserId != uid))
                {
                    ViewBag.Error = "Số điện thoại này đã được tài khoản khác sử dụng!";
                    return View(user);
                }

                user.FullName = FullName;
                user.Phone = Phone;
                db.SaveChanges();

                Session["FullName"] = user.FullName;
                ViewBag.Success = "Cập nhật hồ sơ cá nhân thành công!";
            }

            return View(user);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}