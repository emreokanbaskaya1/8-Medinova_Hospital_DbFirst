using Medinova.Filters;
using Medinova.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class DashboardController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            // Statistics
            ViewBag.TotalDoctors = context.Doctors.Count();
            ViewBag.TotalDepartments = context.Departments.Count();
            ViewBag.TotalAppointments = context.Appointments.Count(a => a.IsActive == true);
            ViewBag.TodayAppointments = context.Appointments
                .Count(a => a.IsActive == true && 
                           DbFunctions.TruncateTime(a.AppointmentDate) == DateTime.Today);
            
            // Recent appointments
            var recentAppointments = context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.IsActive == true)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .ToList();

            return View(recentAppointments);
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
