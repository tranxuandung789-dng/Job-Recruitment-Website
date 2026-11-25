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

        private const int MaxCvSizeBytes = 5 * 1024 * 1024; // 5MB

        private static readonly string[] ValidStatuses =
            { "Pending", "Submitted", "Reviewed", "Interview", "Accepted", "Approved", "Rejected" };

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

        // Kiểm tra chữ ký số chuẩn của PDF "%PDF-" (TC09, TC30)
        public static bool IsPdfFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength < 5)
                return false;

            try
            {
                byte[] header = new byte[5];
                file.InputStream.Position = 0;
                int read = file.InputStream.Read(header, 0, 5);
                file.InputStream.Position = 0;

                return read == 5
                    && header[0] == 0x25
                    && header[1] == 0x50
                    && header[2] == 0x44
                    && header[3] == 0x46
                    && header[4] == 0x2D;
            }
            catch
            {
                return false;
            }
        }

        // Ưu tiên lấy bản chụp Snapshot CV của đúng đơn ứng tuyển (app_{id}.pdf), sau đó mới fallback về CV hồ sơ
        private string ResolveApplicationCvPath(int applicationId, string fallbackCvUrl)
        {
            string snapshotPath = Path.Combine(Server.MapPath("~/App_Data/CVs/"), "app_" + applicationId + ".pdf");
            if (System.IO.File.Exists(snapshotPath))
                return snapshotPath;

            if (string.IsNullOrWhiteSpace(fallbackCvUrl) || fallbackCvUrl == "Đang cập nhật")
                return null;

            if (fallbackCvUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                fallbackCvUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return null;

            string fileName = Path.GetFileName(fallbackCvUrl);
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string appDataPath = Path.Combine(Server.MapPath("~/App_Data/CVs/"), fileName);
            if (System.IO.File.Exists(appDataPath))
                return appDataPath;

            string legacyContentPath = Path.Combine(Server.MapPath("~/Content/CVs/"), fileName);
            if (System.IO.File.Exists(legacyContentPath))
                return legacyContentPath;

            return null;
        }

        private static bool IsValidTransition(string currentStatus, string nextStatus)
        {
            if (currentStatus == "Pending" || currentStatus == "Submitted")
                return nextStatus == "Reviewed" || nextStatus == "Rejected";

            if (currentStatus == "Reviewed")
                return nextStatus == "Interview" || nextStatus == "Rejected";

            if (currentStatus == "Interview")
                return nextStatus == "Accepted" || nextStatus == "Approved" || nextStatus == "Rejected";

            if (currentStatus == "Accepted" || currentStatus == "Approved" || currentStatus == "Rejected")
                return nextStatus == "Reviewed";

            return false;
        }

        private static string FormatRubricStatus(string status)
        {
            switch (status)
            {
                case "Pending":
                case "Submitted": return "Applied (Chờ duyệt)";
                case "Reviewed": return "Review (Đang xem xét)";
                case "Interview": return "Interview (Mời phỏng vấn)";
                case "Accepted":
                case "Approved": return "Accepted (Đã nhận việc)";
                case "Rejected": return "Rejected (Đã từ chối)";
                default: return status;
            }
        }

        // ========================================================
        // XEM FILE CV BẢO VỆ PHÂN QUYỀN (TC30)
        // ========================================================
        public ActionResult ViewCv(int? id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ViewCv", "Applications", new { id = id }) });
            }

            if (id == null) return HttpNotFound();

            var app = db.APPLICATIONS
                        .Include(a => a.JOB)
                        .Include(a => a.CANDIDATEPROFILE)
                        .FirstOrDefault(a => a.ApplicationId == id.Value);

            if (app == null) return HttpNotFound();

            string role = Session["Role"] as string;
            int uid = CurrentUserId;
            bool isAuthorized = false;

            if (role == "Admin")
            {
                isAuthorized = true;
            }
            else if (role == "Candidate")
            {
                isAuthorized = (app.UserId == uid);
            }
            else if (role == "Recruiter")
            {
                var company = GetRecruiterCompany();
                if (company != null && app.JOB != null && app.JOB.CompanyId == company.CompanyId)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(
                    Response,
                    "Bạn không có quyền truy cập file CV của đơn ứng tuyển <strong>#" + app.ApplicationId + "</strong> vì hồ sơ này thuộc quyền quản lý của nhà tuyển dụng khác."
                );
            }

            string cvUrl = app.CANDIDATEPROFILE != null ? app.CANDIDATEPROFILE.CV_Url : null;
            string physicalPath = ResolveApplicationCvPath(app.ApplicationId, cvUrl);

            if (physicalPath == null)
            {
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return Content(
                    "<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'/><title>Không tìm thấy file CV</title></head>" +
                    "<body style='font-family:Segoe UI,Arial,sans-serif;background:#f8f9fc;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;'>" +
                    "<div style='background:#fff;border:1px solid #e2e8f0;border-radius:14px;padding:32px 40px;max-width:520px;text-align:center;box-shadow:0 8px 24px rgba(0,0,0,0.06);'>" +
                    "<h3 style='color:#1e293b;margin:0 0 10px;'>Chưa có file PDF thực tế trên máy chủ</h3>" +
                    "<p style='color:#64748b;line-height:1.6;margin:0;'>Đơn ứng tuyển này đang dùng dữ liệu mẫu ban đầu. Hãy dùng tài khoản Ứng viên tải lên file PDF thật để xem CV.</p>" +
                    "</div></body></html>",
                    "text/html"
                );
            }

            Response.AppendHeader("Content-Disposition", "inline; filename=\"CV_UngVien_" + app.ApplicationId + ".pdf\"");
            return File(physicalPath, "application/pdf");
        }

        // ========================================================
        // 1. DÀNH CHO ỨNG VIÊN (CANDIDATE - TC13, TC14, TC21, TC22)
        // ========================================================
        public ActionResult Apply(int? jobId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Apply", "Applications", new { jobId = jobId }) });
            }

            string role = Session["Role"] as string;
            if (role != "Candidate")
            {
                TempData["Error"] = "Tài khoản Nhà tuyển dụng (Employer) hoặc Quản trị viên (Admin) không thể nộp hồ sơ ứng tuyển. Vui lòng sử dụng tài khoản Ứng viên (Candidate)!";
                return RedirectToAction("Index", "Jobs");
            }

            if (jobId == null) return RedirectToAction("Index", "Jobs");

            var job = db.JOBS.Include(j => j.COMPANy)
                             .FirstOrDefault(j => j.JobId == jobId.Value && j.Status == "Open");
            if (job == null)
            {
                TempData["Error"] = "Công việc không tồn tại hoặc đã đóng tuyển dụng.";
                return RedirectToAction("Index", "Jobs");
            }

            if (job.Deadline < DateTime.Today)
            {
                TempData["Error"] = "Tin tuyển dụng này đã hết hạn nộp hồ sơ.";
                return RedirectToAction("Details", "Jobs", new { id = jobId.Value });
            }

            int uid = CurrentUserId;
            if (db.APPLICATIONS.Any(a => a.JobId == jobId.Value && a.UserId == uid))
            {
                TempData["Error"] = "Bạn đã nộp hồ sơ vào vị trí này rồi!";
                return RedirectToAction("Details", "Jobs", new { id = jobId.Value });
            }

            ViewBag.Job = job;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Candidate")]
        public ActionResult Apply(int jobId, string coverLetter, HttpPostedFileBase cvFile)
        {
            var job = db.JOBS.Include(j => j.COMPANy)
                             .FirstOrDefault(j => j.JobId == jobId && j.Status == "Open");
            if (job == null) return HttpNotFound();

            if (job.Deadline < DateTime.Today)
            {
                TempData["Error"] = "Tin tuyển dụng này đã hết hạn nộp hồ sơ.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            int uid = CurrentUserId;

            if (db.APPLICATIONS.Any(a => a.JobId == jobId && a.UserId == uid))
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
                    ModelState.AddModelError("cvFile", "Từ chối tải lên: Hệ thống chỉ chấp nhận định dạng file PDF (.pdf)!");
                }
                else if (cvFile.ContentLength > MaxCvSizeBytes)
                {
                    ModelState.AddModelError("cvFile", "Từ chối tải lên: Dung lượng file CV không được vượt quá 5MB!");
                }
                else if (!IsPdfFile(cvFile))
                {
                    ModelState.AddModelError("cvFile", "Từ chối tải lên: Phát hiện file giả mạo PDF (nội dung bên trong không đúng chuẩn chữ ký %PDF-)!");
                }
            }

            if (ModelState.IsValid)
            {
                string folderPath = Server.MapPath("~/App_Data/CVs/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Guid.NewGuid().ToString() + ".pdf";
                string savePath = Path.Combine(folderPath, fileName);
                cvFile.SaveAs(savePath);

                string cvVirtualPath = "/App_Data/CVs/" + fileName;

                try
                {
                    var profile = db.CANDIDATEPROFILES.FirstOrDefault(p => p.UserId == uid);
                    if (profile == null)
                    {
                        profile = new CANDIDATEPROFILE
                        {
                            UserId = uid,
                            Education = "Đang cập nhật",
                            Skills = "Đang cập nhật",
                            Experience = "Đang cập nhật",
                            CV_Url = cvVirtualPath,
                            UpdatedAt = DateTime.Today
                        };
                        db.CANDIDATEPROFILES.Add(profile);
                    }
                    else
                    {
                        profile.CV_Url = cvVirtualPath;
                        profile.UpdatedAt = DateTime.Today;
                    }
                    db.SaveChanges();

                    APPLICATION app = new APPLICATION
                    {
                        JobId = jobId,
                        UserId = uid,
                        CandidateId = profile.ProfileId,
                        CoverLetter = coverLetter,
                        Status = "Pending",
                        AppliedDate = DateTime.Today
                    };

                    db.APPLICATIONS.Add(app);
                    db.SaveChanges();

                    // Lưu bản chụp (Snapshot) CV gắn riêng với ApplicationId tại thời điểm nộp đơn
                    string snapshotPath = Path.Combine(folderPath, "app_" + app.ApplicationId + ".pdf");
                    System.IO.File.Copy(savePath, snapshotPath, true);
                }
                catch (Exception)
                {
                    if (System.IO.File.Exists(savePath))
                    {
                        System.IO.File.Delete(savePath);
                    }
                    ModelState.AddModelError("", "Không thể nộp hồ sơ lúc này, vui lòng thử lại.");
                    ViewBag.Job = job;
                    return View();
                }

                TempData["Success"] = "Nộp hồ sơ ứng tuyển thành công (Trạng thái khởi tạo: Applied)!";
                return RedirectToAction("MyApplications");
            }

            ViewBag.Job = job;
            return View();
        }

        public ActionResult MyApplications()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyApplications", "Applications") });
            }

            int uid = CurrentUserId;
            var list = db.APPLICATIONS
                         .Include(a => a.JOB)
                         .Include(a => a.JOB.COMPANy)
                         .Include(a => a.CANDIDATEPROFILE)
                         .Where(a => a.UserId == uid)
                         .OrderByDescending(a => a.AppliedDate)
                         .ToList();
            return View(list);
        }

        // ========================================================
        // 2. DÀNH CHO NHÀ TUYỂN DỤNG (EMPLOYER - TC15, TC16, TC24, TC25, TC26)
        // ========================================================
        [RoleAuthorize("Recruiter")]
        public ActionResult Candidates(int? jobId)
        {
            if (jobId == null) return RedirectToAction("MyJobs", "Jobs");

            var targetJob = db.JOBS.Find(jobId.Value);
            if (targetJob == null) return HttpNotFound();

            var company = GetRecruiterCompany();
            // Chặn Employer B mở URL xem ứng viên thuộc Job của Employer A bằng mã 403 Forbidden (TC24)
            if (company == null || targetJob.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(
                    Response,
                    "Bạn không có quyền xem hoặc duyệt danh sách ứng viên của tin tuyển dụng <strong>#" + targetJob.JobId + " (" + HttpUtility.HtmlEncode(targetJob.Title) + ")</strong> vì tin này thuộc quyền quản lý của công ty khác!",
                    Url.Action("MyJobs", "Jobs")
                );
            }

            ViewBag.JobTitle = targetJob.Title;
            ViewBag.JobId = targetJob.JobId;

            var applicants = db.APPLICATIONS
                               .Include(a => a.USER)
                               .Include(a => a.CANDIDATEPROFILE)
                               .Where(a => a.JobId == jobId.Value)
                               .OrderByDescending(a => a.AppliedDate)
                               .ToList();
            return View(applicants);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Recruiter")]
        public ActionResult UpdateStatus(int applicationId, string newStatus)
        {
            var app = db.APPLICATIONS.Include(a => a.JOB)
                                     .FirstOrDefault(a => a.ApplicationId == applicationId);
            if (app == null) return RedirectToAction("MyJobs", "Jobs");

            var company = GetRecruiterCompany();
            if (company == null || app.JOB == null || app.JOB.CompanyId != company.CompanyId)
            {
                return RoleAuthorizeAttribute.ForbiddenResult(
                    Response,
                    "Từ chối cập nhật: Đơn ứng tuyển <strong>#" + applicationId + "</strong> không thuộc quyền quản lý của công ty bạn!",
                    Url.Action("MyJobs", "Jobs")
                );
            }

            if (ValidStatuses.Contains(newStatus) && IsValidTransition(app.Status, newStatus))
            {
                string oldLabel = FormatRubricStatus(app.Status);
                app.Status = newStatus;
                db.SaveChanges();
                TempData["Success"] = "Đã chuyển trạng thái hồ sơ #" + app.ApplicationId + " từ [" + oldLabel + "] sang [" + FormatRubricStatus(newStatus) + "]!";
            }
            else
            {
                TempData["Error"] = "Từ chối chuyển trạng thái sai quy trình: Không thể nhảy từ [" + FormatRubricStatus(app.Status) + "] sang [" + FormatRubricStatus(newStatus) + "]. Quy trình chuẩn: Applied → Review → Interview → Accepted / Rejected!";
            }

            return RedirectToAction("Candidates", new { jobId = app.JobId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}