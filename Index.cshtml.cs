using System.Data.SqlClient;
using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lab2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<Dates> UpcomingDates { get; set; }

        public List<Tasks> UserTasks { get; set; } = new List<Tasks>();

        public List<Grants> AssignedGrants { get; set; } = new List<Grants>();


        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            UpcomingDates = new List<Dates>();
        }

        public int userId { get; set; } = 0;
        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string? UserFirst { get; set; }
        public string? UserLast { get; set; }
        public decimal CompletionPercent { get; set; }
        //chat helped with the progress bar

        public void OnGet()
        {
            UserEmail = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(UserEmail))
            {
                Response.Redirect("/Login/ParameterizedLogin");
                return;
            }

            UserEmail = HttpContext.Session.GetString("Email");
            UserRole = HttpContext.Session.GetString("Role");
            UserFirst = HttpContext.Session.GetString("FirstName");
            UserLast = HttpContext.Session.GetString("LastName");
            userId = HttpContext.Session.GetInt32("UserID") ?? 0;


            SqlDataReader progressReader = DBClass.GetUserTaskProgress(userId);
            if (progressReader.Read())
            {
                CompletionPercent = Convert.ToDecimal(progressReader["PercentComplete"]);
            }
            progressReader.Close();
            DBClass.Lab2DBConnection.Close();

            //chat helped me clean this up
            SqlDataReader datesReader = DBClass.GetImportantDatesForCurrentMonth();

            while (datesReader.Read())
            {
                UpcomingDates.Add(new Dates
                {
                    Date = Convert.ToDateTime(datesReader["Date"]),
                    DateType = datesReader["DateType"].ToString(),
                    Description = datesReader["Description"].ToString()
                });
            }
            datesReader.Close();
            DBClass.Lab2DBConnection.Close();

            // User's tasks, chat helped
            SqlDataReader taskReader = DBClass.TaskReaderByUser(userId);
            while (taskReader.Read())
            {
                Tasks task = new Tasks
                {
                    Title = taskReader["Title"].ToString(),
                    Description = taskReader["Description"].ToString(),
                    Priority = taskReader["Priority"].ToString(),
                    Status = taskReader["Status"].ToString(),
                    Due_Date = Convert.ToDateTime(taskReader["Due_Date"]),
                    ProjectTitle = taskReader["ProjectTitle"].ToString()
                };

                UserTasks.Add(task);
            }
            taskReader.Close();
            DBClass.Lab2DBConnection.Close();

            //users grants chat helped some
            SqlDataReader reader = DBClass.GrantsByUser(userId);

            while (reader.Read())
            {
                Grants g = new Grants
                {
                    Grant_ID = Convert.ToInt32(reader["Grant_ID"]),
                    Title = reader["Title"].ToString(),
                    Status = reader["Status"].ToString()
                };
                AssignedGrants.Add(g);
            }

            DBClass.Lab2DBConnection.Close();
        }
    }
}
