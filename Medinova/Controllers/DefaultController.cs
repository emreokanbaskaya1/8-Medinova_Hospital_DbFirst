using Medinova.DTOs;
using Medinova.Enums;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class DefaultController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public PartialViewResult DefaultTestimonial()
        {
            var testimonials = context.Testimonials.ToList();
            return PartialView(testimonials);
        }

        [HttpGet]
        public PartialViewResult DefaultBanner()
        {
            var banner = context.Banners.FirstOrDefault();
            return PartialView(banner);
        }

        [HttpGet]
        public PartialViewResult DefaultDoctor()
        {
            var doctors = context.Doctors.Include(x => x.Department).ToList();
            return PartialView(doctors);
        }

        [HttpGet]
        public PartialViewResult DefaultPackage()
        {
            var packages = context.Packages.ToList();
            return PartialView(packages);
        }

        [HttpGet]
        public PartialViewResult DefaultService()
        {
            var services = context.Services.ToList();
            return PartialView(services);
        }

        [HttpGet]
        public PartialViewResult DefaultAbout()
        {
            var about = context.Abouts.FirstOrDefault();
            return PartialView(about);
        }

        [HttpGet]
        public PartialViewResult DefaultAboutItem()
        {
            var aboutitems = context.AboutItems.ToList();
            return PartialView(aboutitems);
        }

        [HttpGet]
        public PartialViewResult DefaultAppointment()
        {
            var departments = context.Departments.ToList();
            ViewBag.departments = (from department in departments
                                   select new SelectListItem
                                   {
                                       Text = department.Name,
                                       Value = department.DepartmentId.ToString()
                                   }).ToList();


            var dateList = new List<SelectListItem>();
            for(int i=0; i<7; i++)
            {
                var date = DateTime.Now.AddDays(i);

                dateList.Add(new SelectListItem
                {
                    Text = date.ToString("dd.MMMM.dddd"),
                    Value = date.ToString("yyyy-MM-dd")
                });
            }
            ViewBag.dateList = dateList;

            return PartialView();
        }

        [HttpPost]
        public ActionResult MakeAppointment(Appointment appointment)
        {
            // Kullanıcı giriş yapmış mı kontrol et
            if (Session["userId"] == null)
            {
                TempData["Error"] = "Randevu oluşturmak için giriş yapmalısınız!";
                return RedirectToAction("Login", "Account");
            }

            // FullName'i session'dan al (eğer boşsa)
            if (string.IsNullOrEmpty(appointment.FullName))
            {
                appointment.FullName = Session["fullName"]?.ToString();
            }
            
            appointment.IsActive = true;
            
            // Aynı doktor, tarih ve saatte randevu kontrolü
            var existingAppointment = context.Appointments
                .FirstOrDefault(a => a.DoctorId == appointment.DoctorId &&
                                    DbFunctions.TruncateTime(a.AppointmentDate) == DbFunctions.TruncateTime(appointment.AppointmentDate) &&
                                    a.AppointmentTime == appointment.AppointmentTime &&
                                    a.IsActive == true);

            if (existingAppointment != null)
            {
                TempData["Error"] = "Bu tarih ve saat için randevu dolu! Lütfen başka bir saat seçin.";
                return RedirectToAction("Index");
            }

            context.Appointments.Add(appointment);
            context.SaveChanges();
            
            TempData["Success"] = "Randevunuz başarıyla oluşturuldu!";
            return RedirectToAction("Index");
        }

        public JsonResult GetDoctorByDepartmentId(int departmentId)
        {
            var doctors = context.Doctors.Where(x=>x.DepartmentId == departmentId)
                                           .Select(doctor => new SelectListItem
                                           {
                                               Text = doctor.FullName,
                                               Value = doctor.DoctorId.ToString()
                                           }).ToList();

            return Json(doctors,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetAvailableHours(DateTime selectedDate, int doctorId)
        {
            // Sadece aktif randevuları kontrol et (iptal edilenler hariç)
            var bookedTimes = context.Appointments
                .Where(x => x.DoctorId == doctorId && 
                           DbFunctions.TruncateTime(x.AppointmentDate) == selectedDate.Date &&
                           x.IsActive == true)
                .Select(x => x.AppointmentTime)
                .ToList();

            var dtoList = new List<AppointmentAvailabilityDto>();
            foreach(var hour in Times.AppointmentHours)
            {
                var dto = new AppointmentAvailabilityDto();
                dto.Time = hour;

                if (bookedTimes.Contains(hour))
                {
                    dto.IsBooked = true;
                }
                else
                {
                    dto.IsBooked = false;
                }

                dtoList.Add(dto);
            }

            return Json(dtoList, JsonRequestBehavior.AllowGet);
        }

        // Kullanıcı giriş durumunu kontrol et (AJAX için)
        [HttpGet]
        public JsonResult CheckLoginStatus()
        {
            bool isLoggedIn = Session["userId"] != null;
            return Json(new { isLoggedIn = isLoggedIn }, JsonRequestBehavior.AllowGet);
        }
    }
}