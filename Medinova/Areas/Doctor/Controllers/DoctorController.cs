using Medinova.Filters;
using Medinova.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [AuthorizeRole("Doctor")]
    public class DoctorController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Doctor/Doctor
        public ActionResult Index()
        {
            int userId = (int)Session["userId"];
            
            // Kullanýcýnýn DoctorId'sini al
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            
            if (user == null || !user.DoctorId.HasValue)
            {
                ViewBag.ErrorMessage = "Doctor information not found! Please contact administrator to assign your doctor profile.";
                ViewBag.DoctorName = Session["fullName"]?.ToString();
                ViewBag.TodayCount = 0;
                ViewBag.TotalAppointments = 0;
                return View(new System.Collections.Generic.List<Appointment>());
            }

            var doctorId = user.DoctorId.Value;
            var doctor = context.Doctors
                .Include(d => d.Department)
                .FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                ViewBag.ErrorMessage = "Doctor profile not found!";
                ViewBag.DoctorName = Session["fullName"]?.ToString();
                ViewBag.TodayCount = 0;
                ViewBag.TotalAppointments = 0;
                return View(new System.Collections.Generic.List<Appointment>());
            }

            // Today's appointments
            var today = DateTime.Today;
            var todayAppointments = context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           DbFunctions.TruncateTime(a.AppointmentDate) == today &&
                           a.IsActive == true)
                .OrderBy(a => a.AppointmentTime)
                .ToList();

            ViewBag.DoctorId = doctor.DoctorId;
            ViewBag.DoctorName = doctor.FullName;
            ViewBag.Department = doctor.Department?.Name;
            ViewBag.TodayCount = todayAppointments.Count;
            ViewBag.TotalAppointments = context.Appointments
                .Count(a => a.DoctorId == doctorId && a.IsActive == true);

            return View(todayAppointments);
        }

        // GET: Doctor/Doctor/AllAppointments
        public ActionResult AllAppointments()
        {
            int? doctorId = GetDoctorId();
            
            if (doctorId == null)
            {
                TempData["Error"] = "Doctor information not found!";
                return RedirectToAction("Index");
            }

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // GET: Doctor/Doctor/ActiveAppointments
        public ActionResult ActiveAppointments()
        {
            int? doctorId = GetDoctorId();
            
            if (doctorId == null)
            {
                TempData["Error"] = "Doctor information not found!";
                return RedirectToAction("Index");
            }

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId && a.IsActive == true)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // POST: Doctor/Doctor/CancelAppointment
        [HttpPost]
        public JsonResult CancelAppointment(int appointmentId)
        {
            try
            {
                int? doctorId = GetDoctorId();

                if (doctorId == null)
                {
                    return Json(new { success = false, message = "Doctor information not found!" });
                }

                var appointment = context.Appointments.Find(appointmentId);
                
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found!" });
                }

                if (appointment.DoctorId != doctorId)
                {
                    return Json(new { success = false, message = "You are not authorized to cancel this appointment!" });
                }

                appointment.IsActive = false;
                context.SaveChanges();

                return Json(new { success = true, message = "Appointment cancelled successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Doctor/Doctor/AppointmentDetails/5
        public ActionResult AppointmentDetails(int id)
        {
            int? doctorId = GetDoctorId();

            if (doctorId == null)
            {
                TempData["Error"] = "Doctor information not found!";
                return RedirectToAction("Index");
            }

            var appointment = context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == id && a.DoctorId == doctorId);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            return View(appointment);
        }

        // Helper method: Get Doctor ID from User
        private int? GetDoctorId()
        {
            if (Session["userId"] == null)
                return null;

            int userId = (int)Session["userId"];
            
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            
            return user?.DoctorId;
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
