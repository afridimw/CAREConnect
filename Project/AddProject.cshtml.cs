using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Project
{
    public class AddProjectModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Projects NewProject { get; set; } = new Projects();

        public List<Users> UserList { get; set; } = new();
        public List<Grants> GrantList { get; set; } = new();


        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            PopulateDropdowns();

            return Page();
        }

        public IActionResult OnPost()
        {
            PopulateDropdowns();
            DBClass.InsertProject(NewProject);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("Index");
        }

        public void PopulateDropdowns()
        {
            SqlDataReader userReader = DBClass.GetProjectLead();
            while (userReader.Read())
            {
                UserList.Add(new Users
                {
                    User_ID = Convert.ToInt32(userReader["User_ID"]),
                    FirstName = userReader["FirstName"].ToString(),
                    LastName = userReader["LastName"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader grantReader = DBClass.AllGrantsReader();
            while (grantReader.Read())
            {
                GrantList.Add(new Grants
                {
                    Grant_ID = Convert.ToInt32(grantReader["Grant_ID"]),
                    Title = grantReader["Title"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();
        }
        public IActionResult OnPostPopulateHandler()
        {
            PopulateDropdowns();

            NewProject.Title = "Remote Learning Accessibility";
            NewProject.StartDate = DateTime.Today;
            NewProject.EndDate = DateTime.Today.AddMonths(6);
            NewProject.Due_Date = DateTime.Today.AddMonths(5);
            NewProject.Budget = 25000;
            NewProject.FundingSource = "DOE";
            NewProject.Description = "Improve access to digital education tools and resources for students with diverse learning needs and limited technology access.";
            NewProject.Department = "Education";
            NewProject.Status = "Active";
            NewProject.LeadUser = UserList.Count > 0 ? UserList[0].User_ID : 0;
            NewProject.Grant_ID = GrantList.Count > 0 ? GrantList[^1].Grant_ID : 0;

            ModelState.Clear();

            return Page();
           
        }

    }
}
