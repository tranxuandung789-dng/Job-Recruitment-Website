using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class ApplicationsController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        private int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        // Hàm hỗ trợ tìm Công ty của Nhà tuyển dụng hiện tại
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
        // 1. DÀNH CHO ỨNG VIÊN (CANDIDATE)
        // ========================================================

        // GET: Hiển thị form nộp đơn ứng tuyển
        [RoleAuthorize("Candidate")]
        public ActionResult Apply(int? jobId)
        {
            if (jobId == null) return RedirectToAction("Index", "Jobs");

            var job = db.JOBS.Include(j => j.COMPANy)
                             .FirstOrDefault(j => j.JobId == jobId.Value && j.Status == "Open");
            if (job == null)
            {
                TempData["Error"] = "Công việc không tồn tại hoặc đã đóng tuyển dụng.";
                return RedirectToAction("Index", "Jobs");
            }

            // Kiểm tra ứng viên đã từng nộp vào việc này chưa
            bool alreadyApplied = db.APPLICATIONS.Any(a => a.JobId == jobId.Value && a.UserId == CurrentUserId);
            if (alreadyApplied)
            {
                TempData["Error"] = "Bạn đã nộp hồ sơ vào vị trí này rồi!";
                return RedirectToAction("Details", "Jobs", new { id = jobId.Value });
            }

            ViewBag.Job = job;
            return View();
        }

        // POST: Xử lý upload file CV và lưu vào database
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Candidate")]
        public ActionResult Apply(int jobId, string coverLetter, HttpPostedFileBase cvFile)
        {
            var job = db.JOBS.Include(j => j.COMPANy)
                             .FirstOrDefault(j => j.JobId == jobId && j.Status == "Open");
            if (job == null) return HttpNotFound();

            // Chặn nộp trùng ở phía POST (Tránh lỗi UQ_APPLICATIONS_Job_User trong SQL)
            if (db.APPLICATIONS.Any(a => a.JobId == jobId && a.UserId == CurrentUserId))
            {
                TempData["Error"] = "Bạn đã nộp hồ sơ vào vị trí này rồi!";
                return RedirectToAction("MyApplications");
            }

            if (cvFile == null || cvFile.ContentLength == 0)
            {
                ModelState.AddModelError("cvFile", "Vui lòng đính kèm file CV của bạn.");
            }
            else
            {
                string ext = Path.GetExtension(cvFile.FileName).ToLower();
                if (ext != ".pdf")
                {
                    ModelState.AddModelError("cvFile", "Hệ thống chỉ chấp nhận định dạng file PDF.");
                }
                else if (cvFile.ContentLength > 5 * 1024 * 1024) // Giới hạn 5MB
                {
                    ModelState.AddModelError("cvFile", "Dung lượng file CV không được vượt quá 5MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Đảm bảo thư mục ~/Content/CVs/ luôn tồn tại trước khi lưu file
                string folderPath = Server.MapPath("~/Content/CVs/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName).ToLower();
                string savePath = Path.Combine(folderPath, fileName);
                cvFile.SaveAs(savePath);

                string cvVirtualPath = "/Content/CVs/" + fileName;

                // Cập nhật hoặc tạo mới hồ sơ CANDIDATEPROFILES để lưu CV_Url và lấy ProfileId
                var profile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == CurrentUserId);
                if (profile == null)
                {
                    profile = new CANDIDATEPROFILE
                    {
                        UserId = CurrentUserId,
                        Education = "Đang cập nhật",
                        Skills = "Đang cập nhật",
                        Experience = "Đang cập nhật",
                        CV_Url = cvVirtualPath,
                        UpdatedAt = DateTime.Today
                    };
                    db.CANDIDATEPROFILES.Add(profile);
                    db.SaveChanges();
                }
                else
                {
                    profile.CV_Url = cvVirtualPath;
                    profile.UpdatedAt = DateTime.Today;
                    db.SaveChanges(); // Lưu cập nhật CV mới nhất vào Profile
                }

                // Khởi tạo đơn ứng tuyển (Tuyệt đối KHÔNG gán CV_Url ở đây để tránh lỗi CS0117)
                APPLICATION app = new APPLICATION
                {
                    JobId = jobId,
                    UserId = CurrentUserId,
                    CandidateId = profile.ProfileId,
                    CoverLetter = coverLetter,
                    Status = "Pending",
                    AppliedDate = DateTime.Today
                };

                db.APPLICATIONS.Add(app);
                db.SaveChanges();

                TempData["Success"] = "Nộp hồ sơ ứng tuyển thành công!";
                return RedirectToAction("MyApplications");
            }

            ViewBag.Job = job;
            return View();
        }

        // GET: Xem lịch sử các công việc đã nộp
        public ActionResult MyApplications()
        {
            // Kiểm tra đăng nhập và tự động đẩy về trang Login kèm returnUrl
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyApplications", "Applications") });
            }

            var list = db.APPLICATIONS
                         .Include(a => a.JOB)
                         .Include(a => a.JOB.COMPANy)
                         .Where(a => a.UserId == CurrentUserId)
                         .OrderByDescending(a => a.AppliedDate)
                         .ToList();
            return View(list);
        }

        // ========================================================
        // 2. DÀNH CHO NHÀ TUYỂN DỤNG (RECRUITER)
        // ========================================================

        // GET: Xem danh sách ứng viên đã nộp vào 1 tin tuyển dụng cụ thể
        [RoleAuthorize("Recruiter")]
        public ActionResult Candidates(int? jobId)
        {
            if (jobId == null) return RedirectToAction("MyJobs", "Jobs");

            var company = GetRecruiterCompany();
            if (company == null) return HttpNotFound();

            // Kiểm tra xem tin tuyển dụng này có thuộc về công ty của Recruiter đang đăng nhập không
            var job = db.JOBS.FirstOrDefault(j => j.JobId == jobId.Value && j.CompanyId == company.CompanyId);
            if (job == null) return HttpNotFound();

            ViewBag.JobTitle = job.Title;
            ViewBag.JobId = job.JobId;

            var applicants = db.APPLICATIONS
                               .Include(a => a.USER)
                               .Where(a => a.JobId == jobId.Value)
                               .OrderByDescending(a => a.AppliedDate)
                               .ToList();
            return View(applicants);
        }

        // POST: Cập nhật trạng thái duyệt/từ chối hồ sơ
        [HttpPost]
        [RoleAuthorize("Recruiter")]
        public ActionResult UpdateStatus(int applicationId, string newStatus)
        {
            var company = GetRecruiterCompany();
            if (company == null) return RedirectToAction("MyJobs", "Jobs");

            var app = db.APPLICATIONS.Include(a => a.JOB)
                                     .FirstOrDefault(a => a.ApplicationId == applicationId
                                                       && a.JOB.CompanyId == company.CompanyId);
            if (app != null)
            {
                // Kiểm tra trạng thái hợp lệ theo đúng CHECK Constraint CK_APPLICATIONS_Status trong SQL
                string[] validStatuses = { "Pending", "Submitted", "Reviewed", "Interview", "Accepted", "Approved", "Rejected" };
                if (validStatuses.Contains(newStatus))
                {
                    app.Status = newStatus;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Candidates", new { jobId = app != null ? app.JobId : 0 });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}