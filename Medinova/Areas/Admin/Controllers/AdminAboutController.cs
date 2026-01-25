using Medinova.Models;
using System.Linq;
using System.Web.Mvc;


namespace Medinova.Areas.Admin.Controllers
{
    public class AdminAboutController : Controller
    {
        MedinovaContext context = new MedinovaContext();
        
        public ActionResult Index()
        {
            var abouts = context.Abouts.ToList();
            return View(abouts);
        }

        [HttpGet]
        public ActionResult CreateAbout()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAbout(About about)
        {
            if (ModelState.IsValid)
            {
                context.Abouts.Add(about);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(about);
        }

        public ActionResult DeleteAbout(int id)
        {
            var value = context.Abouts.Find(id);
            context.Abouts.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateAbout(int id)
        {
            var about = context.Abouts.Find(id);
            return View(about);
        }

        [HttpPost]
        public ActionResult UpdateAbout(About about)
        {
            if (ModelState.IsValid)
            {
                var value = context.Abouts.Find(about.AboutId);
                value.Title = about.Title;
                value.Description = about.Description;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(about);
        }
    }
}