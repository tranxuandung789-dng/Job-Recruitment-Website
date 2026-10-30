using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class AboutController : Controller
    {
        // GET: About
        // Hiển thị trang giới thiệu về trang web
        public ActionResult Index()
        {
            return View();
        }
    }
}