using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Job_Recruitment_Website.Helpers;
using Job_Recruitment_Website.Models;

namespace Job_Recruitment_Website.Controllers
{
    public class CATEGORiesController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        // GET: CATEGORies (Ai cũng xem được danh sách ngành nghề)
        public ActionResult Index()
        {
            return View(db.CATEGORIES.ToList());
        }

        // GET: CATEGORies/Details/5
        [RoleAuthorize("Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null) return HttpNotFound();
            return View(cATEGORy);
        }

        // GET: CATEGORies/Create
        [RoleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CATEGORies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin")]
        public ActionResult Create([Bind(Include = "CategoryId,CategoryName")] CATEGORy cATEGORy)
        {
            if (ModelState.IsValid)
            {
                db.CATEGORIES.Add(cATEGORy);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cATEGORy);
        }

        // GET: CATEGORies/Edit/5
        [RoleAuthorize("Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null) return HttpNotFound();
            return View(cATEGORy);
        }

        // POST: CATEGORies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin")]
        public ActionResult Edit([Bind(Include = "CategoryId,CategoryName")] CATEGORy cATEGORy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cATEGORy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cATEGORy);
        }

        // GET: CATEGORies/Delete/5
        [RoleAuthorize("Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null) return HttpNotFound();
            return View(cATEGORy);
        }

        // POST: CATEGORies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy != null && !db.JOBS.Any(j => j.CategoryId == id))
            {
                db.CATEGORIES.Remove(cATEGORy);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}