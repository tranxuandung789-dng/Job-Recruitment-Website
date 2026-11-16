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

        // Hàm hỗ trợ tìm Công ty của Nhà tuyển dụng hiện tại (đồng bộ với JobsController)
        private COMPANy GetRecruiterCompany()
        {
            int uid = CurrentUserId;
            if (uid == 0) return null;

            var user = db.USERS.Find(uid);
            if (user != null && user.CompanyId.HasValue)
            {
                var compByUser = db.COMPANIES.Find(user.CompanyId.Value);
                if (compByUser != null) return compByUser;
            }

            return db.COMPANIES.FirstOrDefault(c => c.UserId == uid);
        }

        // ========================================================
        // 1. DÀNH CHO TẤT CẢ MỌI NGƯỜI (XEM DANH SÁCH & CHI TIẾT CÔNG TY)
        // ========================================================

        public ActionResult Index()
        {
            var list = db.COMPANIES.OrderByDescending(c => c.CompanyId).ToList();
            return View(list);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            var company = db.COMPANIES.FirstOrDefault(c => c.CompanyId == id.Value);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        // ========================================================
        // 2. DÀNH RIÊNG CHO NHÀ TUYỂN DỤNG (RECRUITER) QUẢN LÝ CÔNG TY
        // ========================================================

        [RoleAuthorize("Recruiter")]
        public ActionResult MyCompany()
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create");
            return View(company);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Create()
        {
            if (GetRecruiterCompany() != null)
                return RedirectToAction("MyCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Create(COMPANy company, HttpPostedFileBase logoFile)
        {
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                {
                    string folderPath = Server.MapPath("~/Content/Logos/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string name = Guid.NewGuid() + ext;
                    string path = Path.Combine(folderPath, name);
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
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create");
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(COMPANy model, HttpPostedFileBase logoFile)
        {
            var company = GetRecruiterCompany();
            if (company == null) return HttpNotFound();

            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                {
                    string folderPath = Server.MapPath("~/Content/Logos/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string name = Guid.NewGuid() + ext;
                    string path = Path.Combine(folderPath, name);
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