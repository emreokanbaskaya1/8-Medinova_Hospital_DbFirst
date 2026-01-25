using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminPackageController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var packages = context.Packages.ToList();
            return View(packages);
        }

        [HttpGet]
        public ActionResult CreatePackage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePackage(Package package)
        {
            if (ModelState.IsValid)
            {
                context.Packages.Add(package);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(package);
        }

        public ActionResult DeletePackage(int id)
        {
            var value = context.Packages.Find(id);
            context.Packages.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdatePackage(int id)
        {
            var package = context.Packages.Find(id);
            return View(package);
        }

        [HttpPost]
        public ActionResult UpdatePackage(Package package)
        {
            if (ModelState.IsValid)
            {
                var value = context.Packages.Find(package.PackageId);
                value.Price = package.Price;
                value.Description = package.Description;
                value.PackageStyle = package.PackageStyle;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(package);
        }
    }
}
