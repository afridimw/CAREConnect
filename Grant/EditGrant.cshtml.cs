using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Reflection;

namespace Lab2.Pages.Grant
{
    public class EditGrantModel : PageModel
    {
        [BindProperty]
        public Grants GrantToUpdate { get; set; }

        public int GrantID { get; set; }
		public int UserID { get; set; }
        public int PermID { get; set; }

		public IActionResult OnGet(int grantiD)
        {
            GrantID = grantiD;
            UserID = HttpContext.Session.GetInt32("UserID") ?? 0;
			

			SqlDataReader getperm = DBClass.GetPermissionForGrantCommand(UserID, GrantID);

			//int permId = 0;
			if (getperm.Read())
			{
				PermID = Convert.ToInt32(getperm["perm_ID"]);
			}

			getperm.Close(); // ? YOU close the reader
			DBClass.Lab2DBConnection.Close(); // ? YOU close the connection


			if (GrantID == 0)
            {
                TempData["ErrorMessage"] = "No grant selected to edit.";
                return RedirectToPage("/Grant/Index");
            }

            SqlDataReader reader = DBClass.SingleGrantReader(GrantID); // this returns one row
            if (reader.Read())
            {
                GrantToUpdate = new Grants
                {
                    Grant_ID = GrantID,
                    Title = reader["Title"].ToString(),
                    Category = reader["Category"].ToString(),
                    Type = reader["Type"].ToString(),
                    Submission_Date = Convert.ToDateTime(reader["Submission_Date"]),
                    Award_Date = reader["Award_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Award_Date"]) : (DateTime?)null,
                    Deadline = Convert.ToDateTime(reader["Deadline"]),
                    AmountRequested = Convert.ToDouble(reader["AmountRequested"]),
                    AmountAwarded = reader["AmountAwarded"] != DBNull.Value ? Convert.ToDouble(reader["AmountAwarded"]) : (double?)null,
                    Status = reader["Status"].ToString(),
                    Pursue = reader["Pursue"].ToString()
                };
            }

            reader.Close();
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            DBClass.EditGrant(GrantToUpdate);  // assumes you have this
            TempData["SuccessMessage"] = "Grant updated successfully!";
            return RedirectToPage("Index");
        }
    }
}

