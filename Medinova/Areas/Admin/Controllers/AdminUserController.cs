using Medinova.Filters;
using Medinova.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminUserController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Admin/AdminUser
        public ActionResult Index()
        {
            var users = context.Users
                .Include(u => u.Roles)
                .Include(u => u.Doctor)
                .OrderBy(u => u.FirstName)
                .ToList();

            return View(users);
        }

        // GET: Admin/AdminUser/AssignRole/5
        public ActionResult AssignRole(int id)
        {
            var user = context.Users
                .Include(u => u.Roles)
                .Include(u => u.Doctor)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var allRoles = context.Roles.ToList();
            var userRoleIds = user.Roles.Select(r => r.RoleId).ToList();
            
            // Doktor listesi
            var doctors = context.Doctors
                .Include(d => d.Department)
                .OrderBy(d => d.FullName)
                .ToList();

            ViewBag.User = user;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoleIds = userRoleIds;
            ViewBag.Doctors = new SelectList(doctors, "DoctorId", "FullName", user.DoctorId);

            return View(user);
        }

        // POST: Admin/AdminUser/AssignRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRole(int id, int[] selectedRoles, int? doctorId)
        {
            var user = context.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Clear existing roles
            user.Roles.Clear();

            // Assign selected roles
            bool isDoctorRole = false;
            if (selectedRoles != null && selectedRoles.Length > 0)
            {
                foreach (var roleId in selectedRoles)
                {
                    var role = context.Roles.Find(roleId);
                    if (role != null)
                    {
                        user.Roles.Add(role);
                        if (role.RoleName == "Doctor")
                        {
                            isDoctorRole = true;
                        }
                    }
                }
            }

            // Eðer Doctor rolü seçildiyse ve DoctorId boþ deðilse ata
            if (isDoctorRole && doctorId.HasValue)
            {
                user.DoctorId = doctorId.Value;
            }
            else if (!isDoctorRole)
            {
                // Doctor rolü yoksa DoctorId'yi null yap
                user.DoctorId = null;
            }
            else if (isDoctorRole && !doctorId.HasValue)
            {
                TempData["Error"] = "Please select which doctor this user is!";
                
                var allRoles = context.Roles.ToList();
                var userRoleIds = selectedRoles.ToList();
                var doctors = context.Doctors
                    .Include(d => d.Department)
                    .OrderBy(d => d.FullName)
                    .ToList();

                ViewBag.User = user;
                ViewBag.AllRoles = allRoles;
                ViewBag.UserRoleIds = userRoleIds;
                ViewBag.Doctors = new SelectList(doctors, "DoctorId", "FullName");
                
                return View(user);
            }

            context.SaveChanges();

            TempData["Success"] = $"Roles for '{user.FirstName} {user.LastName}' have been updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: Admin/AdminUser/QuickAssignRole
        [HttpPost]
        public JsonResult QuickAssignRole(int userId, int roleId)
        {
            try
            {
                var user = context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found!" });
                }

                var role = context.Roles.Find(roleId);
                if (role == null)
                {
                    return Json(new { success = false, message = "Role not found!" });
                }

                // Clear existing roles and assign new one
                user.Roles.Clear();
                user.Roles.Add(role);

                // Eðer Doctor rolü DEÐÝLSE DoctorId'yi temizle
                if (role.RoleName != "Doctor")
                {
                    user.DoctorId = null;
                }

                context.SaveChanges();

                return Json(new { success = true, message = $"Role '{role.RoleName}' assigned successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Admin/AdminUser/Details/5
        public ActionResult Details(int id)
        {
            var user = context.Users
                .Include(u => u.Roles)
                .Include(u => u.Doctor)
                .Include(u => u.Doctor.Department)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Admin/AdminUser/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var user = context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found!" });
                }

                // Don't allow deleting yourself
                var currentUserId = (int)Session["userId"];
                if (user.UserId == currentUserId)
                {
                    return Json(new { success = false, message = "You cannot delete your own account!" });
                }

                // Clear roles first (many-to-many)
                user.Roles.Clear();
                context.Users.Remove(user);
                context.SaveChanges();

                return Json(new { success = true, message = "User deleted successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
