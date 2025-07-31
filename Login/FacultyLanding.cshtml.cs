using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Login
{
    public class FacultyLandingModel : PageModel
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

            // Redirect if user is NOT Faculty
            if (userRole != "Faculty")
            {
                HttpContext.Session.SetString("LoginError", "You must be a Faculty member to access this page.");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            // Display Faculty-level access confirmation
            ViewData["LoginMessage"] = $"Login for {userEmail} successful! You have Faculty-level permissions.";
            return Page();
        }
    }
}
