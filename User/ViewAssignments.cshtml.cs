using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Lab2.Pages.User
{
    public class ViewAssignmentsModel : PageModel
    {
        public int User_ID { get; set; }
        public List<ProjectUser> UserProjects { get; set; } = new();
        public List<GrantUser> UserGrants { get; set; } = new();   
        public int SelectedPermID { get; set; }
        public string SelectedType { get; set; }
        public int SelectedID { get; set; }
        public string UserFullName { get; set; }

        public IActionResult OnGet(int userId)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            User_ID = userId;

            // Get Projects
            SqlDataReader projectReader = DBClass.GetUserProjects(userId);
            while (projectReader.Read())
            {
                UserProjects.Add(new ProjectUser
                {
                    Project_ID = Convert.ToInt32(projectReader["Project_ID"]),
                    Title = projectReader["Title"].ToString(),
                    Permission = projectReader["permission"].ToString(),
                    permId = Convert.ToInt32(projectReader["perm_ID"])
                });
            }
            projectReader.Close();
            DBClass.Lab2DBConnection.Close();

            // Get Grants
            SqlDataReader grantReader = DBClass.GetUserGrants(userId);
            while (grantReader.Read())
            {
                UserGrants.Add(new GrantUser
                {
                    Grant_ID = Convert.ToInt32(grantReader["Grant_ID"]),
                    Title = grantReader["Title"].ToString(),
                    Permission = grantReader["permission"].ToString(),
                    permId = Convert.ToInt32(grantReader["perm_ID"])
                });
            }
            grantReader.Close();

            DBClass.Lab2DBConnection.Close();

            SqlDataReader reader = DBClass.GetUserNameById(userId);

            if (reader.Read())
            {
                UserFullName = reader[0].ToString(); // full name already concatenated
            }

            reader.Close();
            DBClass.Lab2DBConnection.Close();

            return Page();

        }



        public IActionResult OnPostProject(int permId, int userId, int projectId)
        {
           
            DBClass db = new DBClass();
            db.UpdateProjectPermission(permId, userId, projectId);
            DBClass.Lab2DBConnection.Close();

            return RedirectToPage(new { userId });
        }

        public IActionResult OnPostGrant(int permId, int userId, int grantId)
        {

            DBClass db = new DBClass();
            db.UpdateGrantPermission(permId, userId, grantId);
            DBClass.Lab2DBConnection.Close();

            
            
            return RedirectToPage(new { userId });
        }



    }

}
