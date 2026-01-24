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

            appointment.IsActive = true;
            context.Appointments.Add(appointment);
            context.SaveChanges();
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
            var bookedTimes = context.Appointments.Where(x => x.DoctorId == doctorId && x.AppointmentDate == selectedDate).Select(x=>x.AppointmentTime).ToList();

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






    }
}