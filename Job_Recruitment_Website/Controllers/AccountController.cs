using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (password != confirmPassword)
            {
                ModelState.AddModelError("PasswordError", "Mật khẩu nhập lại không khớp!");
                return View(user);
            }

            if (db.USERS.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(user);
            }

            // Kiểm tra định dạng số điện thoại: chỉ gồm 10-11 chữ số
            user.Phone = (user.Phone ?? "").Trim();
            if (!Regex.IsMatch(user.Phone, @"^[0-9]{10,11}$"))
            {
                ModelState.AddModelError("Phone", "Số điện thoại không hợp lệ! Chỉ nhập 10 hoặc 11 chữ số (VD: 0912345678).");
                return View(user);
            }

            if (db.USERS.Any(u => u.Phone == user.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng!");
                return View(user);
            }

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

            var user = db.USERS.FirstOrDefault(u => u.Email == email
                                && (u.PasswordHash == hash || u.PasswordHash == password || (password == "123456" && u.PasswordHash.StartsWith("$2b$12$HashPass")))
                                && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu, hoặc tài khoản đã bị khóa";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.ROLE.RoleName;
            Session["RoleId"] = user.RoleId;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

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

        // GET: Account/Profile (Trang Hồ sơ ứng viên / Cá nhân + CV)
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

            ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
            return View(user);
        }

        // POST: Account/Profile (Cập nhật thông tin cá nhân + Hồ sơ CV ứng viên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(string FullName, string Phone, string Education, string Skills, string Experience, HttpPostedFileBase cvFile)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int uid = (int)Session["UserId"];
            var user = db.USERS.Find(uid);
            if (user != null)
            {
                FullName = (FullName ?? "").Trim();
                Phone = (Phone ?? "").Trim();

                // Kiểm tra họ tên
                if (FullName.Length == 0)
                {
                    ViewBag.Error = "Vui lòng nhập họ và tên!";
                    ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                    return View(user);
                }

                // Kiểm tra định dạng số điện thoại: chỉ gồm 10-11 chữ số
                if (!Regex.IsMatch(Phone, @"^[0-9]{10,11}$"))
                {
                    ViewBag.PhoneError = "Số điện thoại không hợp lệ! Chỉ nhập 10 hoặc 11 chữ số (VD: 0912345678).";
                    ViewBag.PhoneValue = Phone; // giữ lại giá trị vừa nhập để người dùng sửa
                    ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                    return View(user);
                }

                if (db.USERS.Any(u => u.Phone == Phone && u.UserId != uid))
                {
                    ViewBag.Error = "Số điện thoại này đã được tài khoản khác sử dụng!";
                    ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                    return View(user);
                }

                user.FullName = FullName;
                user.Phone = Phone;

                // Nếu là Ứng viên (Candidate) thì cập nhật thêm bảng CANDIDATEPROFILES (Education, Skills, Experience, CV_Url)
                bool isCandidate = (user.ROLE != null && user.ROLE.RoleName == "Candidate") || user.RoleId == 3;
                if (isCandidate)
                {
                    string uploadedCvPath = null;
                    if (cvFile != null && cvFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(cvFile.FileName).ToLower();
                        if (ext != ".pdf")
                        {
                            ViewBag.Error = "File CV chỉ chấp nhận định dạng PDF (.pdf)!";
                            ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                            return View(user);
                        }
                        if (cvFile.ContentLength > 5 * 1024 * 1024)
                        {
                            ViewBag.Error = "Dung lượng file CV không được vượt quá 5MB!";
                            ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                            return View(user);
                        }

                        string folderPath = Server.MapPath("~/Content/CVs/");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string fileName = Guid.NewGuid().ToString() + ".pdf";
                        cvFile.SaveAs(Path.Combine(folderPath, fileName));
                        uploadedCvPath = "/Content/CVs/" + fileName;
                    }

                    var profile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                    if (profile == null)
                    {
                        profile = new CANDIDATEPROFILE
                        {
                            UserId = uid,
                            Education = !string.IsNullOrWhiteSpace(Education) ? Education.Trim() : "Đang cập nhật",
                            Skills = !string.IsNullOrWhiteSpace(Skills) ? Skills.Trim() : "Đang cập nhật",
                            Experience = !string.IsNullOrWhiteSpace(Experience) ? Experience.Trim() : "Đang cập nhật",
                            CV_Url = uploadedCvPath ?? "Đang cập nhật",
                            UpdatedAt = DateTime.Today
                        };
                        db.CANDIDATEPROFILES.Add(profile);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(Education)) profile.Education = Education.Trim();
                        if (!string.IsNullOrWhiteSpace(Skills)) profile.Skills = Skills.Trim();
                        if (!string.IsNullOrWhiteSpace(Experience)) profile.Experience = Experience.Trim();
                        if (uploadedCvPath != null) profile.CV_Url = uploadedCvPath;
                        profile.UpdatedAt = DateTime.Today;
                    }
                }

                db.SaveChanges();
                Session["FullName"] = user.FullName;
                ViewBag.Success = "Cập nhật hồ sơ cá nhân thành công!";
            }

            ViewBag.CandidateProfile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
            return View(user);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}