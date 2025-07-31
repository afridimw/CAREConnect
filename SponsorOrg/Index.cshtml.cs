using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages.SponsorOrg
{
    public class IndexModel : PageModel
    {
        public Dictionary<string, int> StatusCounts { get; set; } = new();

        public List<SponsorOrgs> SponsorOrgs { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        public IndexModel()
        {
            SponsorOrgs = new List<SponsorOrgs>();
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin");
            }


            SqlDataReader orgReader = DBClass.OrgReader();
            while (orgReader.Read())
            {
                int sponsorOrgId = Convert.ToInt32(orgReader["SponsorOrg_ID"]);
                var status = orgReader["Status_Flag"].ToString();

                SponsorOrgs sponsorOrg = new SponsorOrgs
                {
                    SponsorOrg_ID = sponsorOrgId,
                    OrgName = orgReader["OrgName"].ToString(),
                    Sector = orgReader["Sector"].ToString(),
                    Status_Flag = status,
                };

                if (StatusCounts.ContainsKey(status))
                {
                    StatusCounts[status]++;
                }
                else
                {
                    StatusCounts[status] = 1;
                }

                if (string.IsNullOrEmpty(SelectedStatus) || sponsorOrg.Status_Flag == SelectedStatus)
                {
                    SponsorOrgs.Add(sponsorOrg);
                }
            }

            orgReader.Close();
            DBClass.Lab2DBConnection.Close();

            // Chart ViewData setup - help from CHAT
            ViewData["CountProspect"] = SponsorOrgs.Count(x => x.Status_Flag == "Prospect");
            ViewData["CountInitialContact"] = SponsorOrgs.Count(x => x.Status_Flag == "Initial Contact");
            ViewData["CountNegotiation"] = SponsorOrgs.Count(x => x.Status_Flag == "Negotiation");
            ViewData["CountMOUSigned"] = SponsorOrgs.Count(x => x.Status_Flag == "MOU Signed");
            ViewData["CountActive"] = SponsorOrgs.Count(x => x.Status_Flag == "Active");
            ViewData["CountOnHold"] = SponsorOrgs.Count(x => x.Status_Flag == "On Hold");
            ViewData["CountInactive"] = SponsorOrgs.Count(x => x.Status_Flag == "Inactive");

            return Page();
        }

        public int GetProgressPercent(string status)
        {
            return status switch
            {
                "Prospect" => 10,
                "Initial Contact" => 20,
                "Negotiation" => 40,
                "MOU Signed" => 60,
                "Active" => 100,
                "On Hold" => 50,
                "Inactive" => 0,
                _ => 0
            };
        }

        public string GetProgressBarColor(int percent)
        {
            if (percent >= 80) return "bg-success";
            if (percent >= 50) return "bg-info";
            if (percent >= 25) return "bg-warning";
            return "bg-danger";
        }
    }
}
