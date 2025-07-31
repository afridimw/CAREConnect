using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.SponsorOrg
{
    public class EditOrgModel : PageModel
    {
        [BindProperty]
        public SponsorOrgs OrgToUpdate { get; set; }

        public EditOrgModel()
        {
            OrgToUpdate = new SponsorOrgs();
        }

        public IActionResult OnGet(int sponsororgid)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            SqlDataReader singleOrg = DBClass.SingleOrgReader(sponsororgid);

            while (singleOrg.Read())
            {
                OrgToUpdate.SponsorOrg_ID = sponsororgid; 
                OrgToUpdate.OrgName = singleOrg["OrgName"].ToString();
                OrgToUpdate.Sector = singleOrg["Sector"].ToString();
                OrgToUpdate.Status_Flag = singleOrg["Status_Flag"].ToString();
            }

            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            DBClass.UpdateOrg(OrgToUpdate);

            DBClass.Lab2DBConnection.Close();

            return RedirectToPage("Index");
        }
        public IActionResult OnPostPopulateHandler()
        {
            ModelState.Clear();

            OrgToUpdate.OrgName = "Updated Org";
            OrgToUpdate.Status_Flag = "Prospect";
            OrgToUpdate.Sector = "Unknown";  //not sure what to call this
			return Page();
        }
    }
}
