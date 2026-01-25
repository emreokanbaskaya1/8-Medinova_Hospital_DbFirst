using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminBannerController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var banners = context.Banners.ToList();
            return View(banners);
        }

        [HttpGet]
        public ActionResult CreateBanner()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateBanner(Banner banner)
        {
            if (ModelState.IsValid)
            {
                context.Banners.Add(banner);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(banner);
        }

        public ActionResult DeleteBanner(int id)
        {
            var value = context.Banners.Find(id);
            context.Banners.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateBanner(int id)
        {
            var banner = context.Banners.Find(id);
            return View(banner);
        }

        [HttpPost]
        public ActionResult UpdateBanner(Banner banner)
        {
            if (ModelState.IsValid)
            {
                var value = context.Banners.Find(banner.BannerId);
                value.Title = banner.Title;
                value.Description = banner.Description;
                value.ImageUrl = banner.ImageUrl;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(banner);
        }
    }
}
