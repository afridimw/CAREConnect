using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Lab2.Pages.Grant
{
    public class GraphModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<(DateTime AwardDate, double Amount)> DataPoints { get; set; } = new();

        public void OnGet()
        {
            if (StartDate == null || EndDate == null)
                return;

            SqlDataReader reader = DBClass.GrantGraphReader(StartDate, EndDate);
            while (reader.Read())
            {
                DateTime awardDate = Convert.ToDateTime(reader["Award_Date"]);
                double amount = Convert.ToDouble(reader["AmountAwarded"]);
                DataPoints.Add((awardDate, amount));
            }

            reader.Close();
            DBClass.Lab2DBConnection.Close();
        }
    }
}


