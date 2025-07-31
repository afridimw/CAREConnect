using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using Lab2.Pages.DB;
using Lab2.Pages.DataClasses;
using System.Reflection.PortableExecutable;
//Chat helped pull files uploaded by partners for admin view
namespace Lab2.Pages.SponsorOrg
{
    public class SponsorOrgDetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int SponsorOrgID { get; set; }

        [BindProperty]
        public string OrgName { get; set; }

        [BindProperty]
        public string StatusFlag { get; set; }

        [BindProperty]
        public string Sector { get; set; }
		[BindProperty]
		public IFormFile SponsorUpload { get; set; }

		[BindProperty]
		public string SponsorNote { get; set; }

		[BindProperty]
		public string FileType { get; set; }
        public bool IsInactiveByUpload { get; set; }



		public List<FileUpload> PartnerUploads { get; set; } = new List<FileUpload>();
		public IActionResult OnPost(int sponsorOrgId)
		{
			SponsorOrgID = sponsorOrgId;

			if (SponsorUpload != null && SponsorUpload.Length > 0)
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fileupload", SponsorUpload.FileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					SponsorUpload.CopyTo(stream);
				}

                string email = HttpContext.Session.GetString("Email");

                int User_ID = -1;
                SqlDataReader userReader = DBClass.GetUserDetailsByEmail(email);
                if (userReader.Read())
                {
                    User_ID = Convert.ToInt32(userReader["User_ID"]);
                }
                userReader.Close();
                DBClass.Lab2DBConnection.Close();

                DBClass.SaveFileUpload(SponsorUpload.FileName, SponsorNote, null, null, sponsorOrgId, FileType, User_ID);
				DBClass.Lab2DBConnection.Close();
			}

			return RedirectToPage("SponsorOrgDetails", new { sponsorOrgId = sponsorOrgId });
		}

		public IActionResult OnGet(int sponsorOrgId)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            SqlDataReader orgReader = DBClass.SingleOrgReader(sponsorOrgId);
            if (orgReader.Read())
            {
                OrgName = orgReader["OrgName"].ToString();
                StatusFlag = orgReader["Status_Flag"].ToString();
                Sector = orgReader["Sector"].ToString();
            }
            orgReader.Close();
            DBClass.Lab2DBConnection.Close();

            SqlDataReader uploadReader = DBClass.GetUploadsForSponsorOrg(sponsorOrgId);
            while (uploadReader.Read())
            {
                PartnerUploads.Add(new FileUpload
                {
                    FileName = uploadReader["FileName"].ToString(),
                    UploadDate = Convert.ToDateTime(uploadReader["UploadDate"]),
                    Note = uploadReader["Note"].ToString(),
                    FileType = uploadReader["FileType"].ToString()
                });
            }
            uploadReader.Close();
            DBClass.Lab2DBConnection.Close();

            if (StatusFlag == "Inactive")
            {
                SqlDataReader reader = DBClass.GetLastUploadDateReader(sponsorOrgId);
                DateTime? lastUpload = null;

                if (reader.Read() && reader["LastUpload"] != DBNull.Value)
                {
                    lastUpload = Convert.ToDateTime(reader["LastUpload"]);
                }

                reader.Close();
                DBClass.Lab2DBConnection.Close();
                // CHATGPT helped track date to implement 30 days before CURRENT

                IsInactiveByUpload = !lastUpload.HasValue || lastUpload.Value < DateTime.Now.AddDays(-30);
            }
            else
            {
                IsInactiveByUpload = false;
            }


            return Page();
        }
    }
}
