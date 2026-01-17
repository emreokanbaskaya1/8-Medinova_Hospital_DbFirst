using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    public class DefaultController : Controller
    {
        private readonly MedinovaContext _context;

        public DefaultController(MedinovaContext context)
        {
            _context = context;
        }

        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public PartialViewResult DefaultAppointment()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult MakeAppointment(Appointment appointment)
        {
            var departments = _context.Departments.ToList();
            ViewBag.departments = (from department in departments
                                   select new SelectListItem
                                   {
                                       Text = department.Name,
                                       Value = department.DepartmentId.ToString()
                                   }).ToList();

            var doctors = _context.Doctors.ToList();
            ViewBag.doctors = (from doctor in doctors
                               select new SelectListItem
                               {
                                   Text = doctor.FullName,
                                   Value = doctor.DoctorId.ToString()
                               }).ToList();


            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}