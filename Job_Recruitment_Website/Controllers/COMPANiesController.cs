using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class CompaniesController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        private int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        // ========================================================
        // 1. DÀNH CHO TẤT CẢ MỌI NGƯỜI (XEM DANH SÁCH & CHI TIẾT CÔNG TY)
        // ========================================================

        // Trang danh sách công ty (Ai cũng xem được, không cần bắt buộc đăng nhập Recruiter)
        public ActionResult Index()
        {
            var list = db.COMPANIES.OrderByDescending(c => c.CompanyId).ToList();
            return View(list);
        }

        // Xem chi tiết một công ty và các việc làm đang tuyển của công ty đó
        public ActionResult Details(int id)
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.CompanyId == id);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        // ========================================================
        // 2. DÀNH RIÊNG CHO NHÀ TUYỂN DỤNG (RECRUITER) QUẢN LÝ CÔNG TY
        // ========================================================

        [RoleAuthorize("Recruiter")]
        public ActionResult MyCompany()
        {
            int userId = CurrentUserId;
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == userId);
            if (company == null) return RedirectToAction("Create");
            return View(company);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Create()
        {
            int userId = CurrentUserId;
            if (db.COMPANIES.Any(c => c.UserId == userId))
                return RedirectToAction("MyCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Create(COMPANy company, HttpPostedFileBase logoFile)
        {
            // Xử lý upload file logo
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                {
                    string name = Guid.NewGuid() + ext;
                    string path = Server.MapPath("~/Content/Logos/" + name);

                    // Kiểm tra và tạo thư mục nếu chưa có
                    if (!Directory.Exists(Server.MapPath("~/Content/Logos/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Logos/"));
                    }

                    logoFile.SaveAs(path);
                    company.Logo_Url = "/Content/Logos/" + name;
                }
            }

            if (string.IsNullOrEmpty(company.Logo_Url))
            {
                company.Logo_Url = "https://via.placeholder.com/150";
            }

            ModelState.Remove("Logo_Url");

            if (ModelState.IsValid)
            {
                company.UserId = CurrentUserId;
                db.COMPANIES.Add(company);
                db.SaveChanges();
                return RedirectToAction("MyCompany");
            }
            return View(company);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Edit()
        {
            int userId = CurrentUserId;
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == userId);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(COMPANy model, HttpPostedFileBase logoFile)
        {
            int userId = CurrentUserId;
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == userId && c.CompanyId == model.CompanyId);
            if (company == null) return HttpNotFound();

            // Xử lý cập nhật logo mới nếu có
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                {
                    string name = Guid.NewGuid() + ext;
                    string path = Server.MapPath("~/Content/Logos/" + name);
                    logoFile.SaveAs(path);
                    company.Logo_Url = "/Content/Logos/" + name;
                }
            }

            if (ModelState.IsValid)
            {
                company.CompanyName = model.CompanyName;
                company.Address = model.Address;
                company.Website = model.Website;
                company.Description = model.Description;
                db.SaveChanges();
                return RedirectToAction("MyCompany");
            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}