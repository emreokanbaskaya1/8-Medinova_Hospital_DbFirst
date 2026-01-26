using Medinova.DTOs;
using Medinova.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        MedinovaContext context = new MedinovaContext();

        // GET: Login
        public ActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yaptıysa rolüne göre yönlendir
            if (Session["userId"] != null)
            {
                return RedirectToUserDashboard();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Lütfen tüm alanları doldurun.";
                return View(model);
            }

            try
            {
                // Kullanıcıyı bul
                var user = context.Users
                    .FirstOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
                    return View(model);
                }

                // Kullanıcının rolünü al (UserRoles tablosundan)
                var userRole = context.Roles
                    .Where(r => r.Users.Any(u => u.UserId == user.UserId))
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                // Session bilgilerini kaydet
                FormsAuthentication.SetAuthCookie(user.UserName, false);
                Session["userId"] = user.UserId;
                Session["userName"] = user.UserName;
                Session["fullName"] = $"{user.FirstName} {user.LastName}";
                Session["userRole"] = userRole ?? "Patient"; // Varsayılan rol Patient

                // Rol bazlı yönlendirme
                return RedirectToUserDashboard();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Giriş sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Lütfen tüm alanları doğru şekilde doldurun.";
                return View(model);
            }

            try
            {
                // Kullanıcı adı kontrolü
                var existingUser = context.Users.FirstOrDefault(x => x.UserName == model.UserName);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor!";
                    return View(model);
                }

                // Yeni kullanıcı oluştur
                var newUser = new User
                {
                    UserName = model.UserName,
                    Password = model.Password, // NOT: Production'da şifreyi hash'leyin!
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                // Patient rolünü al ve kullanıcıya ata
                var patientRole = context.Roles.FirstOrDefault(r => r.RoleName == "Patient");
                if (patientRole != null)
                {
                    // Many-to-Many ilişkisini kur
                    newUser.Roles.Add(patientRole);
                    context.SaveChanges();
                }

                ViewBag.SuccessMessage = "Hesabınız başarıyla oluşturuldu! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kayıt sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Default");
        }

        public ActionResult Unauthorized()
        {
            ViewBag.ErrorMessage = "Bu sayfaya erişim yetkiniz yok!";
            return View();
        }

        // Helper method: Kullanıcıyı rolüne göre yönlendir
        private ActionResult RedirectToUserDashboard()
        {
            var userRole = Session["userRole"]?.ToString();

            switch (userRole)
            {
                case "Admin":
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                case "Doctor":
                    return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
                case "Patient":
                default:
                    return RedirectToAction("Index", "Patient", new { area = "Patient" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}