using Medinova.DTOs;
using Medinova.Enums;
using Medinova.Filters;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Patient.Controllers
{
    [AuthorizeRole("Patient")]
    public class PatientController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Patient/Patient
        public ActionResult Index()
        {
            int userId = (int)Session["userId"];
            string userFullName = Session["fullName"]?.ToString();

            // Find appointments by FullName (temporary solution - until PatientId field is added)
            var activeAppointments = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .Where(a => a.FullName == userFullName && 
                           a.IsActive == true &&
                           a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            ViewBag.ActiveCount = activeAppointments.Count;
            ViewBag.PastCount = context.Appointments
                .Count(a => a.FullName == userFullName && 
                           a.IsActive == true &&
                           a.AppointmentDate < DateTime.Today);
            ViewBag.CancelledCount = context.Appointments
                .Count(a => a.FullName == userFullName && a.IsActive == false);

            return View(activeAppointments);
        }

        // GET: Patient/Patient/ActiveAppointments
        public ActionResult ActiveAppointments()
        {
            string userFullName = Session["fullName"]?.ToString();

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .Where(a => a.FullName == userFullName && 
                           a.IsActive == true &&
                           a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // GET: Patient/Patient/PastAppointments
        public ActionResult PastAppointments()
        {
            string userFullName = Session["fullName"]?.ToString();

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .Where(a => a.FullName == userFullName && 
                           a.IsActive == true &&
                           a.AppointmentDate < DateTime.Today)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // GET: Patient/Patient/CancelledAppointments
        public ActionResult CancelledAppointments()
        {
            string userFullName = Session["fullName"]?.ToString();

            var appointments = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .Where(a => a.FullName == userFullName && a.IsActive == false)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // GET: Patient/Patient/CreateAppointment
        public ActionResult CreateAppointment()
        {
            // Department list
            var departments = context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name");

            // Date list (7 days)
            var dateList = new List<SelectListItem>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.AddDays(i);
                dateList.Add(new SelectListItem
                {
                    Text = date.ToString("dd MMMM yyyy - dddd"),
                    Value = date.ToString("yyyy-MM-dd")
                });
            }
            ViewBag.DateList = dateList;

            return View();
        }

        // POST: Patient/Patient/CreateAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAppointment(int departmentId, int doctorId, string appointmentDate, string appointmentTime)
        {
            try
            {
                int userId = (int)Session["userId"];
                string userFullName = Session["fullName"]?.ToString();

                var date = DateTime.Parse(appointmentDate);

                // Check if appointment exists for the same doctor, date and time
                var existingAppointment = context.Appointments
                    .FirstOrDefault(a => a.DoctorId == doctorId &&
                                        DbFunctions.TruncateTime(a.AppointmentDate) == date &&
                                        a.AppointmentTime == appointmentTime &&
                                        a.IsActive == true);

                if (existingAppointment != null)
                {
                    TempData["Error"] = "This time slot is already booked!";
                    return RedirectToAction("CreateAppointment");
                }

                // Create new appointment
                var appointment = new Appointment
                {
                    DoctorId = doctorId,
                    FullName = userFullName,
                    AppointmentDate = date,
                    AppointmentTime = appointmentTime,
                    IsActive = true
                };

                context.Appointments.Add(appointment);
                context.SaveChanges();

                TempData["Success"] = "Your appointment has been created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating appointment: " + ex.Message;
                return RedirectToAction("CreateAppointment");
            }
        }

        // POST: Patient/Patient/CancelAppointment
        [HttpPost]
        public JsonResult CancelAppointment(int appointmentId)
        {
            try
            {
                string userFullName = Session["fullName"]?.ToString();

                var appointment = context.Appointments.Find(appointmentId);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found!" });
                }

                if (appointment.FullName != userFullName)
                {
                    return Json(new { success = false, message = "You are not authorized to cancel this appointment!" });
                }

                // Set appointment as inactive (cancel)
                appointment.IsActive = false;
                context.SaveChanges();

                return Json(new { success = true, message = "Your appointment has been cancelled successfully. This time slot is now available again." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Get doctors by department (AJAX)
        public JsonResult GetDoctorsByDepartment(int departmentId)
        {
            var doctors = context.Doctors
                .Where(d => d.DepartmentId == departmentId)
                .Select(d => new { Value = d.DoctorId, Text = d.FullName })
                .ToList();

            return Json(doctors, JsonRequestBehavior.AllowGet);
        }

        // GET: Get available hours (AJAX)
        [HttpPost]
        public JsonResult GetAvailableHours(int doctorId, string selectedDate)
        {
            var date = DateTime.Parse(selectedDate);

            // Find booked times for this doctor on the selected date
            var bookedTimes = context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           DbFunctions.TruncateTime(a.AppointmentDate) == date &&
                           a.IsActive == true)
                .Select(a => a.AppointmentTime)
                .ToList();

            var availableHours = new List<AppointmentAvailabilityDto>();
            
            foreach (var hour in Times.AppointmentHours)
            {
                availableHours.Add(new AppointmentAvailabilityDto
                {
                    Time = hour,
                    IsBooked = bookedTimes.Contains(hour)
                });
            }

            return Json(availableHours, JsonRequestBehavior.AllowGet);
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
