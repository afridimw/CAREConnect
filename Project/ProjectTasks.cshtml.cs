using Lab2.Pages.DataClasses;
using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Lab2.Pages.Project
{
    public class ProjectTasksModel : PageModel
    {
        public Tasks Tasks { get; set; }

        public List<Users> UserList { get; set; }

        [BindProperty]
        public int AssignedUserID { get; set; }

        public List<Tasks> ProjectTaskList { get; set; }
        public ProjectTasksModel()
        {
            Tasks = new Tasks();
            ProjectTaskList = new List<Tasks>();
        }

        [BindProperty]
        public int TaskID { get; set; }

        [BindProperty]
        public string NewStatus { get; set; }

        [BindProperty]
        public int ProjectID { get; set; }

        [BindProperty]
        public string NoteContent { get; set; }

        public Dictionary<int, List<TaskNote>> TaskNotes { get; set; }

        public IActionResult OnGet(int projectid)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            ProjectID = projectid;


            SqlDataReader tasksReader = DBClass.SingleTaskReader(projectid);
            while (tasksReader.Read())
            {
                Tasks.Task_ID = Int32.Parse(tasksReader["Task_id"].ToString());
                Tasks.Project_ID = projectid;
                Tasks.Title = tasksReader["Title"].ToString();
                Tasks.Status = tasksReader["Status"].ToString();
                Tasks.AssignedUser = Int32.Parse(tasksReader["AssignedUser"].ToString());
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader taskReader = DBClass.TaskReader(projectid);
            ProjectTaskList = new List<Tasks>();
            while (taskReader.Read())
            {
                ProjectTaskList.Add(
                    new Tasks
                    {
                        Project_ID = projectid,
                        Task_ID = Int32.Parse(taskReader["Task_ID"].ToString()),
                        Title = taskReader["Title"].ToString(),
                        Status = taskReader["Status"].ToString(),
                        AssignedUser = Int32.Parse(taskReader["AssignedUser"].ToString())
                    });
            }
            DBClass.Lab2DBConnection.Close();

            SqlDataReader userReader = DBClass.AllJMUCAREUsers();
            UserList = new List<Users>();
            while (userReader.Read())
            {
                UserList.Add(new Users
                {
                    User_ID = Convert.ToInt32(userReader["User_ID"]),
                    FirstName = userReader["FirstName"].ToString(),
                    LastName = userReader["LastName"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();

            TaskNotes = new Dictionary<int, List<TaskNote>>();
            foreach (var task in ProjectTaskList)
            {
                var noteReader = DBClass.GetTaskNotes(task.Task_ID);
                List<TaskNote> notes = new List<TaskNote>();

                while (noteReader.Read())
                {
                    notes.Add(new TaskNote
                    {
                        TaskNote_ID = Convert.ToInt32(noteReader["TaskNote_ID"]),
                        Timestamp = Convert.ToDateTime(noteReader["Timestamp"]),
                        Content = noteReader["Content"].ToString(),
                        Task_ID = Convert.ToInt32(noteReader["Task_ID"])
                    });
                }
                TaskNotes[task.Task_ID] = notes;
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            //figures out what form was submitted on the view
            string submitType = Request.Form["submitType"];
            if (submitType == "status")
            {
                DBClass.UpdateTaskStatus(TaskID, NewStatus);
            }
            else if (submitType == "user")
            {
                DBClass.UpdateTaskAssignment(TaskID, AssignedUserID);
            }
            else if (submitType == "note")
            {
                DBClass.InsertTaskNote(TaskID, NoteContent);
            }
            return RedirectToPage("/Project/ProjectTasks", new {projectid = ProjectID});
        }
        
    }
}
