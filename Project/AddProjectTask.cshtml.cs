using System.Data.Common;
using System.Data.SqlClient;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab2.Pages.Project
{
    public class AddProjectTaskModel : PageModel
    {
        [BindProperty]
        public Tasks NewTask { get; set; }
        public List<SelectListItem> SystemUserList { get; set; }
        public AddProjectTaskModel()
        {
            SystemUserList = new List<SelectListItem>();
        }
        public IActionResult OnGet(int projectid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            NewTask = new Tasks{ Project_ID = projectid};

            SystemUserList = new List<SelectListItem>();
            SqlDataReader getUsers = DBClass.UsersReader();
            while (getUsers.Read())
            {
                SystemUserList.Add(
                    new SelectListItem
                    {
                        Value = getUsers["User_ID"].ToString(),
                        Text = $"{getUsers["FirstName"]} {getUsers["LastName"]}" //help from chat to get first and last name to both show

                    });
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!NewTask.Due_Date.HasValue)
            {
                NewTask.Due_Date = DateTime.Now.AddDays(7);
            }


            DBClass.InsertTask(NewTask);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("/Project/ProjectTasks", new { projectid = NewTask.Project_ID});
        }
    }
}

