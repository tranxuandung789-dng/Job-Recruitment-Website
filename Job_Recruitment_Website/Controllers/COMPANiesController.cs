using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Job_Recruitment_Website.Controllers
{
    [RoleAuthorize("Recruiter")]
    public class CompaniesController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        private int CurrentUserId { get { return (int)Session["UserId"]; } }

        public ActionResult MyCompany()
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            if (company == null) return RedirectToAction("Create");
            return View(company);
        }

        public ActionResult Create()
        {
            if (db.COMPANIES.Any(c => c.UserId == CurrentUserId))
                return RedirectToAction("MyCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(COMPANy company, HttpPostedFileBase logoFile)
        {
            // Xử lý upload file logo
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".png" || ext == ".jpeg")
                {
                    // Tạo tên file ngẫu nhiên để tránh trùng lặp
                    string name = Guid.NewGuid() + ext;
                    string path = Server.MapPath("~/Content/Logos/" + name);
                    logoFile.SaveAs(path);

                    // Thêm tiền tố http:// để vượt qua ràng buộc CHECK trong CSDL SQL Server
                    company.Logo_Url = "http://localhost:55955/Content/Logos/" + name;
                }
            }

            // Nếu người dùng không chọn ảnh, gán tạm một đường dẫn hợp lệ mặc định
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

        public ActionResult Edit()
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(COMPANy model)
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId
                                                       && c.CompanyId == model.CompanyId);
            if (company == null) return HttpNotFound();
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
    }
}