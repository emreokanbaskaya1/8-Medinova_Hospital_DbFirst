using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    public class DefaultController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        // GET: Default
        public ActionResult Index()
        {
            return View();
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
            return PartialView();
        }

        [HttpPost]
        public ActionResult MakeAppointment(Appointment appointment)
        {

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
                                               Value = doctor.DepartmentId.ToString()
                                           }).ToList();

            return Json(doctors,JsonRequestBehavior.AllowGet);
        }
    }
}