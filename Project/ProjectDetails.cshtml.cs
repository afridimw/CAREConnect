using System.Data.SqlClient;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab2.Pages.Project
{
    public class ProjectDetailsModel : PageModel
    {
        [BindProperty]
        public Projects ProjectDetail { get; set; }

        [BindProperty]
        public ProjectNote ProjectNotes { get; set; }

        public List<ProjectNote> ProjectNotesList { get; set; }

        [BindProperty]
        public int ProjectID { get; set; }

        public int ProjectProgress { get; set; }
        public List<string> StatusOptions { get; } = new List<string>
        {
            "Active", "Archived"
        };

        //for file upload
        private readonly IWebHostEnvironment _env;
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string FileToDelete { get; set; }
        public bool UploadSuccess { get; set; }
        public List<string> UploadedDocs { get; set; } = new();

        public ProjectDetailsModel(IWebHostEnvironment env)
        {
            _env = env; //inject environment
            ProjectDetail = new Projects();
            ProjectNotes = new ProjectNote();
            ProjectNotesList = new List<ProjectNote>();
        }

        public string GetProgressBarColor(int percent)
        {
            if (percent >= 80) return "bg-success";
            if (percent >= 50) return "bg-info";
            if (percent >= 25) return "bg-warning";
            return "bg-danger";
        }

        public IActionResult OnGet(int projectid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            ProjectID = projectid;

            SqlDataReader singleProject = DBClass.SingleProjectReader(projectid);
            while (singleProject.Read())
            {
                ProjectDetail.Project_ID = projectid;
                ProjectDetail.Title = singleProject["Title"].ToString();
                ProjectDetail.Due_Date = DateTime.Parse(singleProject["Due_Date"].ToString());
                ProjectDetail.Status = singleProject["Status"].ToString();
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader notesReader = DBClass.SingleNoteReader(projectid);
            ProjectNotesList = new List<ProjectNote>();

            while (notesReader.Read())
            {
                ProjectNotesList.Add(
                    new ProjectNote
                    {
                        Project_ID = projectid,
                        //Note_ID = Int32.Parse(notesReader["Note_ID"].ToString()),
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();

            ProjectProgress = DBClass.CalculateProjectProgress(projectid);
            LoadUploadedFiles();

            return Page();
        }
        public IActionResult OnPostUpdateStatus(int ProjectID, string Status)
        {
            if (!StatusOptions.Contains(Status))
            {
                return Page();
            }

            string userRole = HttpContext.Session.GetString("Role");
            int? loggedInUserID = HttpContext.Session.GetInt32("UserID");

            if (userRole != "Center Director" && userRole != "Admin Staff" || loggedInUserID == null)
            {
                TempData["ErrorMessage"] = "You are not authorized to update this project status.";
                return RedirectToPage();
            }
            DBClass.Lab2DBConnection.Close();

            DBClass.UpdateProjectStatus(ProjectID, Status);

            return RedirectToPage(new { projectid=ProjectID});
        }
        public string GetStatusColor(string status)
        {
            return status switch
            {
                "Awarded" => "green",
                "Archived" => "red",
                _ => "gray"
            };
        }
        public IActionResult OnPost()
        {
            DBClass.ReadOneProject(ProjectDetail);
            DBClass.ReadOneNote(ProjectNotes);

            DBClass.Lab2DBConnection.Close();

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Upload != null && ProjectID > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "projects", ProjectID.ToString());

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, Upload.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }

                UploadSuccess = true;
            }

            SqlDataReader singleProject = DBClass.SingleProjectReader(ProjectID);
            while (singleProject.Read())
            {
                ProjectDetail.Project_ID = ProjectID;
                ProjectDetail.Title = singleProject["Title"].ToString();
                ProjectDetail.Due_Date = DateTime.Parse(singleProject["Due_Date"].ToString());
                ProjectDetail.Status = singleProject["Status"].ToString();
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader notesReader = DBClass.SingleNoteReader(ProjectID);
            ProjectNotesList = new List<ProjectNote>();

            while (notesReader.Read())
            {
                ProjectNotesList.Add(
                    new ProjectNote
                    {
                        Project_ID = ProjectID,
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();
            ProjectProgress = DBClass.CalculateProjectProgress(ProjectID);

            LoadUploadedFiles();
            return Page();
        }

        public IActionResult OnPostDeleteAsync()
        {
            if (!string.IsNullOrEmpty(FileToDelete) && ProjectID > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "projects", ProjectID.ToString());
                string filePath = Path.Combine(folder, FileToDelete);

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            SqlDataReader singleProject = DBClass.SingleProjectReader(ProjectID);
            while (singleProject.Read())
            {
                ProjectDetail.Project_ID = ProjectID;
                ProjectDetail.Title = singleProject["Title"].ToString();
                ProjectDetail.Due_Date = DateTime.Parse(singleProject["Due_Date"].ToString());
                ProjectDetail.Status = singleProject["Status"].ToString();
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader notesReader = DBClass.SingleNoteReader(ProjectID);
            ProjectNotesList = new List<ProjectNote>();

            while (notesReader.Read())
            {
                ProjectNotesList.Add(
                    new ProjectNote
                    {
                        Project_ID = ProjectID,
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();
            ProjectProgress = DBClass.CalculateProjectProgress(ProjectID);

            LoadUploadedFiles();
            return Page();
        }

        private void LoadUploadedFiles()
        {
            string folder = Path.Combine(_env.WebRootPath, "uploads", "projects", ProjectID.ToString());

            if (Directory.Exists(folder))
            {
                UploadedDocs = Directory.GetFiles(folder)
                    .Select(f => $"/uploads/projects/{ProjectID}/{Path.GetFileName(f)}")
                    .ToList();
            }
        }
    }
}
