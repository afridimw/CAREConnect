using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Login
{
    public class CreateHashedLoginModel : PageModel
    {
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string FirstName { get; set; }
        [BindProperty] public string LastName { get; set; }
        [BindProperty] public string Status { get; set; }
        [BindProperty] public string Organization { get; set; }
        [BindProperty] public int RoleID { get; set; }

        public IActionResult OnPostPopulateHandler()
        {
            Email = "fmiller@jmu.edu";
            Password = "password";
            FirstName = "Frank";
            LastName = "Miller";
            Status = "Active";
            Organization = "JMU Staff";
            RoleID = 5;

            ViewData["UserCreate"] = "Form populated with test data.";
            ModelState.Clear();

            return Page();
        }

        public IActionResult OnPost()
        {

            Users newUser = new Users
            {
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Status = Status,
                Organization = Organization,
                Role_ID = RoleID
            };

            DBClass.CreateHashedUser(newUser, Password);

            DBClass.AuthDBConnection.Close();
            DBClass.Lab2DBConnection.Close();

            ViewData["UserCreate"] = "User successfully created!";
            return RedirectToPage("/User/UserDashboard");

        }
        


    }
}