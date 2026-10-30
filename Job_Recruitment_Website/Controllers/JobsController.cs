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
        private int CurrentUserId { get { return (int)Session["UserId"]; } }

        // ==========================================
        // 1. DÀNH CHO TẤT CẢ MỌI NGƯỜI (ỨNG VIÊN & KHÁCH)
        // ==========================================

        public ActionResult Index(string keyword, int? categoryId)
        {
            // 1. Lấy danh sách việc làm đang mở
            var jobs = db.JOBS.Include(j => j.COMPANy).Include(j => j.CATEGORy)
                              .Where(j => j.Status == "Open").AsQueryable();

            // 2. Lọc theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                jobs = jobs.Where(j => j.Title.Contains(keyword));
            }

            // 3. Lọc theo ngành nghề
            if (categoryId.HasValue)
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId);
            }

            // 4. DÒNG NÀY RẤT QUAN TRỌNG: Truyền danh sách ngành nghề sang View để hết lỗi Dropdown
            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", categoryId);
            ViewBag.Keyword = keyword;

            return View(jobs.OrderByDescending(j => j.PostedDate).ToList());
        }

        public ActionResult Details(int id)
        {
            var job = db.JOBS.Include(j => j.COMPANy).Include(j => j.CATEGORy)
                             .FirstOrDefault(j => j.JobId == id);
            if (job == null) return HttpNotFound();
            return View(job);
        }

        // ==========================================
        // 2. DÀNH RIÊNG CHO NHÀ TUYỂN DỤNG (RECRUITER)
        // ==========================================

        [RoleAuthorize("Recruiter")]
        public ActionResult MyJobs()
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            if (company == null) return RedirectToAction("Create", "Companies");

            var myJobs = db.JOBS.Include(j => j.CATEGORy)
                                .Where(j => j.CompanyId == company.CompanyId).ToList();
            return View(myJobs);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Create()
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            if (company == null) return RedirectToAction("Create", "Companies");

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Create(JOB job)
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            if (ModelState.IsValid)
            {
                job.CompanyId = company.CompanyId;
                job.PostedDate = DateTime.Now;
                job.Status = "Open"; // Thiết lập trạng thái mặc định theo DB

                db.JOBS.Add(job);
                db.SaveChanges();
                return RedirectToAction("MyJobs");
            }
            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", job.CategoryId);
            return View(job);
        }

        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(int id)
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            var job = db.JOBS.FirstOrDefault(j => j.JobId == id && j.CompanyId == company.CompanyId);
            if (job == null) return HttpNotFound();

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", job.CategoryId);
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(JOB model)
        {
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            var job = db.JOBS.FirstOrDefault(j => j.JobId == model.JobId && j.CompanyId == company.CompanyId);
            if (job == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                job.Title = model.Title;
                job.Description = model.Description;
                job.SalaryRange = model.SalaryRange;
                job.Location = model.Location;
                job.Deadline = model.Deadline;
                job.CategoryId = model.CategoryId;
                // Đã loại bỏ dòng gán job.Requirements vì bảng JOBS của bạn không chứa cột này

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
            var company = db.COMPANIES.FirstOrDefault(c => c.UserId == CurrentUserId);
            var job = db.JOBS.FirstOrDefault(j => j.JobId == id && j.CompanyId == company.CompanyId);
            if (job != null)
            {
                job.Status = "Closed"; // Đổi trạng thái theo đúng Constraint CHECK trong bảng SQL
                db.SaveChanges();
            }
            return RedirectToAction("MyJobs");
        }
    }
}