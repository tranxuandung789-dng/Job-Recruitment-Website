using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System;
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

        // ========================================================
        // 1. DÀNH CHO ỨNG VIÊN (CANDIDATE)
        // ========================================================

        // GET: Hiển thị form nộp đơn ứng tuyển
        [RoleAuthorize("Candidate")]
        public ActionResult Apply(int jobId)
        {
            var job = db.JOBS.FirstOrDefault(j => j.JobId == jobId && j.Status == "Open");
            if (job == null)
            {
                TempData["Error"] = "Công việc không tồn tại hoặc đã đóng tuyển dụng.";
                return RedirectToAction("Index", "Jobs");
            }

            // Kiểm tra ứng viên đã từng nộp vào việc này chưa
            bool alreadyApplied = db.APPLICATIONS.Any(a => a.JobId == jobId && a.UserId == CurrentUserId);
            if (alreadyApplied)
            {
                TempData["Error"] = "Bạn đã nộp hồ sơ vào vị trí này rồi!";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
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
            var job = db.JOBS.FirstOrDefault(j => j.JobId == jobId && j.Status == "Open");
            if (job == null) return HttpNotFound();

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
                // Lưu file CV vào thư mục ~/Content/CVs/
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName).ToLower();
                string savePath = Server.MapPath("~/Content/CVs/" + fileName);
                cvFile.SaveAs(savePath);

                APPLICATION app = new APPLICATION
                {
                    JobId = jobId,
                    UserId = CurrentUserId,
                    CV_Url = "/Content/CVs/" + fileName,
                    CoverLetter = coverLetter,
                    Status = "Submitted", // Trạng thái mặc định khi nộp
                    AppliedDate = DateTime.Now
                };

                db.APPLICATIONS.Add(app);
                db.SaveChanges();

                TempData["Success"] = "Nộp hồ sơ ứng tuyển thành công!";
                return RedirectToAction("MyApplications");
            }

            ViewBag.Job = job;
            return View();
        }

        // GET: Xem lịch sử các công việc ứng viên đã nộp
        [RoleAuthorize("Candidate")]
        public ActionResult MyApplications()
        {
            var list = db.APPLICATIONS
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
        public ActionResult Candidates(int jobId)
        {
            // Kiểm tra xem tin tuyển dụng này có thuộc về công ty của Recruiter đang đăng nhập không
            var job = db.JOBS.FirstOrDefault(j => j.JobId == jobId && j.COMPANy.UserId == CurrentUserId);
            if (job == null) return HttpNotFound();

            ViewBag.JobTitle = job.Title;
            ViewBag.JobId = job.JobId;

            var applicants = db.APPLICATIONS
                               .Where(a => a.JobId == jobId)
                               .OrderByDescending(a => a.AppliedDate)
                               .ToList();
            return View(applicants);
        }

        // POST: Cập nhật trạng thái duyệt/từ chối hồ sơ
        [HttpPost]
        [RoleAuthorize("Recruiter")]
        public ActionResult UpdateStatus(int applicationId, string newStatus)
        {
            var app = db.APPLICATIONS.FirstOrDefault(a => a.ApplicationId == applicationId
                                                       && a.JOB.COMPANy.UserId == CurrentUserId);
            if (app != null)
            {
                app.Status = newStatus; // "Approved" hoặc "Rejected"
                db.SaveChanges();
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