using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Lab2.Pages.Grant
{
    public class InsertGrantModel : PageModel
    {
        [BindProperty]
        public Grants NewGrant { get; set; } = new Grants();

        public List<SponsorOrgs> SponsorOrgList { get; set; } = new();

		//[BindProperty]
		//[Required]
		//public String Title { get; set; }

		//[BindProperty]
		//[Required]
		//public String Category { get; set; }

		//[BindProperty]
		//[Required]
		//public String Type { get; set; }

		//[BindProperty]
		//[Required]
		//public Date Submission_Date { get; set; }

		//[BindProperty]
		//[Required]
		//public Date Award_Date { get; set; }

		//[BindProperty]
		//[Required]
		//public Date Deadline { get; set; }

		//[BindProperty]
		//[Required]
		//public Double AmountRequested { get; set; }

		//[BindProperty]
		//[Required]
		//public String AmountAwarded { get; set; }

		//[BindProperty]
		//[Required]
		//public String Status { get; set; }

		//[BindProperty]
		//[Required]
		//public int SponsorOrg_ID { get; set; }

		//[BindProperty]
		//[Required]
		//public String Pursue { get; set; }

		public void PopulateSponsorOrgs()
		{
			var sponsorReader = DBClass.GetSponsorOrgs(); // You'll need to add this method if it doesn't exist
			while (sponsorReader.Read())
			{
				SponsorOrgList.Add(new SponsorOrgs
				{
					SponsorOrg_ID = Convert.ToInt32(sponsorReader["SponsorOrg_ID"]),
					OrgName = sponsorReader["OrgName"].ToString()
				});
			}
			DBClass.Lab2DBConnection.Close();
		}

		public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }
			PopulateSponsorOrgs();
			return Page();
        }

        public IActionResult OnPost()
        {
            //Console.WriteLine("Submit button clicked, this is a debug check");
            //I took some edit suggestions from chat here, this if he wrote
            if (!ModelState.IsValid)
            {
                Console.WriteLine("model state not valid, this is a debug check");
                return Page(); // Prevent submission if validation fails
            }

            DBClass.InsertGrant(NewGrant);
            DBClass.Lab2DBConnection.Close();
			PopulateSponsorOrgs();

			TempData["SuccessMessage"] = "Grant added successfully!";
            //Console.WriteLine("Grant added successfully, redirecting...");
            return RedirectToPage("/Grant/Index");
        }

		public IActionResult OnPostPopulateHandler()
		{
			ModelState.Clear();

			NewGrant.Title = "Remote Learning Accessibility";
			NewGrant.Category = "Educational Technology";
			NewGrant.Type = "University";
			NewGrant.Submission_Date = new DateTime(2025, 4, 12);
			NewGrant.Award_Date = new DateTime(2025, 4, 14);
			NewGrant.Deadline = new DateTime(2025, 4, 17);
			NewGrant.AmountRequested = 200000;
			NewGrant.AmountAwarded = null;
			NewGrant.Status = "N/A";
			NewGrant.SponsorOrg_ID = 3;
			NewGrant.Pursue = "NO";

			PopulateSponsorOrgs(); // Important so dropdown renders right
			return Page();
		}
	}
}
