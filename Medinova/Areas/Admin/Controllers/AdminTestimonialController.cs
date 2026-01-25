using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminTestimonialController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var testimonials = context.Testimonials.ToList();
            return View(testimonials);
        }

        [HttpGet]
        public ActionResult CreateTestimonial()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateTestimonial(Testimonial testimonial)
        {
            if (ModelState.IsValid)
            {
                context.Testimonials.Add(testimonial);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(testimonial);
        }

        public ActionResult DeleteTestimonial(int id)
        {
            var value = context.Testimonials.Find(id);
            context.Testimonials.Remove(value);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateTestimonial(int id)
        {
            var testimonial = context.Testimonials.Find(id);
            return View(testimonial);
        }

        [HttpPost]
        public ActionResult UpdateTestimonial(Testimonial testimonial)
        {
            if (ModelState.IsValid)
            {
                var value = context.Testimonials.Find(testimonial.TestimonialId);
                value.Name = testimonial.Name;
                value.ImageUrl = testimonial.ImageUrl;
                value.Description = testimonial.Description;
                value.Profession = testimonial.Profession;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(testimonial);
        }
    }
}
