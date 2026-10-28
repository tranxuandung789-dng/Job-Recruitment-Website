using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Job_Recruitment_Website.Models;

namespace Job_Recruitment_Website.Controllers
{
    public class RoleController : Controller
    {
        //khoi tao db
        RecruitmentDBEntities database = new RecruitmentDBEntities();
        // GET: Role
        public ActionResult Index()
        {
            return View(database.ROLES.ToList());
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(ROLE role)
        {
            if (ModelState.IsValid)
            {
                database.ROLES.Add(role);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(role);
        }
    }