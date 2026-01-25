using Medinova.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminDoctorController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var doctors = context.Doctors.Include(x => x.Department).ToList();
            return View(doctors);
        }

        [HttpGet]
        public ActionResult CreateDoctor()
        {
            var departments = context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult CreateDoctor(Models.Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                context.Doctors.Add(doctor);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            var departments = context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", doctor.DepartmentId);
            return View(doctor);
        }

        public ActionResult DeleteDoctor(int id)
        {
            var value = context.Doctors.Find(id);
            context.Doctors.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateDoctor(int id)
        {
            var doctor = context.Doctors.Find(id);
            var departments = context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", doctor.DepartmentId);
            return View(doctor);
        }

        [HttpPost]
        public ActionResult UpdateDoctor(Models.Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                var value = context.Doctors.Find(doctor.DoctorId);
                value.FullName = doctor.FullName;
                value.ImageUrl = doctor.ImageUrl;
                value.DepartmentId = doctor.DepartmentId;
                value.Description = doctor.Description;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            var departments = context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", doctor.DepartmentId);
            return View(doctor);
        }
    }
}
