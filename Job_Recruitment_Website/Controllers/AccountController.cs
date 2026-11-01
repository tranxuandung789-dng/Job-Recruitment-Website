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

        public ActionResult Register() { return View(); }

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
            user.RoleId = db.ROLES.First(r => r.RoleName == role).RoleId;
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            db.USERS.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        public ActionResult Login() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            string hash = PasswordHelper.Hash(password);
            var user = db.USERS.FirstOrDefault(u => u.Email == email
                               && u.PasswordHash == hash && u.IsActive == true);
            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu, hoặc tài khoản đã bị khóa";
                return View();
            }

            // Lưu thông tin vào Session
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.ROLE.RoleName;

            // TỰ ĐỘNG CHUYỂN HƯỚNG THEO VAI TRÒ NGAY KHI ĐĂNG NHẬP:
            if (user.ROLE.RoleName == "Admin")
            {
                // Nếu là Admin -> Chuyển thẳng vào trang Admin Dashboard
                return RedirectToAction("Index", "Admin");
            }
            else if (user.ROLE.RoleName == "Recruiter")
            {
                // Nếu là Nhà tuyển dụng -> Chuyển thẳng vào trang Quản lý tin (MyJobs)
                return RedirectToAction("MyJobs", "Jobs");
            }
            else
            {
                // Nếu là Ứng viên (Candidate) -> Chuyển thẳng ra trang Danh sách việc làm
                return RedirectToAction("Index", "Jobs");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}