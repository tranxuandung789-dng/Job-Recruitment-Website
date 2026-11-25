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

        private const int MaxLogoSizeBytes = 2 * 1024 * 1024; // 2MB

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

        // Kiểm tra chữ ký ảnh JPEG / PNG thật sự (Magic Bytes)
        private static bool IsValidImageSignature(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength < 4) return false;
            try
            {
                byte[] header = new byte[4];
                file.InputStream.Position = 0;
                int read = file.InputStream.Read(header, 0, 4);
                file.InputStream.Position = 0;
                if (read < 4) return false;

                // JPEG: FF D8 FF
                bool isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                // PNG: 89 50 4E 47
                bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;

                return isJpeg || isPng;
            }
            catch
            {
                return false;
            }
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
            if (GetRecruiterCompany() != null)
                return RedirectToAction("MyCompany");

            company.CompanyName = (company.CompanyName ?? "").Trim();
            company.Address = (company.Address ?? "").Trim();
            company.Website = string.IsNullOrWhiteSpace(company.Website) ? null : company.Website.Trim();

            if (company.CompanyName.Length == 0)
            {
                ModelState.AddModelError("CompanyName", "Vui lòng nhập tên công ty.");
            }
            else if (db.COMPANIES.Any(c => c.CompanyName.ToLower() == company.CompanyName.ToLower()))
            {
                ModelState.AddModelError("CompanyName", "Tên công ty này đã tồn tại trên hệ thống!");
            }

            if (company.Address.Length == 0)
            {
                ModelState.AddModelError("Address", "Vui lòng nhập địa chỉ trụ sở công ty.");
            }

            string uploadedLogoUrl = null;
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("logoFile", "Logo chỉ chấp nhận định dạng ảnh .JPG, .JPEG hoặc .PNG!");
                }
                else if (logoFile.ContentLength > MaxLogoSizeBytes)
                {
                    ModelState.AddModelError("logoFile", "Dung lượng ảnh Logo không được vượt quá 2MB!");
                }
                else if (!IsValidImageSignature(logoFile))
                {
                    ModelState.AddModelError("logoFile", "File tải lên không phải là tệp hình ảnh hợp lệ!");
                }
                else
                {
                    string folderPath = Server.MapPath("~/Content/Logos/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string name = Guid.NewGuid() + ext;
                    logoFile.SaveAs(Path.Combine(folderPath, name));
                    uploadedLogoUrl = "/Content/Logos/" + name;
                }
            }

            ModelState.Remove("Logo_Url");

            if (ModelState.IsValid)
            {
                company.UserId = CurrentUserId;
                company.Logo_Url = uploadedLogoUrl ?? "https://via.placeholder.com/150";
                db.COMPANIES.Add(company);
                db.SaveChanges();

                // Đồng bộ CompanyId cho tài khoản Employer vừa tạo công ty
                var currentUser = db.USERS.Find(CurrentUserId);
                if (currentUser != null)
                {
                    currentUser.CompanyId = company.CompanyId;
                    db.SaveChanges();
                }

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
            if (company == null) return RedirectToAction("Create");

            model.CompanyName = (model.CompanyName ?? "").Trim();
            model.Address = (model.Address ?? "").Trim();
            model.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();

            if (model.CompanyName.Length == 0)
            {
                ModelState.AddModelError("CompanyName", "Vui lòng nhập tên công ty.");
            }
            else if (db.COMPANIES.Any(c => c.CompanyId != company.CompanyId && c.CompanyName.ToLower() == model.CompanyName.ToLower()))
            {
                ModelState.AddModelError("CompanyName", "Tên công ty này đã được doanh nghiệp khác sử dụng!");
            }

            if (model.Address.Length == 0)
            {
                ModelState.AddModelError("Address", "Vui lòng nhập địa chỉ trụ sở công ty.");
            }

            string newLogoUrl = null;
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(logoFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("logoFile", "Logo chỉ chấp nhận định dạng ảnh .JPG, .JPEG hoặc .PNG!");
                }
                else if (logoFile.ContentLength > MaxLogoSizeBytes)
                {
                    ModelState.AddModelError("logoFile", "Dung lượng ảnh Logo không được vượt quá 2MB!");
                }
                else if (!IsValidImageSignature(logoFile))
                {
                    ModelState.AddModelError("logoFile", "File tải lên không phải là tệp hình ảnh hợp lệ!");
                }
                else
                {
                    string folderPath = Server.MapPath("~/Content/Logos/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string name = Guid.NewGuid() + ext;
                    logoFile.SaveAs(Path.Combine(folderPath, name));
                    newLogoUrl = "/Content/Logos/" + name;
                }
            }

            if (ModelState.IsValid)
            {
                company.CompanyName = model.CompanyName;
                company.Address = model.Address;
                company.Website = model.Website;
                company.Description = model.Description;
                if (newLogoUrl != null)
                {
                    company.Logo_Url = newLogoUrl;
                }

                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin công ty thành công!";
                return RedirectToAction("MyCompany");
            }

            model.Logo_Url = company.Logo_Url;
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
                TempData["Error"] = "Bảo toàn khóa ngoại: Công ty đang có tin tuyển dụng nên không thể xóa. Vui lòng xóa hết tin tuyển dụng trước!";
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