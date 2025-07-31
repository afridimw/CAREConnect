using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Login
{
    public class SecureLoginLandingModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Email") != null)
            {
                string userEmail = HttpContext.Session.GetString("Email");
                string userRole = HttpContext.Session.GetString("Role") ?? "General User";

                if (userRole == "General User")
                {
                    ViewData["PermissionMessage"] = "You have been assigned General User permissions. Some features may be restricted.";
                }

                ViewData["LoginMessage"] = $"Login for {userEmail} successful! Role: {userRole}.";

                return Page();
            }
            else
            {
                HttpContext.Session.SetString("LoginError", "You must login to access that page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }
        }
    }
}
