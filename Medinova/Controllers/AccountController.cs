using Medinova.DTOs;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill in all fields.";
                return View(model);
            }

            try
            {
                var user = context.Users.FirstOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Username or password is incorrect!";
                    return View(model);
                }

                FormsAuthentication.SetAuthCookie(user.UserName, false);
                Session["userName"] = user.UserName;
                Session["fullName"] = user.FirstName + " " + user.LastName;
                Session["userId"] = user.UserId;
                
                return RedirectToAction("Index", "AdminAbout", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login: " + ex.Message;
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
        public ActionResult Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill in all fields correctly.";
                return View(model);
            }

            if (!model.AcceptTerms)
            {
                ViewBag.ErrorMessage = "You must accept the terms and conditions.";
                return View(model);
            }

            try
            {
                // Check username exists
                var existingUser = context.Users.FirstOrDefault(x => x.UserName == model.UserName);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "This username is already taken!";
                    return View(model);
                }

                // Create new user
                var newUser = new User
                {
                    UserName = model.UserName,
                    Password = model.Password, // NOTE: In production, password should be hashed!
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                ViewBag.SuccessMessage = "Your account has been successfully created! You can now login.";
                
                // Redirect to success page
                return View("RegisterSuccess");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during registration: " + ex.Message;
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}