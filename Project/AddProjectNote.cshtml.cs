using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.Project
{
    public class AddProjectNoteModel : PageModel
    {
        [BindProperty]
        public ProjectNote ProjectNote { get; set; }
        public List<ProjectNote> ProjectNotesList { get; set; }

        public IActionResult OnGet(int projectid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            ProjectNote = new ProjectNote { Project_ID = projectid };

            ProjectNotesList = new List<ProjectNote>();
            var reader = DBClass.SingleNoteReader(projectid);
            while (reader.Read())
            {
                ProjectNotesList.Add(new ProjectNote
                {
                    Timestamp = DateTime.Parse(reader["Timestamp"].ToString()),
                    Content = reader["Content"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            ProjectNote.Timestamp = DateTime.Now;
            DBClass.InsertProjectNote(ProjectNote);
            DBClass.Lab2DBConnection.Close();
            return RedirectToPage("/Project/ProjectDetails", new { projectid = ProjectNote.Project_ID });
        }
        public IActionResult OnPostPopulateHandler()
        {
            ModelState.Clear();

            ProjectNote.Content = "Newly Inserted Note!";
            ProjectNote.Timestamp = DateTime.Now; //asked chat how to insert a datetime

            return Page();
        }
    }
}
