using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Lab2.Pages.Project
{
    public class EditProjectModel : PageModel
    {
        [BindProperty]
        public Projects ProjectToUpdate { get; set; }

        public List<Users> UserList { get; set; } = new();
        public List<Grants> GrantList { get; set; } = new();
		public int PermID { get; set; }
		public int ProjectID { get; set; }
		public int UserID { get; set; }

		public EditProjectModel()
        {
            ProjectToUpdate = new Projects();
        }

        public IActionResult OnGet(int projectid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

			UserID = HttpContext.Session.GetInt32("UserID") ?? 0;
			ProjectID = projectid;

			SqlDataReader getperm = DBClass.GetPermissionForProjCommand(UserID, ProjectID);

			if (getperm.Read())
			{
				PermID = Convert.ToInt32(getperm["perm_ID"]);
			}

			getperm.Close(); 
			DBClass.Lab2DBConnection.Close(); 

			PopulateDropdowns();

            SqlDataReader singleProject = DBClass.SingleProjectReader(projectid);

            while (singleProject.Read())
            {
                ProjectToUpdate.Project_ID = projectid;
                ProjectToUpdate.Title = singleProject["Title"].ToString();
                ProjectToUpdate.StartDate = singleProject["StartDate"] != DBNull.Value ? Convert.ToDateTime(singleProject["StartDate"]) : (DateTime?)null;
                ProjectToUpdate.EndDate = singleProject["EndDate"] != DBNull.Value ? Convert.ToDateTime(singleProject["EndDate"]) : (DateTime?)null;
                ProjectToUpdate.Due_Date = singleProject["Due_Date"] != DBNull.Value ? Convert.ToDateTime(singleProject["Due_Date"]) : (DateTime?)null;
                ProjectToUpdate.Budget = singleProject["Budget"] != DBNull.Value ? Convert.ToDecimal(singleProject["Budget"]) : 0;
                ProjectToUpdate.FundingSource = singleProject["FundingSource"].ToString();
                ProjectToUpdate.LeadUser = Convert.ToInt32(singleProject["LeadUser"]);
                ProjectToUpdate.Description = singleProject["Description"].ToString();
                ProjectToUpdate.Department = singleProject["Department"].ToString();
                ProjectToUpdate.Status = singleProject["Status"].ToString();
                ProjectToUpdate.Grant_ID = singleProject["Grant_ID"] != DBNull.Value ? Convert.ToInt32(singleProject["Grant_ID"]) : 0;
            }

            DBClass.Lab2DBConnection.Close();
            return Page();
        }

        public IActionResult OnPost()
        {
            PopulateDropdowns();
            DBClass.UpdateProject(ProjectToUpdate);
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
            ModelState.Clear();

            ProjectToUpdate.Title = "New Research Program";
            ProjectToUpdate.Due_Date = DateTime.Now; //asked chat how to insert a datetime
            ProjectToUpdate.Status = "Completed";

            return Page();
        }
    }
}
