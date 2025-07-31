using System.Data.SqlClient;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.User
{
    public class UserDashboardModel : PageModel
    {
        public List<Users> UsersList { get; set; }

        public UserDashboardModel()
        {
            UsersList = new List<Users>();
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            UsersList = new List<Users>();

            SqlDataReader userReader = DBClass.UsersReader();
            while (userReader.Read())
            {
                UsersList.Add(new Users
                {
                    User_ID = Convert.ToInt32(userReader["User_ID"]),
                    Email = userReader["Email"].ToString(),
                    FirstName = userReader["FirstName"].ToString(),
                    LastName = userReader["LastName"].ToString(),
                    Status = userReader["Status"].ToString(),
                    Organization = userReader["Organization"].ToString(),
                    RoleType = userReader["RoleType"].ToString() //fix to show role name
                });
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }
    }
}
