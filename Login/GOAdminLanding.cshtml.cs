using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Login
{
    public class GOAdminLandingModel : PageModel
    {
        public IActionResult OnGet()
        {
            string userEmail = HttpContext.Session.GetString("Email");
            string userRole = HttpContext.Session.GetString("Role");

            // Redirect if user is not logged in
            if (string.IsNullOrEmpty(userEmail))
            {
                HttpContext.Session.SetString("LoginError", "You must login to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            
            if (userRole != "GOAdmin")
            {
                HttpContext.Session.SetString("LoginError", "You must be a Grant Org Admin member to access this page.");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            // Display Faculty-level access confirmation
            ViewData["LoginMessage"] = $"Login for {userEmail} successful! You have Grant Org Admin permissions.";
            return Page();
        }
    }
}
