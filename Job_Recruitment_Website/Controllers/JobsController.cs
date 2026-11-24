using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Job_Recruitment_Website.Models;
using Job_Recruitment_Website.Helpers;

namespace Job_Recruitment_Website.Controllers
{
    public class JobsController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        private const int PageSize = 6; // Chuẩn 6 việc làm / 1 trang (3 hàng x 2 cột)

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

        // Kiểm tra dữ liệu đầu vào dùng chung cho cả Create & Edit (TC12)
        private void ValidateJobInput(JOB job, DateTime minDeadline, string deadlineErrorMsg)
        {
            job.Title = (job.Title ?? "").Trim();
            job.Description = (job.Description ?? "").Trim();
            job.SalaryRange = (job.SalaryRange ?? "").Trim();
            job.Location = (job.Location ?? "").Trim();

            if (job.Title.Length < 5)
            {
                ModelState.AddModelError("Title", "Tiêu đề công việc phải có ít nhất 5 ký tự.");
            }

            if (job.CategoryId <= 0 || !db.CATEGORIES.Any(c => c.CategoryId == job.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn ngành nghề hợp lệ.");
            }

            if (job.SalaryRange.Length == 0)
            {
                ModelState.AddModelError("SalaryRange", "Vui lòng nhập mức lương.");
            }
            else
            {
                bool hasNegativeNumber = Regex.IsMatch(job.SalaryRange, @"(^|[^\d\s])\s*-\s*\d");
                bool hasInvalidChars = !Regex.IsMatch(job.SalaryRange, @"^[\p{L}\d\s\-\/\.,\+\$]+$");
                bool hasLetterOrDigit = Regex.IsMatch(job.SalaryRange, @"[\p{L}\d]");

                if (hasNegativeNumber || hasInvalidChars || !hasLetterOrDigit)
                {
                    ModelState.AddModelError("SalaryRange", "Mức lương không hợp lệ (không được nhập số âm hoặc ký tự đặc biệt như @, #, %...).");
                }
            }

            if (job.Location.Length < 2)
            {
                ModelState.AddModelError("Location", "Vui lòng nhập địa điểm làm việc hợp lệ.");
            }

            if (job.Deadline < minDeadline)
            {
                ModelState.AddModelError("Deadline", deadlineErrorMsg);
            }

            if (job.Description.Length < 20)
            {
                ModelState.AddModelError("Description", "Mô tả chi tiết công việc phải có ít nhất 20 ký tự.");
            }
        }

        // ==========================================
        // 1. DÀNH CHO TẤT CẢ MỌI NGƯỜI (ỨNG VIÊN & KHÁCH - TC01..TC04, TC32)
        // ==========================================
        public ActionResult Index(string search, int? categoryId, string location, int page = 1)
        {
            var jobs = db.JOBS.Include(j => j.COMPANy)
                              .Include(j => j.CATEGORy)
                              .Where(j => j.Status == "Open");

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                jobs = jobs.Where(j => j.Title.Contains(kw)
                                    || j.COMPANy.CompanyName.Contains(kw)
                                    || j.Location.Contains(kw)
                                    || j.Description.Contains(kw)
                                    || j.CATEGORy.CategoryName.Contains(kw));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId.Value);
            }

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

            var orderedJobs = jobs.OrderByDescending(j => j.PostedDate)
                                  .ThenByDescending(j => j.JobId);

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
        // 2. DÀNH RIÊNG CHO NHÀ TUYỂN DỤNG (EMPLOYER - TC11, TC12, TC23, TC27)
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

            ValidateJobInput(job, DateTime.Today, "Hạn nộp hồ sơ không được nhỏ hơn ngày hiện tại!");

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

            var targetJob = db.JOBS.Find(id.Value);
            if (targetJob == null) return HttpNotFound();

            var company = GetRecruiterCompany();
            // Chặn Employer B cố truy cập URL sửa Job của Employer A bằng mã 403 Forbidden (TC23)
            if (company == null || targetJob.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(
                    Response,
                    "Bạn không có quyền chỉnh sửa tin tuyển dụng <strong>#" + targetJob.JobId + " (" + HttpUtility.HtmlEncode(targetJob.Title) + ")</strong> vì tin đăng này thuộc quyền sở hữu của công ty khác!",
                    Url.Action("MyJobs", "Jobs")
                );
            }

            ViewBag.CategoryId = new SelectList(db.CATEGORIES, "CategoryId", "CategoryName", targetJob.CategoryId);
            return View(targetJob);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Edit(JOB model)
        {
            var existingJob = db.JOBS.Find(model.JobId);
            if (existingJob == null) return HttpNotFound();

            var company = GetRecruiterCompany();
            if (company == null || existingJob.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(
                    Response,
                    "Từ chối lưu thay đổi: Bạn không sở hữu tin tuyển dụng <strong>#" + existingJob.JobId + "</strong>!",
                    Url.Action("MyJobs", "Jobs")
                );
            }

            model.PostedDate = existingJob.PostedDate;
            ValidateJobInput(model, existingJob.PostedDate, "Hạn nộp hồ sơ không được nhỏ hơn ngày đăng tin (" + existingJob.PostedDate.ToString("dd/MM/yyyy") + ")!");

            if (ModelState.IsValid)
            {
                existingJob.Title = model.Title;
                existingJob.Description = model.Description;
                existingJob.SalaryRange = model.SalaryRange;
                existingJob.Location = model.Location;
                existingJob.Deadline = model.Deadline;
                existingJob.CategoryId = model.CategoryId;

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
            var job = db.JOBS.Find(id);
            if (job == null) return RedirectToAction("MyJobs");

            var company = GetRecruiterCompany();
            if (company == null || job.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(Response, "Bạn không có quyền đóng tin tuyển dụng của công ty khác!", Url.Action("MyJobs", "Jobs"));
            }

            job.Status = "Closed";
            db.SaveChanges();
            TempData["Success"] = "Đã đóng tin tuyển dụng: " + job.Title;
            return RedirectToAction("MyJobs");
        }

        [HttpPost]
        [RoleAuthorize("Recruiter")]
        public ActionResult Reopen(int id)
        {
            var job = db.JOBS.Find(id);
            if (job == null) return RedirectToAction("MyJobs");

            var company = GetRecruiterCompany();
            if (company == null || job.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(Response, "Bạn không có quyền mở lại tin tuyển dụng của công ty khác!", Url.Action("MyJobs", "Jobs"));
            }

            job.Status = "Open";
            if (job.Deadline < DateTime.Today)
            {
                job.Deadline = DateTime.Today.AddDays(30);
            }
            db.SaveChanges();
            TempData["Success"] = "Đã mở lại tin tuyển dụng: " + job.Title;
            return RedirectToAction("MyJobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult Delete(int id)
        {
            var job = db.JOBS.Find(id);
            if (job == null)
            {
                TempData["Error"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction("MyJobs");
            }

            var company = GetRecruiterCompany();
            if (company == null || job.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(Response, "Bạn không có quyền xóa tin tuyển dụng của công ty khác!", Url.Action("MyJobs", "Jobs"));
            }

            if (db.APPLICATIONS.Any(a => a.JobId == id))
            {
                TempData["Error"] = "Bảo toàn khóa ngoại: Tin tuyển dụng này đã có ứng viên nộp hồ sơ nên không thể xóa. Vui lòng dùng chức năng Đóng tin!";
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