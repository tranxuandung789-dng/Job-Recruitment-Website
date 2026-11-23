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

        private const int PageSize = 6; // Chuẩn 6 việc làm / 1 trang (3 hàng x 2 cột)

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

        public ActionResult Index(string search, int? categoryId, string location, int page = 1)
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

            // Lọc theo Địa điểm (Cột Aside bên trái: TP.HCM, Hà Nội, Đà Nẵng, hoặc Khu vực khác)
            if (!string.IsNullOrWhiteSpace(location))
            {
                string loc = location.Trim();
                if (loc == "Khác")
                {
                    jobs = jobs.Where(j => !j.Location.Contains("Hà Nội")
                                        && !j.Location.Contains("Hồ Chí Minh")
                                        && !j.Location.Contains("TP.HCM")
                                        && !j.Location.Contains("Đà Nẵng"));
                }
                else if (loc == "Hồ Chí Minh" || loc == "TP.HCM")
                {
                    jobs = jobs.Where(j => j.Location.Contains("Hồ Chí Minh") || j.Location.Contains("TP.HCM"));
                }
                else
                {
                    jobs = jobs.Where(j => j.Location.Contains(loc));
                }
            }

            // Sắp xếp tin mới nhất lên đầu
            var orderedJobs = jobs.OrderByDescending(j => j.PostedDate)
                                  .ThenByDescending(j => j.JobId);

            // Tính toán phân trang (6 việc / trang)
            int totalItems = orderedJobs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / PageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pagedJobs = orderedJobs.Skip((page - 1) * PageSize)
                                       .Take(PageSize)
                                       .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedJobs);
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
                TempData["Success"] = "Đăng tin tuyển dụng mới thành công!";
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
                TempData["Success"] = "Cập nhật tin tuyển dụng thành công!";
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
                    TempData["Success"] = "Đã đóng tin tuyển dụng: " + job.Title;
                }
            }
            return RedirectToAction("MyJobs");
        }

        [HttpPost]
        [RoleAuthorize("Recruiter")]
        public ActionResult Reopen(int id)
        {
            var company = GetRecruiterCompany();
            if (company != null)
            {
                var job = db.JOBS.FirstOrDefault(j => j.JobId == id && j.CompanyId == company.CompanyId);
                if (job != null)
                {
                    job.Status = "Open";
                    // Nếu hạn nộp cũ đã qua thì tự động gia hạn thêm 30 ngày
                    if (job.Deadline < DateTime.Today)
                    {
                        job.Deadline = DateTime.Today.AddDays(30);
                    }
                    db.SaveChanges();
                    TempData["Success"] = "Đã mở lại tin tuyển dụng: " + job.Title;
                }
            }
            return RedirectToAction("MyJobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Delete(int id)
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("Create", "COMPANies");

            var job = db.JOBS.FirstOrDefault(j => j.JobId == id && j.CompanyId == company.CompanyId);
            if (job == null)
            {
                TempData["Error"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền xóa.";
                return RedirectToAction("MyJobs");
            }

            if (db.APPLICATIONS.Any(a => a.JobId == id))
            {
                TempData["Error"] = "Tin tuyển dụng này đã có ứng viên nộp hồ sơ nên không thể xóa. Vui lòng dùng chức năng Đóng tin!";
                return RedirectToAction("MyJobs");
            }

            string title = job.Title;
            db.JOBS.Remove(job);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa tin tuyển dụng: " + title;
            return RedirectToAction("MyJobs");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}