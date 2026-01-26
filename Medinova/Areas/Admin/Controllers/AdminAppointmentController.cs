using Medinova.Filters;
using Medinova.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminAppointmentController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Admin/AdminAppointment
        public ActionResult Index()
        {
            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentId)
                .ToList();

            return View(appointments);
        }

        // POST: Cancel Appointment
        [HttpPost]
        public JsonResult CancelAppointment(int appointmentId)
        {
            try
            {
                var appointment = context.Appointments.Find(appointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Randevu bulunamadý!" });
                }

                appointment.IsActive = false;
                context.SaveChanges();

                return Json(new { success = true, message = "Randevu baþarýyla iptal edildi!" });
            }
            catch
            {
                return Json(new { success = false, message = "Bir hata oluþtu!" });
            }
        }

        // POST: Activate Appointment
        [HttpPost]
        public JsonResult ActivateAppointment(int appointmentId)
        {
            try
            {
                var appointment = context.Appointments.Find(appointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Randevu bulunamadý!" });
                }

                appointment.IsActive = true;
                context.SaveChanges();

                return Json(new { success = true, message = "Randevu baþarýyla aktifleþtirildi!" });
            }
            catch
            {
                return Json(new { success = false, message = "Bir hata oluþtu!" });
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
