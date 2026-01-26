using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var userId = session["userId"];
            var userRole = session["userRole"]?.ToString();

            // Kullanıcı giriş yapmamışsa Login sayfasına yönlendir
            if (userId == null)
            {
                filterContext.Result = new RedirectResult("/Account/Login");
                return;
            }

            // Rol kontrolü
            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
                {
                    filterContext.Result = new RedirectResult("/Account/Unauthorized");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}