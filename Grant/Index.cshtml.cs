using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab2.Pages.Grant
{
    public class IndexModel : PageModel
    {
        public List<Grants> Grants { get; set; }
        public List<GrantUser> GrantUserAssignments { get; set; } = new List<GrantUser>();
        public List<string> StatusOptions { get; } = new List<string>
        {
            "Opportunity", "Draft", "Applied", "Pending", "Awarded", "Rejected", "Completed"
        };
        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Pursue { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MinAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MaxAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? AwardStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? AwardEndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DeadlineStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DeadlineEndDate { get; set; }
		public List<GrantPermission> GrantPermissions { get; set; } = new();

		public int userId { get; set; } 




		public IndexModel()
        {
            Grants = new List<Grants>();
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            // Check if user has the required role and permission to access certain parts of the page

            //chat helped me figure out how to do this on a grant by grant basis
            
            userId = HttpContext.Session.GetInt32("UserID") ?? 0;

            SqlDataReader reader = DBClass.GetAllGrantPermissionsForUser(userId);

			while (reader.Read())
			{
				GrantPermissions.Add(new GrantPermission
				{
					Grant_ID = Convert.ToInt32(reader["Grant_ID"]),
					Perm_ID = Convert.ToInt32(reader["perm_ID"])
				});
			}

			reader.Close(); 
			DBClass.Lab2DBConnection.Close(); 


			//end of perm check

			GrantUserAssignments.Clear();
            DBClass.Lab2DBConnection.Close();


            int? loggedInUserId = HttpContext.Session.GetInt32("UserID");
            string role = HttpContext.Session.GetString("Role") ?? "";

            SqlDataReader grantReader = DBClass.FilteredGrantsReader(Status, Pursue,MinAmount, MaxAmount,AwardStartDate, AwardEndDate,DeadlineStartDate, DeadlineEndDate, loggedInUserId, role);

            while (grantReader.Read())
            {
                Grants.Add(new Grants
                {
                    Grant_ID = Convert.ToInt32(grantReader["Grant_id"]),
                    Title = grantReader["Title"]?.ToString(),
                    Category = grantReader["Category"]?.ToString(),
                    Type = grantReader["Type"]?.ToString(),
                    Submission_Date = string.IsNullOrEmpty(grantReader["Submission_Date"]?.ToString())
                        ? (DateTime?)null : DateTime.Parse(grantReader["Submission_Date"].ToString()),
                    Award_Date = string.IsNullOrEmpty(grantReader["Award_Date"]?.ToString())
                        ? (DateTime?)null : DateTime.Parse(grantReader["Award_Date"].ToString()),
                    Deadline = string.IsNullOrEmpty(grantReader["Deadline"]?.ToString())
                        ? (DateTime?)null : DateTime.Parse(grantReader["Deadline"].ToString()),
                    AmountRequested = string.IsNullOrEmpty(grantReader["AmountRequested"]?.ToString())
                        ? (double?)null : Convert.ToDouble(grantReader["AmountRequested"]),
                    AmountAwarded = string.IsNullOrEmpty(grantReader["AmountAwarded"]?.ToString())
                        ? (double?)null : Convert.ToDouble(grantReader["AmountAwarded"]),
                    Status = grantReader["Status"]?.ToString(),
                    SponsorOrg_ID = string.IsNullOrEmpty(grantReader["SponsorOrg_ID"]?.ToString())
                        ? (int?)null : Convert.ToInt32(grantReader["SponsorOrg_ID"]),
                    Pursue = grantReader["Pursue"]?.ToString()
                });
            }
            grantReader.Close();
            DBClass.Lab2DBConnection.Close();

            foreach (var grant in Grants)
            {
                SqlDataReader userReader = DBClass.UsersByGrant(grant.Grant_ID);
                if (userReader.Read())
                {
                    int userID = Convert.ToInt32(userReader["User_ID"]);
                    GrantUserAssignments.Add(new GrantUser
                    {
                        GrantUser_ID = grant.Grant_ID,
                        User_ID = userID
                    });
                }
                userReader.Close();
                DBClass.Lab2DBConnection.Close();
            }

            return Page();
        }

        public IActionResult OnPostEdit(int GrantID)//Chat helped used session state to hide GrantID in URL
        {
            HttpContext.Session.SetInt32("GrantID", GrantID);
            return RedirectToPage("EditGrant");
        }

        public IActionResult OnPostSelectGrant(int GrantID)
        {
            // Store the selected GrantID in Session
            HttpContext.Session.SetInt32("GrantID", GrantID);

            // Redirect to Grant Details Page
            return RedirectToPage("/Grant/GrantDetails");
        }

        public IActionResult OnPostUpdateStatus(int GrantID, string Status)
        {
            if (!StatusOptions.Contains(Status))
            {
                return Page();
            }

            //I don't think we need this anymore but didnt want to delete it 
        //    string userRole = HttpContext.Session.GetString("Role");
        //    int? loggedInUserID = HttpContext.Session.GetInt32("UserID");

       //     if (userRole != "Center Director" && userRole != "Admin Staff"|| loggedInUserID == null)
        //    {
        //        TempData["ErrorMessage"] = "You are not authorized to update this grant status.";
        //        return RedirectToPage();
       //     }


            SqlCommand cmd = DBClass.UpdateGrantStatus(GrantID, Status);
            cmd.ExecuteNonQuery();
            DBClass.Lab2DBConnection.Close();

            TempData["SuccessMessage"] = "Grant status updated successfully!";
            return RedirectToPage();
        }

        //Chat helped me figure out how to create this to refer to color the statuses
        public string GetStatusColor(string status)
        {
            return status switch
            {
                "Opportunity" => "blue",
                "Draft" => "gray",
                "Applied" => "lightblue",
                "Pending" => "orange",
                "Awarded" => "green",
                "Rejected" => "red",
                "Completed" => "darkgreen",
                _ => "gray"
            };
        }
    }
}
