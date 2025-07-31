using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.SponsorOrg
{
    public class AddOrgModel : PageModel
    {
        [BindProperty]
        public SponsorOrgs NewOrg { get; set; }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            DBClass.InsertNewOrg(NewOrg);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("Index");
        }
        public IActionResult OnPostPopulateHandler()
        {
            ModelState.Clear();

			//why do we set them to these values?

			NewOrg.OrgName = "Brand New Org";
            NewOrg.Status_Flag = "In Negotiation";
            NewOrg.Sector = "Unknown"; 

			return Page();
        }
    }
}
