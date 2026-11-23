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
                TempData["Success"] = "Tạo hồ sơ công ty thành công!";
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
                TempData["Success"] = "Cập nhật thông tin công ty thành công!";
                return RedirectToAction("MyCompany");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Delete()
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create");

            if (db.JOBS.Any(j => j.CompanyId == company.CompanyId))
            {
                TempData["Error"] = "Công ty đang có tin tuyển dụng nên không thể xóa. Vui lòng xóa hết tin tuyển dụng trước!";
                return RedirectToAction("MyCompany");
            }

            var linkedUsers = db.USERS.Where(u => u.CompanyId == company.CompanyId).ToList();
            foreach (var u in linkedUsers)
            {
                u.CompanyId = null;
            }

            string companyName = company.CompanyName;
            db.COMPANIES.Remove(company);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa hồ sơ công ty: " + companyName;
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}