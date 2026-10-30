using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class JobController : Controller
    {
        // GET: Job
        // Hiển thị trang danh sách việc làm
        public ActionResult Index()
        {
            return View();
        }

        // GET: Job/Details
        // Hiển thị trang chi tiết việc làm
        public ActionResult Details(int id)
        {
            if (id == 1)
            {
                ViewBag.Job = "Nhân viên kinh doanh";
                ViewBag.Company = "Công ty KIM HỒNG";
                ViewBag.Location = "Thành phố Hồ Chí Minh";
                ViewBag.Salary = "8 - 12 triệu VNĐ";
                ViewBag.Description = "Tìm kiếm khách hàng, tư vấn sản phẩm và chăm sóc khách hàng.";
            }
            else if (id == 2)
            {
                ViewBag.Job = "Nhân viên Marketing";
                ViewBag.Company = "Công ty TNHH";
                ViewBag.Location = "Hà Nội";
                ViewBag.Salary = "10 - 15 triệu VNĐ";
                ViewBag.Description = "Lên kế hoạch Marketing, quản lý nội dung và quảng bá thương hiệu.";
            }
            else if (id == 3)
            {
                ViewBag.Job = "Lập trình viên Web";
                ViewBag.Company = "Công ty DEF";
                ViewBag.Location = "Đà Nẵng";
                ViewBag.Salary = "15 - 20 triệu VNĐ";
                ViewBag.Description = "Phát triển website, sửa lỗi và bảo trì hệ thống.";
            }
            else
            {
                return HttpNotFound();
            }

            return View();
        }
    }
}