using Medinova.Models;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AdminServiceController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var services = context.Services.ToList();
            return View(services);
        }

        [HttpGet]
        public ActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateService(Service service)
        {
            try
            {
                // ServiceId'yi 0 yap (otomatik artan için)
                service.ServiceId = 0;
                
                // NULL kontrolü
                if (string.IsNullOrWhiteSpace(service.Title))
                {
                    ModelState.AddModelError("Title", "Baþlýk boþ olamaz");
                }
                
                if (string.IsNullOrWhiteSpace(service.Description))
                {
                    ModelState.AddModelError("Description", "Açýklama boþ olamaz");
                }

                if (ModelState.IsValid)
                {
                    context.Services.Add(service);
                    context.SaveChanges();
                    TempData["SuccessMessage"] = "Hizmet baþarýyla eklendi!";
                    return RedirectToAction("Index");
                }
                return View(service);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.AppendLine("=== VALIDATION ERRORS ===");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    errorMessages.AppendLine($"Entity: {validationErrors.Entry.Entity.GetType().Name}");
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.AppendLine($"  - Property: {validationError.PropertyName}");
                        errorMessages.AppendLine($"    Error: {validationError.ErrorMessage}");
                    }
                }
                ViewBag.ErrorMessage = errorMessages.ToString();
                return View(service);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"GENEL HATA:\n{ex.Message}\n\nInner Exception:\n{ex.InnerException?.Message}";
                return View(service);
            }
        }

        public ActionResult DeleteService(int id)
        {
            try
            {
                var value = context.Services.Find(id);
                if (value != null)
                {
                    context.Services.Remove(value);
                    context.SaveChanges();
                    TempData["SuccessMessage"] = "Hizmet baþarýyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hizmet bulunamadý!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Silme hatasý: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateService(int id)
        {
            var service = context.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost]
        public ActionResult UpdateService(Service service)
        {
            try
            {
                // NULL kontrolü
                if (string.IsNullOrWhiteSpace(service.Title))
                {
                    ModelState.AddModelError("Title", "Baþlýk boþ olamaz");
                }
                
                if (string.IsNullOrWhiteSpace(service.Description))
                {
                    ModelState.AddModelError("Description", "Açýklama boþ olamaz");
                }

                if (ModelState.IsValid)
                {
                    var value = context.Services.Find(service.ServiceId);
                    if (value != null)
                    {
                        value.Title = service.Title;
                        value.Description = service.Description;
                        context.SaveChanges();
                        TempData["SuccessMessage"] = "Hizmet baþarýyla güncellendi!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hizmet bulunamadý!");
                    }
                }
                return View(service);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.AppendLine("=== VALIDATION ERRORS ===");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    errorMessages.AppendLine($"Entity: {validationErrors.Entry.Entity.GetType().Name}");
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.AppendLine($"  - Property: {validationError.PropertyName}");
                        errorMessages.AppendLine($"    Error: {validationError.ErrorMessage}");
                    }
                }
                ViewBag.ErrorMessage = errorMessages.ToString();
                return View(service);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"GENEL HATA:\n{ex.Message}\n\nInner Exception:\n{ex.InnerException?.Message}";
                return View(service);
            }
        }
    }
}
