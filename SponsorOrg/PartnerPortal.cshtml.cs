using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.IO;

namespace Lab2.Pages.Partner
{
    public class PartnerPortalModel : PageModel
    {
        [BindProperty]
        public IFormFile PartnerUpload { get; set; }

        [BindProperty]
        public string PartnerNote { get; set; }

        [BindProperty]
        public string FileType { get; set; }

        public string OrgName { get; set; }
        public string Status { get; set; }

        public List<FileUpload> UploadedFiles { get; set; } = new();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Industry Partner")
            {
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            string email = HttpContext.Session.GetString("Email");

            // Get organization details
            var reader = DBClass.GetPartnerPortalInfo(email);
            if (reader.Read())
            {
                OrgName = reader["OrgName"].ToString();
                Status = reader["Status_Flag"].ToString();
            }
            reader.Close();
            DBClass.Lab2DBConnection.Close();

            // Get user ID
            SqlDataReader userReader = DBClass.GetUserDetailsByEmail(email);
            int userID = -1;
            if (userReader.Read())
            {
                userID = Convert.ToInt32(userReader["User_ID"]);
            }
            userReader.Close();
            DBClass.Lab2DBConnection.Close();

            // Get uploaded files
            SqlDataReader fileReader = DBClass.GetFilesUploadedByUser(userID);
            while (fileReader.Read())
            {
                UploadedFiles.Add(new FileUpload
                {
                    FileName = fileReader["FileName"].ToString(),
                    UploadDate = Convert.ToDateTime(fileReader["UploadDate"]),
                    Note = fileReader["Note"].ToString(),
                    FileType = fileReader["FileType"].ToString()
                });
            }
            fileReader.Close();
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("Role") != "Industry Partner")
            {
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            if (PartnerUpload != null && PartnerUpload.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fileupload", PartnerUpload.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    PartnerUpload.CopyTo(stream);
                }

                string email = HttpContext.Session.GetString("Email");

                // Get sponsor org ID
                SqlDataReader orgReader = DBClass.GetSponsorOrgIdByEmail(email);
                int sponsorOrgId = -1;
                if (orgReader.Read())
                {
                    sponsorOrgId = Convert.ToInt32(orgReader["SponsorOrg_ID"]);
                }
                orgReader.Close();
                DBClass.Lab2DBConnection.Close();

                // Get user ID
                SqlDataReader userReader = DBClass.GetUserDetailsByEmail(email);
                int userID = -1;
                if (userReader.Read())
                {
                    userID = Convert.ToInt32(userReader["User_ID"]);
                }
                userReader.Close();
                DBClass.Lab2DBConnection.Close();

                // Save file to database
                DBClass.SaveFileUpload(PartnerUpload.FileName, PartnerNote, null, null, sponsorOrgId, FileType, userID);
                DBClass.Lab2DBConnection.Close();
            }

            return RedirectToPage("PartnerPortal");
        }
    }
}
