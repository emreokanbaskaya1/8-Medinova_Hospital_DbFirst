using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminAboutItemController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var aboutItems = context.AboutItems.ToList();
            return View(aboutItems);
        }

        [HttpGet]
        public ActionResult CreateAboutItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAboutItem(AboutItem aboutItem)
        {
            if (ModelState.IsValid)
            {
                context.AboutItems.Add(aboutItem);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aboutItem);
        }

        public ActionResult DeleteAboutItem(int id)
        {
            var value = context.AboutItems.Find(id);
            context.AboutItems.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateAboutItem(int id)
        {
            var aboutItem = context.AboutItems.Find(id);
            return View(aboutItem);
        }

        [HttpPost]
        public ActionResult UpdateAboutItem(AboutItem aboutItem)
        {
            if (ModelState.IsValid)
            {
                var value = context.AboutItems.Find(aboutItem.AboutItemId);
                value.Icon = aboutItem.Icon;
                value.Name = aboutItem.Name;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aboutItem);
        }
    }
}
