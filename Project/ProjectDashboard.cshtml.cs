using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Project
{
    public class ProjectDashboardModel : PageModel
    {
        public List<Projects> Projects { get; set; }
        public Users usersList { get; set; }
        public Dictionary<string, int> ProjectStatusCounts { get; set; }
        public List<Tasks> UserTasks { get; set; }

        public ProjectDashboardModel()
        {
            Projects = new List<Projects>();
            UserTasks = new List<Tasks>();

        }
        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            Projects = new List<Projects>();
            string userEmail = HttpContext.Session.GetString("Email");
            SqlDataReader projectReader = DBClass.ProjectDashboardReader(userEmail);
            while (projectReader.Read())
            {
                Projects.Add(new Projects
                {
                    Project_ID = Int32.Parse(projectReader["Project_id"].ToString()),
                    Title = projectReader["Title"].ToString(),
                    Status = projectReader["Status"].ToString(),
                    Due_Date = DateTime.Parse(projectReader["Due_Date"].ToString()),
                });
            }
            DBClass.Lab2DBConnection.Close();

            int userID = -1;
            SqlDataReader userReader = DBClass.GetUserDetailsByEmail(userEmail);
            if (userReader.Read())
            {
                userID = Convert.ToInt32(userReader["User_ID"]);
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader taskReader = DBClass.TaskReaderByUser(userID);
            while (taskReader.Read())
            {
                Tasks task = new Tasks
                {
                    Title = taskReader["Title"].ToString(),
                    Description = taskReader["Description"].ToString(),
                    Priority = taskReader["Priority"].ToString(),
                    Status = taskReader["Status"].ToString(),
                    Due_Date = Convert.ToDateTime(taskReader["Due_Date"]),
                    ProjectTitle = taskReader["ProjectTitle"].ToString()
                };

                UserTasks.Add(task);
            }
            taskReader.Close();
            DBClass.Lab2DBConnection.Close();
            BuildChartData();
            
            return Page();
        }
        private void BuildChartData()
        {
            ProjectStatusCounts = Projects
           .GroupBy(p => p.Status)
           .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
