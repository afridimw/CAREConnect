using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Lab2.Pages.Grant
{
    public class GrantDetailsModel : PageModel
    {
        [BindProperty]
        public Grants GrantDetail { get; set; }
        [BindProperty]
        public List<Users> GrantUser { get; set; }
        [BindProperty]
        public int GrantID { get; set; }

        [BindProperty]
        public GrantNotes GrantNotes { get; set; }
        public List<GrantNotes> GrantNotesList { get; set; }

        // ?? Added for file upload
        private readonly IWebHostEnvironment _env;
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string FileToDelete { get; set; }
        public bool UploadSuccess { get; set; }
        public List<string> UploadedDocs { get; set; } = new();

        public GrantDetailsModel(IWebHostEnvironment env) //Chat added this in order to inject a new environment
        {
            _env = env; 
            GrantDetail = new Grants();
            GrantUser = new List<Users>();
            GrantNotesList = new List<GrantNotes>();
            GrantNotes = new GrantNotes();
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            GrantID = HttpContext.Session.GetInt32("GrantID") ?? 0;

            if (GrantID == 0)
            {
                TempData["ErrorMessage"] = "No grant selected.";
                return RedirectToPage("/Grant/Index");
            }

            SqlDataReader singleGrant = DBClass.SingleGrantReader(GrantID);

            if (singleGrant.Read())
            {
                GrantDetail.Grant_ID = Convert.ToInt32(singleGrant["Grant_ID"]);
                GrantDetail.Title = singleGrant["Title"]?.ToString();
                GrantDetail.Category = singleGrant["Category"]?.ToString();
                GrantDetail.Submission_Date = string.IsNullOrEmpty(singleGrant["Submission_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Submission_Date"].ToString());
                GrantDetail.Award_Date = string.IsNullOrEmpty(singleGrant["Award_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Award_Date"].ToString());
                GrantDetail.AmountRequested = string.IsNullOrEmpty(singleGrant["AmountRequested"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountRequested"]);
                GrantDetail.AmountAwarded = string.IsNullOrEmpty(singleGrant["AmountAwarded"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountAwarded"]);
                GrantDetail.Deadline = string.IsNullOrEmpty(singleGrant["Deadline"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Deadline"].ToString());
                GrantDetail.Pursue = singleGrant["Pursue"]?.ToString();
                GrantDetail.Status = singleGrant["Status"]?.ToString();
                GrantDetail.SponsorOrg_ID = string.IsNullOrEmpty(singleGrant["SponsorOrg_ID"]?.ToString())
                    ? (int?)null : Convert.ToInt32(singleGrant["SponsorOrg_ID"]);
            }
            DBClass.Lab2DBConnection.Close();

            LoadFacultyForGrant();

            SqlDataReader notesReader = DBClass.SingleGrantNoteReader(GrantID);
            GrantNotesList = new List<GrantNotes>();

            while (notesReader.Read())
            {
                GrantNotesList.Add(
                    new GrantNotes
                    {
                        Grant_ID = GrantID,
                        GrantNote_ID = Int32.Parse(notesReader["GrantNote_ID"].ToString()),
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();

            LoadUploadedFiles(); 

            return Page();
        }

        public IActionResult OnPost(int User_id)
        {
            GrantID = HttpContext.Session.GetInt32("GrantID") ?? 0;

            if (GrantID == 0)
            {
                TempData["ErrorMessage"] = "GrantID is missing. Please try again.";
                return RedirectToPage("/Grant/Index");
            }

            DBClass.ReadOneGrantNote(GrantNotes);
            DBClass.Lab2DBConnection.Close();

            if (User_id > 0)
            {
                DBClass.RemoveUserFromGrant(GrantID, User_id);
                DBClass.Lab2DBConnection.Close();
            }

            return RedirectToPage("/Grant/GrantDetails");
        }

        
        public async Task<IActionResult> OnPostUploadAsync() //Chat helped work with/ debug this in order to handle file upload
        {
            GrantID = HttpContext.Session.GetInt32("GrantID") ?? 0;

            if (Upload != null && GrantID > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "grants", GrantID.ToString());

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, Upload.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }

                UploadSuccess = true;
            }

            SqlDataReader singleGrant = DBClass.SingleGrantReader(GrantID);

            if (singleGrant.Read())
            {
                GrantDetail.Grant_ID = Convert.ToInt32(singleGrant["Grant_ID"]);
                GrantDetail.Title = singleGrant["Title"]?.ToString();
                GrantDetail.Category = singleGrant["Category"]?.ToString();
                GrantDetail.Submission_Date = string.IsNullOrEmpty(singleGrant["Submission_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Submission_Date"].ToString());
                GrantDetail.Award_Date = string.IsNullOrEmpty(singleGrant["Award_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Award_Date"].ToString());
                GrantDetail.AmountRequested = string.IsNullOrEmpty(singleGrant["AmountRequested"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountRequested"]);
                GrantDetail.AmountAwarded = string.IsNullOrEmpty(singleGrant["AmountAwarded"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountAwarded"]);
                GrantDetail.Deadline = string.IsNullOrEmpty(singleGrant["Deadline"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Deadline"].ToString());
                GrantDetail.Pursue = singleGrant["Pursue"]?.ToString();
                GrantDetail.Status = singleGrant["Status"]?.ToString();
                GrantDetail.SponsorOrg_ID = string.IsNullOrEmpty(singleGrant["SponsorOrg_ID"]?.ToString())
                    ? (int?)null : Convert.ToInt32(singleGrant["SponsorOrg_ID"]);
            }
            DBClass.Lab2DBConnection.Close();

            LoadFacultyForGrant();

            SqlDataReader notesReader = DBClass.SingleGrantNoteReader(GrantID);
            GrantNotesList = new List<GrantNotes>();

            while (notesReader.Read())
            {
                GrantNotesList.Add(
                    new GrantNotes
                    {
                        Grant_ID = GrantID,
                        GrantNote_ID = Int32.Parse(notesReader["GrantNote_ID"].ToString()),
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();

            LoadUploadedFiles();
            return Page();
        }

        
        public IActionResult OnPostDeleteAsync() //Chat helped work with/ debug this in order to delete files
        {
            GrantID = HttpContext.Session.GetInt32("GrantID") ?? 0;

            if (!string.IsNullOrEmpty(FileToDelete) && GrantID > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "grants", GrantID.ToString());
                string filePath = Path.Combine(folder, FileToDelete);

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            SqlDataReader singleGrant = DBClass.SingleGrantReader(GrantID);

            if (singleGrant.Read())
            {
                GrantDetail.Grant_ID = Convert.ToInt32(singleGrant["Grant_ID"]);
                GrantDetail.Title = singleGrant["Title"]?.ToString();
                GrantDetail.Category = singleGrant["Category"]?.ToString();
                GrantDetail.Submission_Date = string.IsNullOrEmpty(singleGrant["Submission_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Submission_Date"].ToString());
                GrantDetail.Award_Date = string.IsNullOrEmpty(singleGrant["Award_Date"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Award_Date"].ToString());
                GrantDetail.AmountRequested = string.IsNullOrEmpty(singleGrant["AmountRequested"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountRequested"]);
                GrantDetail.AmountAwarded = string.IsNullOrEmpty(singleGrant["AmountAwarded"]?.ToString())
                    ? (double?)null : Convert.ToDouble(singleGrant["AmountAwarded"]);
                GrantDetail.Deadline = string.IsNullOrEmpty(singleGrant["Deadline"]?.ToString())
                    ? (DateTime?)null : DateTime.Parse(singleGrant["Deadline"].ToString());
                GrantDetail.Pursue = singleGrant["Pursue"]?.ToString();
                GrantDetail.Status = singleGrant["Status"]?.ToString();
                GrantDetail.SponsorOrg_ID = string.IsNullOrEmpty(singleGrant["SponsorOrg_ID"]?.ToString())
                    ? (int?)null : Convert.ToInt32(singleGrant["SponsorOrg_ID"]);
            }
            DBClass.Lab2DBConnection.Close();

            LoadFacultyForGrant();

            SqlDataReader notesReader = DBClass.SingleGrantNoteReader(GrantID);
            GrantNotesList = new List<GrantNotes>();

            while (notesReader.Read())
            {
                GrantNotesList.Add(
                    new GrantNotes
                    {
                        Grant_ID = GrantID,
                        GrantNote_ID = Int32.Parse(notesReader["GrantNote_ID"].ToString()),
                        Timestamp = DateTime.Parse(notesReader["Timestamp"].ToString()),
                        Content = notesReader["Content"].ToString()
                    });
            }
            DBClass.Lab2DBConnection.Close();

            LoadUploadedFiles();
            return Page();
        }

        
        private void LoadUploadedFiles() //Chat helped work with/ debug this in order to load uploaded files 
        {
            string folder = Path.Combine(_env.WebRootPath, "uploads", "grants", GrantID.ToString());

            if (Directory.Exists(folder))
            {
                UploadedDocs = Directory.GetFiles(folder)
                    .Select(f => $"/uploads/grants/{GrantID}/{Path.GetFileName(f)}")
                    .ToList();
            }
        }

        private void LoadFacultyForGrant()
        {
            GrantUser = new List<Users>();
            SqlDataReader userByGrantReader = DBClass.UsersByGrant(GrantID);
            while (userByGrantReader.Read())
            {
                Users grantUser = new Users
                {
                    User_ID = Convert.ToInt32(userByGrantReader["User_ID"]),
                    LastName = userByGrantReader["LastName"].ToString(),
                    FirstName = userByGrantReader["FirstName"].ToString(),
                    Email = userByGrantReader["Email"].ToString()
                };
                GrantUser.Add(grantUser);
            }
            DBClass.Lab2DBConnection.Close();
        }
    }
}
