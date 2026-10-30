using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class CompanyController : Controller
    {
        // GET: Company
        // Hiển thị trang danh sách công ty
        public ActionResult Index()
        {
            return View();
        }
    }
}