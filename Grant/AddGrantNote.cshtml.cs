using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Grant
{
    public class AddGrantNoteModel : PageModel
    {
        [BindProperty]
        public GrantNotes GrantNote { get; set; }

        public IActionResult OnGet(int grantid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            GrantNote = new GrantNotes { Grant_ID = grantid };

            return Page();
        }

        public IActionResult OnPost()
        {
            DBClass.InsertGrantNote(GrantNote);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("Index");
        }
    }
}
