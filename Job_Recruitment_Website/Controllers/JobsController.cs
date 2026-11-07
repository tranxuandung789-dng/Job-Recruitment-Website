using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Job_Recruitment_Website.Models;
using Job_Recruitment_Website.Helpers;

namespace Job_Recruitment_Website.Controllers
{
    public class JobsController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        // Lấy UserId an toàn từ Session
        private int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        // Hàm hỗ trợ tìm Công ty của Nhà tuyển dụng hiện tại (tương thích hoàn toàn với DB mẫu)
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

        // ==========================================
        // 1. DÀNH CHO TẤT CẢ MỌI NGƯỜI (ỨNG VIÊN & KHÁCH)
        // ==========================================

        public ActionResult Index(string search, int? categoryId, string location)
        {
            var jobs = db.JOBS.Include(j => j.COMPANy)
                              .Include(j => j.CATEGORy)
                              .Where(j => j.Status == "Open");

            // Lọc theo từ khóa (Tiêu đề, Tên công ty, Địa điểm, Mô tả, HOẶC Tên ngành nghề)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                jobs = jobs.Where(j => j.Title.Contains(kw)
                                    || j.COMPANy.CompanyName.Contains(kw)
                                    || j.Location.Contains(kw)
                                    || j.Description.Contains(kw)
                                    || j.CATEGORy.CategoryName.Contains(kw));
            }

            // Lọc theo Ngành nghề (Chỉ lọc khi có chọn ID cụ thể và hợp lệ)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId.Value);
            }

            // Lọc theo Địa điểm (Cột Aside bên trái)
            if (!string.IsNullOrWhiteSpace(location))
            {
                string loc = location.Trim();
                jobs = jobs.Where(j => j.Location.Contains(loc));
            }

            return View(jobs.OrderByDescending(j => j.PostedDate).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return RedirectToAction("Index");

            var job = db.JOBS.Include(j => j.COMPANy)
                             .Include(j => j.CATEGORy)
                             .FirstOrDefault(j => j.JobId == id.Value);

            if (job == null) return HttpNotFound();
            return View(job);
        }

        // ==========================================
        // 2. DÀNH RIÊNG CHO NHÀ TUYỂN DỤNG (RECRUITER)
        // ==========================================

        [RoleAuthorize("Recruiter")]
        public ActionResult MyJobs()
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            var myJobs = db.JOBS.Include(j => j.CATEGORy)
                                .Where(j => j.CompanyId == company.CompanyId)
                                .OrderByDescending(j => j.PostedDate)
                                .ToList();
            return View(myJobs);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Create()
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Create(JOB job)
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            // Kiểm tra ràng buộc CK_JOBS_Dates trong SQL: Deadline phải >= PostedDate
            if (job.Deadline < DateTime.Today)
            {
                ModelState.AddModelError("Deadline", "Hạn nộp hồ sơ không được nhỏ hơn ngày hiện tại!");
            }

            if (ModelState.IsValid)
            {
                job.CompanyId = company.CompanyId;
                job.PostedDate = DateTime.Today;
                job.Status = "Open";

                db.JOBS.Add(job);
                db.SaveChanges();
                return RedirectToAction("MyJobs");
            }

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", job.CategoryId);
            return View(job);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("MyJobs");

            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            var job = db.JOBS.FirstOrDefault(j => j.JobId == id.Value && j.CompanyId == company.CompanyId);
            if (job == null) return HttpNotFound();

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", job.CategoryId);
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(JOB model)
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            var job = db.JOBS.FirstOrDefault(j => j.JobId == model.JobId && j.CompanyId == company.CompanyId);
            if (job == null) return HttpNotFound();

            if (model.Deadline < job.PostedDate)
            {
                ModelState.AddModelError("Deadline", "Hạn nộp hồ sơ không được nhỏ hơn ngày đăng tin!");
            }

            if (ModelState.IsValid)
            {
                job.Title = model.Title;
                job.Description = model.Description;
                job.SalaryRange = model.SalaryRange;
                job.Location = model.Location;
                job.Deadline = model.Deadline;
                job.CategoryId = model.CategoryId;

                db.SaveChanges();
                return RedirectToAction("MyJobs");
            }

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [RoleAuthorize("Recruiter")]
        public ActionResult Close(int id)
        {
            var company = GetRecruiterCompany();
            if (company != null)
            {
                var job = db.JOBS.FirstOrDefault(j => j.JobId == id && j.CompanyId == company.CompanyId);
                if (job != null)
                {
                    job.Status = "Closed";
                    db.SaveChanges();
                }
            }
            return RedirectToAction("MyJobs");
        }
    }
}