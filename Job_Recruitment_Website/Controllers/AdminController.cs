using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;
using System.Linq;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        // Trang tổng quan Admin (Dashboard)
        public ActionResult Index()
        {
            ViewBag.TotalUsers = db.USERS.Count();
            ViewBag.TotalCompanies = db.COMPANIES.Count();
            ViewBag.TotalJobs = db.JOBS.Count();
            ViewBag.TotalApplications = db.APPLICATIONS.Count();

            var users = db.USERS.OrderByDescending(u => u.CreatedAt).ToList();
            return View(users);
        }

        // Khóa / Mở khóa tài khoản người dùng
        [HttpPost]
        public ActionResult ToggleUserStatus(int id)
        {
            var user = db.USERS.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}