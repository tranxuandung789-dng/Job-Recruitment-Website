using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Job_Recruitment_Website.Models;

namespace Job_Recruitment_Website.Controllers
{
    public class CATEGORiesController : Controller
    {
        private RecruitmentDBEntities db = new RecruitmentDBEntities();

        // GET: CATEGORies
        public ActionResult Index()
        {
            return View(db.CATEGORIES.ToList());
        }

        // GET: CATEGORies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORy);
        }

        // GET: CATEGORies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CATEGORies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORy);
        }

        // POST: CATEGORies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            if (cATEGORy == null)
            {
                return HttpNotFound();
            }
            return View(cATEGORy);
        }

        // POST: CATEGORies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CATEGORy cATEGORy = db.CATEGORIES.Find(id);
            db.CATEGORIES.Remove(cATEGORy);
            db.SaveChanges();
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
