using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lab2.Pages.DataClasses;
using System.Data.SqlClient;
using Lab2.Pages.DB;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Lab2.Pages.Grant
{
    public class SelectFacultyModel : PageModel
    {
        public List<Users> AllUsers { get; set; }

        [BindProperty]
        public int GrantID { get; set; }

        public SelectFacultyModel()
        {
            AllUsers = new List<Users>();
        }
        public IActionResult OnGet(int Grant_id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            GrantID = Grant_id;

            SqlDataReader selectAllUsers = DBClass.AllJMUCAREUsers(); //method change for capstone
            while (selectAllUsers.Read())
            {
                Users facultyMember = new Users
                {
                    User_ID = Convert.ToInt32(selectAllUsers["User_ID"]),
                    LastName = selectAllUsers["LastName"].ToString(),
                    FirstName = selectAllUsers["FirstName"].ToString(),
                    Email = selectAllUsers["Email"].ToString()
                };
                AllUsers.Add(facultyMember);

            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }
        public IActionResult OnPostSelect(int Faculty_id)
        {
            // chat suggested this if becaue of an error I had
            //if (!Request.Query.ContainsKey("Grant"))
            //{
            //    return RedirectToPage("/Grant/SelectFaculty", new { id = GrantID });
            //}

            //model binding allows us to link a control to a property and vice versa, you dont have to do request

            int facultyCount = DBClass.UserExistsForGrant(GrantID, Faculty_id); // this will need to change to user id the db class parameters changed
            DBClass.Lab2DBConnection.Close();
            if (facultyCount > 0)
            {
                TempData["ErrorMessage"] = "User is already assinged to this grant.";
                return RedirectToPage("/Grant/SelectFaculty", new { id = GrantID }); //chat suggest this instead of the return page because we were loosing data
            }
            else
            {
                DBClass.AddUserToGrant(Faculty_id, GrantID);
                DBClass.Lab2DBConnection.Close();
                TempData["SuccessMessage"] = "Faculty member sucessfully added to grant.";
                // Redirect to GrantDetails page
                return RedirectToPage("/Grant/GrantDetails", new { id = GrantID });
            }
        }
    }
}