using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Reflection;

namespace Lab2.Pages.Project
{
    public class IndexModel : PageModel
    {
        public List<Projects> Projects { get; set; }
        public List<(int UserId, string FullName)> FacultyList { get; set; }

        [BindProperty(SupportsGet = true)] //Couldn't get the filter to work without this, had to debug with chat
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Faculty { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }
		public int UserID { get; set; }

		public int Perm_ID { get; set; }
		public List<ProjectPermission> ProjectPermissions { get; set; } = new List<ProjectPermission>();

		public IndexModel()
        {
            Projects = new List<Projects>();
            FacultyList = new List<(int, string)>();
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            //chat helped me figure out how to check the permissions per project.
            //Check roles and permissions
            UserID = HttpContext.Session.GetInt32("UserID") ?? 0;
            SqlDataReader reader = DBClass.GetAllProjectPermissionsForUser(UserID);

			while (reader.Read())
			{
				ProjectPermissions.Add(new ProjectPermission
				{
					Project_ID = Convert.ToInt32(reader["Project_ID"]),
					Perm_ID = Convert.ToInt32(reader["perm_ID"])
				});
			}

			reader.Close(); 
			DBClass.Lab2DBConnection.Close(); 


			//end of permissions check



			SqlDataReader facultyReader = DBClass.UsersReader();
            while (facultyReader.Read())
            {
                int id = Convert.ToInt32(facultyReader["User_ID"]);
                string fullName = facultyReader["FirstName"].ToString() + " " + facultyReader["LastName"].ToString();
                FacultyList.Add((id, fullName));
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader filterReader = DBClass.FilteredProjectReader(Status, Faculty, Date);
            while (filterReader.Read())
            {
                Projects.Add(new Projects
                {
                    Project_ID = Int32.Parse(filterReader["Project_ID"].ToString()),
                    Title = filterReader["Title"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();
            return Page();
        }


    }
}

