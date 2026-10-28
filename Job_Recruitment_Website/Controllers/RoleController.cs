using Job_Recruitment_Website.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Job_Recruitment_Website.Controllers
{
    public class RoleController : Controller
    {
        // Khởi tạo db
        RecruitmentDBEntities database = new RecruitmentDBEntities();

        // GET: Role
        public ActionResult Index()
        {
            return View(database.ROLES.ToList());
        }

        // GET: Role/Details/5
        public ActionResult Details(int? id)
        {
            // Kiểm tra id hợp lệ (theo ràng buộc CK_ROLES_RoleId > 0)
            if (id == null || id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var role = database.ROLES.Find(id);
            if (role == null)
                return HttpNotFound();

            return View(role);
        }

        // GET: Role/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ROLE role)
        {
            // 1. Cắt khoảng trắng thừa ở 2 đầu (khớp với LTRIM/RTRIM của CK_ROLES_RoleName)
            role.RoleName = role.RoleName?.Trim();

            // 2. Kiểm tra rỗng và độ dài tối đa NVARCHAR(100)
            if (string.IsNullOrEmpty(role.RoleName))
            {
                ModelState.AddModelError("RoleName", "Tên vai trò không được để trống.");
            }
            else if (role.RoleName.Length > 100)
            {
                ModelState.AddModelError("RoleName", "Tên vai trò không được vượt quá 100 ký tự.");
            }
            // 3. Kiểm tra trùng tên (khớp với UQ_ROLES_RoleName)
            else if (database.ROLES.Any(r => r.RoleName == role.RoleName))
            {
                ModelState.AddModelError("RoleName", "Tên vai trò này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                database.ROLES.Add(role);
                database.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // GET: Role/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var role = database.ROLES.Find(id);
            if (role == null)
                return HttpNotFound();

            return View(role);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ROLE role, int id)
        {
            var existingRole = database.ROLES.Find(id);
            if (existingRole == null)
                return HttpNotFound();

            // 1. Cắt khoảng trắng thừa
            role.RoleName = role.RoleName?.Trim();

            // 2. Kiểm tra rỗng và độ dài NVARCHAR(100)
            if (string.IsNullOrEmpty(role.RoleName))
            {
                ModelState.AddModelError("RoleName", "Tên vai trò không được để trống.");
            }
            else if (role.RoleName.Length > 100)
            {
                ModelState.AddModelError("RoleName", "Tên vai trò không được vượt quá 100 ký tự.");
            }
            // 3. Kiểm tra trùng tên với các Role KHÁC (trừ chính Role đang sửa: r.RoleId != id)
            else if (database.ROLES.Any(r => r.RoleName == role.RoleName && r.RoleId != id))
            {
                ModelState.AddModelError("RoleName", "Tên vai trò này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                existingRole.RoleName = role.RoleName;
                database.SaveChanges();
                return RedirectToAction("Index");
            }

            // Gán lại RoleId để View giữ đúng id khi hiển thị thông báo lỗi
            role.RoleId = id;
            return View(role);
        }

        // GET: Role/Delete/5
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var role = database.ROLES.Find(id);
            if (role == null)
                return HttpNotFound();

            return View(role);
        }

        // POST: Role/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ROLE role, int id)
        {
            var roleToDelete = database.ROLES.Find(id);
            if (roleToDelete == null)
                return HttpNotFound();

            // Kiểm tra khóa ngoại FK_USERS_ROLES trước khi xóa
            bool isUsedByUsers = database.USERS.Any(u => u.RoleId == id);
            if (isUsedByUsers)
            {
                ModelState.AddModelError("", "Không thể xóa vai trò này vì đang có tài khoản người dùng sử dụng.");
                return View(roleToDelete);
            }

            database.ROLES.Remove(roleToDelete);
            database.SaveChanges();
            return RedirectToAction("Index");
        }

        // Giải phóng kết nối DbContext sau khi hoàn tất request
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                database.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}