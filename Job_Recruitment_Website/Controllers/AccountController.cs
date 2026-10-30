
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        // GET: Account/Register
        // Hiển thị giao diện đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // GET: Account/Login
        // Trang đăng nhập
        public ActionResult Login()
        {
            return View();
        }

    }
}
