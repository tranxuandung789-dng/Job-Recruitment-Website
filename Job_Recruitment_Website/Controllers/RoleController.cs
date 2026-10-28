using Job_Recruitment_Website.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var role = database.ROLES.Find(id); //select * from De where id=?
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ROLE role, int id)
        {
            var role2 = database.ROLES.Find(id);
            if (role2 == null)
                return View(role);
            else
            {
                role2.RoleName = role.RoleName;
                /*database.Entry(role2).State = EntityState.Modified;*/
                database.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var role = database.ROLES.Find(id); //select * from De where id=?
            return View(role);
        }
        [HttpPost]
        public ActionResult Delete(ROLE role, int id)
        {
            var roleToDelete = database.ROLES.Find(id);
            database.ROLES.Remove(roleToDelete);
            database.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Details(int id)
        {
            var role = database.ROLES.Find(id); //select * from De where id=?
            return View(role);
        }
    }
}