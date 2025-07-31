using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace Lab2.Pages.Messages
{
    public class IndexModel : PageModel
    {
        //chat helped with implmenting session here and solving errors
        public List<Users> AllUsers { get; set; }
        public int LoggedInUserId { get; set; }
        public string LoggedInUserFirstName { get; set; }
        public string LoggedInUserLastName { get; set; }

        public IndexModel()
        {
            AllUsers = new List<Users>();
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            } 

            // Retrieve user data from Session
            if (HttpContext.Session.GetInt32("UserID") != null)
            {
                LoggedInUserId = (int)HttpContext.Session.GetInt32("UserID");
                LoggedInUserFirstName = HttpContext.Session.GetString("FirstName");
                LoggedInUserLastName = HttpContext.Session.GetString("LastName");
            }
            else
            {
                Response.Redirect("/Login/ParameterizedLogin");
            }

            // Retrieve all users except the logged-in user
            SqlDataReader userReader = DBClass.GetAllUsersExcept(LoggedInUserId);
            while (userReader.Read())
            {
                AllUsers.Add(new Users
                {
                    User_ID = Convert.ToInt32(userReader["User_id"]),
                    FirstName = userReader["FirstName"].ToString(),
                    LastName = userReader["LastName"].ToString(),
                    RoleType = userReader["RoleType"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public void OnPost()
        {

        }
    }
}
