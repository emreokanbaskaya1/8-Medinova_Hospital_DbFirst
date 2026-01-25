using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminDepartmentController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var departments = context.Departments.ToList();
            return View(departments);
        }

        [HttpGet]
        public ActionResult CreateDepartment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                context.Departments.Add(department);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }

        public ActionResult DeleteDepartment(int id)
        {
            var value = context.Departments.Find(id);
            context.Departments.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateDepartment(int id)
        {
            var department = context.Departments.Find(id);
            return View(department);
        }

        [HttpPost]
        public ActionResult UpdateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                var value = context.Departments.Find(department.DepartmentId);
                value.Name = department.Name;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }
    }
}
