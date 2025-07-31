using Lab2.Pages.DB;
using Lab2.Pages.DataClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Text;
// Used https://www.aspsnippets.com/Articles/3793/ASPNet-Core-Razor-Pages-Export-data-from-database-to-CSV-File/
// to learn how to export data to CSV files in Razor Pages.
// Also used ChatGPT to help adjust the export to work with my stored procedure, custom SearchResult class, and preserve Razor Pages formatting with session filters.

namespace Lab2.Pages.Admin
{
    public class AdminSearchModel : PageModel
    {
        [BindProperty] public DateTime? StartDate { get; set; }
        [BindProperty] public DateTime? EndDate { get; set; }
        [BindProperty] public string ProjectStatus { get; set; }
        [BindProperty] public string GrantStatus { get; set; }
        [BindProperty] public string FacultyName { get; set; }
        [BindProperty] public string ExternalPartnerName { get; set; }
        [BindProperty] public string FundingSource { get; set; }


        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            return Page();
        }

        public void OnPost()
        {
            Results = GetSearchResults();
        }

        //Chatgpt helped formatting to ensure exporting is done properly
        public IActionResult OnPostExportCsv()
        {
            var results = GetSearchResults();

            var csv = new StringBuilder();
            csv.AppendLine("Project Title,Project Status,Due Date,Grant Title,Grant Status,Submission Date,Award Date,Amount,Faculty Name,Faculty Email,Partner Org,Funding Source");

            foreach (var r in results)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    r.ProjectTitle,
                    r.ProjectStatus,
                    r.DueDate.ToString("yyyy-MM-dd"),
                    r.GrantTitle,
                    r.GrantStatus,
                    r.SubmissionDate.ToString("yyyy-MM-dd"),
                    r.AwardDate.ToString("yyyy-MM-dd"),
                    r.Amount.ToString(),
                    r.FacultyName,
                    r.FacultyEmail,
                    r.ExternalPartnerOrgName,
                    r.FundingSource
                }.Select(field => $"\"{field}\"")));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "ReportResults.csv");
        }

        private List<SearchResult> GetSearchResults()
        {
            var results = new List<SearchResult>();
            SqlDataReader reader = DBClass.RunSearchProcedure(StartDate, EndDate, ProjectStatus, GrantStatus, FacultyName, ExternalPartnerName);

            while (reader.Read())
            {
                results.Add(new SearchResult
                {
                    ProjectID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    ProjectTitle = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                    DueDate = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                    ProjectStatus = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3),
                    GrantID = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    GrantTitle = reader.IsDBNull(5) ? "N/A" : reader.GetString(5),
                    GrantStatus = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                    SubmissionDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                    AwardDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                    Amount = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                    FacultyName = (reader.IsDBNull(10) ? "" : reader.GetString(10)) + " " +
                                  (reader.IsDBNull(11) ? "" : reader.GetString(11)),
                    FacultyEmail = reader.IsDBNull(12) ? "None" : reader.GetString(12),
                    ExternalPartnerOrgName = reader.IsDBNull(13) ? "N/A" : reader.GetString(13),
                    FundingSource = reader.IsDBNull(14) ? "N/A" : reader.GetString(14)

                });
            }

            reader.Close();
            DBClass.Lab2DBConnection.Close();

            return results;
        }

        public class SearchResult
        {
            public int? ProjectID { get; set; }
            public string ProjectTitle { get; set; }
            public DateTime DueDate { get; set; }
            public string ProjectStatus { get; set; }

            public int GrantID { get; set; }
            public string GrantTitle { get; set; }
            public string GrantStatus { get; set; }
            public DateTime SubmissionDate { get; set; }
            public DateTime AwardDate { get; set; }
            public decimal Amount { get; set; }

            public string FacultyName { get; set; }
            public string FacultyEmail { get; set; }
            public string ExternalPartnerOrgName { get; set; }
            public string FundingSource { get; set; }

        }
    }
}