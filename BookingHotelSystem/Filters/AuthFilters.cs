using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingHotelSystem.Filters
{
    // [LoginRequired] = mesti login dulu baru boleh masuk
    public class LoginRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("UserId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
        }
    }

    // [StaffOnly] = Staf atau Admin sahaja
    public class StaffOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("Role");

            if (session.GetInt32("UserId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
            else if (role != "Staff" && role != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }

    // [AdminOnly] = Admin sahaja
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (session.GetInt32("UserId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
            else if (session.GetString("Role") != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
