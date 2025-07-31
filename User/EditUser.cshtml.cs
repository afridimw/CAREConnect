using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace Lab2.Pages.User
{
    public class EditUserModel : PageModel
    {
        [BindProperty]
        public Users UserToUpdate { get; set; }
        public List<Users> RoleList { get; set; } = new();
        public List<Users> UserList { get; set; } = new();

        public EditUserModel()
        {
            UserToUpdate = new Users();
        }

        public IActionResult OnGet(string email)
        {
            // Check if user is logged in 
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }


            SqlDataReader singleUser = DBClass.SingleUserReader(email);

            while (singleUser.Read())
            {
                UserToUpdate.Email = email;
                UserToUpdate.FirstName = singleUser["FirstName"].ToString();
                UserToUpdate.LastName = singleUser["LastName"].ToString();
                UserToUpdate.Status = singleUser["Status"].ToString();
                UserToUpdate.Organization = singleUser["Organization"].ToString();
                UserToUpdate.Role_ID = Int32.Parse(singleUser["Role_ID"].ToString());
                UserToUpdate.RoleType = singleUser["RoleType"].ToString();
            }

            DBClass.Lab2DBConnection.Close();
            return Page();
        }

        public IActionResult OnPost()
        {
            DBClass.UpdateUser(UserToUpdate);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("UserDashboard");
        }

      
    }
}
