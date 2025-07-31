using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using Lab2.Pages.Admin;
using Lab2.Pages.DataClasses;
using Lab2.Pages.Login;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

namespace Lab2.Pages.DB
{
    public class DBClass
    {
        // Use this class to define methods that make connecting to
        // and retrieving data from the DB easier.

        // Connection Object at Data Field Level
        // the cross out is just mad bc we are using the old version
        public static SqlConnection Lab2DBConnection = new SqlConnection();
        //Connection Object at Data Field Level for AUTH DB
        public static SqlConnection AuthDBConnection = new SqlConnection();

        ////Connection String - How to find and connect to DB
        private static readonly String? Lab2DBConnString =
            "Server=LocalHost;Database=JMUCARE;Trusted_Connection=True"; //UPDATED FOR JMU CARE

        ////Connection String to AUTH database
        private static readonly String? AuthConnString =
            "Server=Localhost;Database=AUTH;Trusted_Connection=True";

        ////Connection String for Azure
        //private static readonly String? Lab2DBConnString =
        //    "Server=tcp:jmucare-capstone.database.windows.net,1433;" +
        //    "Initial Catalog=JMUCARE;Persist Security Info=False;" +
        //    "User ID=JMUCareAdmin;Password=C@pst0neTe@mF;" +
        //    "MultipleActiveResultSets=False;Encrypt=True;" +
        //    "TrustServerCertificate=False;Connection Timeout=30;";

        ////Azure Connection String for AUTH DB
        //private static readonly String? AuthConnString =
        //    "Server=tcp:jmucare-capstone.database.windows.net,1433;" +
        //    "Initial Catalog=AUTH;Persist Security Info=False;" +
        //    "User ID=JMUCareAdmin;Password=C@pst0neTe@mF;" +
        //    "MultipleActiveResultSets=False;Encrypt=True;" +
        //    "TrustServerCertificate=False;Connection Timeout=30;";

        //Connection Methods:

        //                    GRANTS
        //Basic Grants Reader

        // JMUCARE STATUS : no change needed


        public static SqlDataReader GrantsReader()
        {
            SqlCommand cmdGrantsRead = new SqlCommand();
            cmdGrantsRead.Connection = Lab2DBConnection;
            cmdGrantsRead.Connection.ConnectionString = Lab2DBConnString;
            cmdGrantsRead.CommandText = "SELECT * FROM GRANTS";

            cmdGrantsRead.Connection.Open(); // Open connection here, close in Model!
            SqlDataReader tempReader = cmdGrantsRead.ExecuteReader();

            return tempReader;
        }

        //got this from chat and it works better than ^^ but there was still an error
        //public static SqlDataReader GrantsReader()
        //{
        //    SqlConnection conn = new SqlConnection(Lab2DBConnString);
        //    conn.Open();

        //    SqlCommand cmd = new SqlCommand("SELECT Grant_id FROM Grants", conn);

        //    // Return reader, and auto-close connection when reader is closed
        //    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //}



        // JMUCARE STATUS : complete
        // change > Grant_id > Grant_ID
        public static SqlDataReader SingleGrantReader(int Grant_id)
        {
            SqlCommand cmdSingleGrant = new SqlCommand();
            cmdSingleGrant.Connection = Lab2DBConnection;
            cmdSingleGrant.Connection.ConnectionString = Lab2DBConnString;
            cmdSingleGrant.CommandText = "SELECT * FROM Grants WHERE Grant_ID = @GrantID";

            cmdSingleGrant.Parameters.AddWithValue("@GrantID", Grant_id);

            cmdSingleGrant.Connection.Open();
            SqlDataReader tempReader = cmdSingleGrant.ExecuteReader();
            return tempReader;
        }

        // JMUCARE STATUS : complete
        // Grants (Title, Category, Type, Submission_Date, Award_Date, Deadline, AmountRequested, AmountAwarded, Status, SponsorOrg_ID, Pursue) 
        public static void InsertGrant(Grants g)
        {
            SqlCommand cmdInsertGrant = new SqlCommand();
            cmdInsertGrant.Connection = Lab2DBConnection;
            cmdInsertGrant.Connection.ConnectionString = Lab2DBConnString;
            cmdInsertGrant.CommandText = "INSERT INTO Grants (Title, Category, Type, Submission_Date, Award_Date, Deadline, AmountRequested, AmountAwarded, Status, SponsorOrg_ID, Pursue) " +
                                          "VALUES (@Title, @Category, @Type, @Submission_Date, @Award_Date, @Deadline, @AmountRequested, @AmountAwarded, @Status, @SponsorOrg_ID, @Pursue)";

            cmdInsertGrant.Parameters.AddWithValue("@Title", g.Title);
            cmdInsertGrant.Parameters.AddWithValue("@Category", g.Category);
            cmdInsertGrant.Parameters.AddWithValue("@Type", g.Category);
            cmdInsertGrant.Parameters.AddWithValue("@Submission_Date", g.Submission_Date ?? (object)DBNull.Value);
            cmdInsertGrant.Parameters.AddWithValue("@Award_Date", g.Award_Date ?? (object)DBNull.Value);
            cmdInsertGrant.Parameters.AddWithValue("@Deadline", g.Deadline);
            cmdInsertGrant.Parameters.AddWithValue("@AmountRequested", g.AmountRequested);
            cmdInsertGrant.Parameters.AddWithValue("@AmountAwarded", g.AmountAwarded ?? (object)DBNull.Value);
            cmdInsertGrant.Parameters.AddWithValue("@Status", g.Status);
            cmdInsertGrant.Parameters.AddWithValue("@SponsorOrg_ID", g.SponsorOrg_ID);
            cmdInsertGrant.Parameters.AddWithValue("@Pursue", g.Pursue);

            cmdInsertGrant.Connection.Open();
            cmdInsertGrant.ExecuteNonQuery();
        }

        // JMUCARE STATUS : complete
        // Used to be FacultyByGrant now its UsersByGrant, all refrence locations updated 
        public static SqlDataReader UsersByGrant(int Grant_id)
        {
            SqlCommand cmdUserRead = new SqlCommand();
            cmdUserRead.Connection = Lab2DBConnection;

            //having issues with connection state, asked chat to debug and was given this solution
            if (cmdUserRead.Connection.State == System.Data.ConnectionState.Closed)
            {
                cmdUserRead.Connection.ConnectionString = Lab2DBConnString;
                cmdUserRead.Connection.Open();
            }

            cmdUserRead.CommandText =
                "SELECT Users.User_ID, " +
                "       COALESCE(Users.LastName, '') AS LastName, " +
                "       COALESCE(Users.FirstName, '') AS FirstName, " +
                "       Users.Email " +
                "FROM Users " +
                "JOIN Role ON Users.Role_ID = Role.Role_ID " +
                "JOIN GrantUser ON Users.User_ID = GrantUser.User_ID " +
                "WHERE GrantUser.Grant_ID = @GrantID";

            cmdUserRead.Parameters.AddWithValue("@GrantID", Grant_id);

            return cmdUserRead.ExecuteReader();
        }

        // JMUCARE STATUS : complete
        //used to be AllFaculty now its AllJMUCAREUsers, all reference locations updated
        //no need to parameterize it doesn't have input
        public static SqlDataReader AllJMUCAREUsers()
        {
            SqlCommand cmdAllFacultyRead = new SqlCommand();
            cmdAllFacultyRead.Connection = Lab2DBConnection;
            cmdAllFacultyRead.Connection.ConnectionString = Lab2DBConnString;
            cmdAllFacultyRead.CommandText =
                "SELECT Users.User_ID, " +
                "       COALESCE(Users.LastName, '') AS LastName, " +
                "       COALESCE(Users.FirstName, '') AS FirstName, " +
                "       COALESCE(Users.Email, '') AS Email " +
                "FROM Users";
            cmdAllFacultyRead.Connection.Open();
            SqlDataReader tempReader = cmdAllFacultyRead.ExecuteReader();
            return tempReader;
        }

        // JMUCARE STATUS : complete
        // used to be FacultyExistsForGrant new name UserExistsForGrant, all refrence locations changed
        public static int UserExistsForGrant(int Grant_id, int User_id)
        {
            SqlCommand cmdDupeCheck = new SqlCommand();
            cmdDupeCheck.Connection = Lab2DBConnection;
            cmdDupeCheck.Connection.ConnectionString = Lab2DBConnString;

            cmdDupeCheck.CommandText = "SELECT COUNT(*) FROM GrantUser WHERE Grant_ID = @GrantID AND User_ID = @UserID";

            cmdDupeCheck.Parameters.AddWithValue("@GrantID", Grant_id);
            cmdDupeCheck.Parameters.AddWithValue("@UserID", User_id);

            cmdDupeCheck.Connection.Open();
            return (int)cmdDupeCheck.ExecuteScalar();
        }

        // JMUCARE STATUS : incomplete
        // used to be AddFacultyToGrant new name AddUserToGrant
        public static void AddUserToGrant(int User_id, int Grant_id)
        {
            SqlCommand cmdAdd = new SqlCommand();
            cmdAdd.Connection = Lab2DBConnection;
            cmdAdd.Connection.ConnectionString = Lab2DBConnString;

            cmdAdd.CommandText = "INSERT INTO GrantUser (User_ID, Grant_ID, perm_ID) VALUES (@UserID, @GrantID, 1)";
            cmdAdd.Parameters.AddWithValue("@UserID", User_id);
            cmdAdd.Parameters.AddWithValue("@GrantID", Grant_id);

            cmdAdd.Connection.Open();
            cmdAdd.ExecuteNonQuery();

        }

        // JMUCARE STATUS : complete
        // used to be RemoveFacultyFromGrant changed to RemoveUserFromGrant, all references changed
        public static void RemoveUserFromGrant(int Grant_id, int User_id)
        {
            SqlCommand cmdRemove = new SqlCommand();
            cmdRemove.Connection = Lab2DBConnection;
            cmdRemove.Connection.ConnectionString = Lab2DBConnString;
            cmdRemove.CommandText = "DELETE FROM GrantUser WHERE Grant_ID = @GrantID AND User_ID = @UserID";

            cmdRemove.Parameters.AddWithValue("@GrantID", Grant_id);
            cmdRemove.Parameters.AddWithValue("@UserID", User_id);

            cmdRemove.Connection.Open();
            cmdRemove.ExecuteNonQuery();
        }

        // JMUCARE STATUS : complete
        public static void EditGrant(Grants g)
        {
            SqlCommand cmdEditGrant = new SqlCommand();
            cmdEditGrant.Connection = Lab2DBConnection;
            cmdEditGrant.Connection.ConnectionString = Lab2DBConnString;

            // chat helped me make this pretty instead of messily on one line
            cmdEditGrant.CommandText =
                "UPDATE Grants SET " +
                "Title = @Title, " +
                "Category = @Category, " +
                "Type = @Type, " +
                "Submission_Date = @Submission_Date, " +
                "Award_Date = @Award_Date, " +
                "Deadline = @Deadline, " +
                "AmountRequested = @AmountRequested, " +
                "AmountAwarded = @AmountAwarded, " +
                "Status = @Status, " +
                "Pursue = @Pursue " +
                "WHERE Grant_ID = @Grant_ID";

            cmdEditGrant.Parameters.AddWithValue("@Title", g.Title ?? (object)DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Category", g.Category ?? (object)DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Type", g.Type);
            cmdEditGrant.Parameters.AddWithValue("@Submission_Date", g.Submission_Date.HasValue ? (object)g.Submission_Date.Value : DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Award_Date", g.Award_Date.HasValue ? (object)g.Award_Date.Value : DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Deadline", g.Deadline.HasValue ? (object)g.Deadline.Value : DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@AmountRequested", g.AmountRequested.HasValue ? (object)g.AmountRequested.Value : DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@AmountAwarded", g.AmountAwarded.HasValue ? (object)g.AmountAwarded.Value : DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Status", g.Status ?? (object)DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Pursue", g.Pursue ?? (object)DBNull.Value);
            cmdEditGrant.Parameters.AddWithValue("@Grant_ID", g.Grant_ID);

            cmdEditGrant.Connection.Open();
            cmdEditGrant.ExecuteNonQuery();

        }

        // JMUCARE STATUS : complete
        public static SqlCommand UpdateGrantStatus(int grantId, string status)
        {
            string sqlQuery = "UPDATE Grants SET Status = @Status WHERE Grant_ID = @GrantID";
            SqlCommand cmdUpdateStatus = new SqlCommand();
            cmdUpdateStatus.Connection = Lab2DBConnection;
            cmdUpdateStatus.Connection.ConnectionString = Lab2DBConnString;
            cmdUpdateStatus.CommandText = sqlQuery;

            cmdUpdateStatus.Parameters.AddWithValue("@Status", status);
            cmdUpdateStatus.Parameters.AddWithValue("@GrantID", grantId);

            cmdUpdateStatus.Connection.Open();
            cmdUpdateStatus.ExecuteNonQuery();
            return cmdUpdateStatus;
        }
        // END GRANTS








        //PROJECT DB CONNECTIONS - all updated

        // JMUCARE STATUS : complete
        public static SqlDataReader ProjectReader()
        {
            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = "SELECT * FROM Project";
            cmdProjectRead.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();

            return tempReader;
        }

        // JMUCARE STATUS : complete
        public static SqlDataReader SingleProjectReader(int Project_ID)
        {
            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.Close();
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = "SELECT * FROM Project WHERE Project_ID = " + Project_ID;
            cmdProjectRead.Connection.Open();

            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();
            return tempReader;
        }

        // JMUCARE STATUS : complete
        public static void UpdateProject(Projects p)
        {
            //chat helped update this method to make the formatting more consistent with my other db class and add parameters
            SqlCommand cmdUpdateProject = new SqlCommand();
            cmdUpdateProject.Connection = Lab2DBConnection;
            cmdUpdateProject.Connection.ConnectionString = Lab2DBConnString;
            cmdUpdateProject.CommandText = "UPDATE Project SET Title = @Title, StartDate = @StartDate, EndDate = @EndDate, Due_Date = @DueDate," +
                " Budget = @Budget, FundingSource = @FundingSource, LeadUser = @LeadUser, Description = @Description, Department = @Department, " +
                "Status = @Status WHERE Project_ID = @ProjectID";

            cmdUpdateProject.Parameters.AddWithValue("@Title", p.Title);
            cmdUpdateProject.Parameters.AddWithValue("@StartDate", p.StartDate);
            cmdUpdateProject.Parameters.AddWithValue("@EndDate", p.EndDate);
            cmdUpdateProject.Parameters.AddWithValue("@DueDate", p.Due_Date);
            cmdUpdateProject.Parameters.AddWithValue("@Budget", p.Budget);
            cmdUpdateProject.Parameters.AddWithValue("@FundingSource", p.FundingSource);
            cmdUpdateProject.Parameters.AddWithValue("@LeadUser", p.LeadUser);
            cmdUpdateProject.Parameters.AddWithValue("@Description", p.Description);
            cmdUpdateProject.Parameters.AddWithValue("@Department", p.Department);
            cmdUpdateProject.Parameters.AddWithValue("@Status", p.Status);
            cmdUpdateProject.Parameters.AddWithValue("@ProjectID", p.Project_ID);

            cmdUpdateProject.Connection.Open();
            cmdUpdateProject.ExecuteNonQuery();
        }


        // JMUCARE STATUS : complete
        public static void InsertProject(Projects p)
        {//chat helped with this method reformatting to be like my other ones 
            SqlCommand cmdInsertProject = new SqlCommand();
            cmdInsertProject.Connection = Lab2DBConnection;
            cmdInsertProject.Connection.ConnectionString = Lab2DBConnString;
            cmdInsertProject.CommandText = "INSERT INTO Project (Title, StartDate, EndDate, Due_Date, Budget, FundingSource, LeadUser, Description, Department, Status, Grant_ID) " +
                "VALUES (@Title, @StartDate, @EndDate, @DueDate, @Budget, @FundingSource, @LeadUser, @Description, @Department, @Status, @Grant_ID)";

            cmdInsertProject.Parameters.AddWithValue("@Title", p.Title);
            cmdInsertProject.Parameters.AddWithValue("@StartDate", p.StartDate);
            cmdInsertProject.Parameters.AddWithValue("@EndDate", p.EndDate);
            cmdInsertProject.Parameters.AddWithValue("@DueDate", p.Due_Date);
            cmdInsertProject.Parameters.AddWithValue("@Budget", p.Budget);
            cmdInsertProject.Parameters.AddWithValue("@FundingSource", p.FundingSource);
            cmdInsertProject.Parameters.AddWithValue("@LeadUser", p.LeadUser);
            cmdInsertProject.Parameters.AddWithValue("@Description", p.Description);
            cmdInsertProject.Parameters.AddWithValue("@Department", p.Department);
            cmdInsertProject.Parameters.AddWithValue("@Status", p.Status);
            cmdInsertProject.Parameters.AddWithValue("@Grant_ID", p.Grant_ID);


            cmdInsertProject.Connection.Open();
            cmdInsertProject.ExecuteNonQuery();
        }

        public static SqlDataReader GetProjectLead()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "SELECT User_ID, FirstName, LastName FROM Users";

            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static SqlDataReader AllGrantsReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "SELECT Grant_ID, Title FROM Grants";

            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        // JMUCARE STATUS : complete
        public static void ReadOneProject(Projects p)
        {
            //not gonna parameterize this bc no user input other than the button which gets the project id
            String sqlQuery = "SELECT " + p.Title + ", " + p.StartDate + ", " + p.EndDate + ", " + p.Due_Date + ", "
                + p.Budget + ", " + p.FundingSource + ", " + p.LeadUser + ", " + p.Description + ", " + p.Department + ", "
                + p.Status + ", " + " FROM Project WHERE Project_ID=" + p.Project_ID;

            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = sqlQuery;
            cmdProjectRead.Connection.Open();

            cmdProjectRead.ExecuteNonQuery();
        }

        public static SqlDataReader FilteredProjectReader(string? status, int? faculty, DateTime? date)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            string sqlQuery = "SELECT * FROM Project WHERE 1=1";

            if (!string.IsNullOrEmpty(status))
            {
                sqlQuery += " AND Status = @status";
                cmd.Parameters.AddWithValue("@status", status);
            }

            if (faculty.HasValue)
            {
                sqlQuery += " AND LeadUser = @faculty";
                cmd.Parameters.AddWithValue("@faculty", faculty.Value);
            }

            if (date.HasValue)
            {
                sqlQuery += " AND CAST(StartDate AS DATE) = @date";
                cmd.Parameters.AddWithValue("@date", date.Value.Date);
            }

            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();

            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static int CalculateProjectProgress(int projectId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            string sqlQuery = "SELECT Count(*) AS TotalTasks, SUM(CASE WHEN Status='Complete' THEN 1 ELSE 0 END) AS CompletedTasks FROM Task WHERE Project_ID = @ProjectID";
            cmd.Parameters.AddWithValue("@ProjectID", projectId);
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();

            SqlDataReader tempReader = cmd.ExecuteReader();

            //used chat to figure out this calculation
            int percent = 0;
            if (tempReader.Read())
            {
                int total = tempReader.GetInt32(0);
                int complete = tempReader.IsDBNull(1) ? 0 : tempReader.GetInt32(1);

                percent = total == 0 ? 0 : (int)((double)complete / total * 100);
            }
            DBClass.Lab2DBConnection.Close();
            return percent;
        }

        public static SqlCommand UpdateProjectStatus(int projectId, string status)
        {
            string sqlQuery = "UPDATE Project SET Status = @Status WHERE Project_ID = @ProjectID";
            SqlCommand cmdUpdateStatus = new SqlCommand();
            cmdUpdateStatus.Connection = Lab2DBConnection;
            cmdUpdateStatus.Connection.ConnectionString = Lab2DBConnString;
            cmdUpdateStatus.CommandText = sqlQuery;

            cmdUpdateStatus.Parameters.AddWithValue("@Status", status);
            cmdUpdateStatus.Parameters.AddWithValue("@ProjectID", projectId);

            cmdUpdateStatus.Connection.Open();
            cmdUpdateStatus.ExecuteNonQuery();
            return cmdUpdateStatus;
        }
        //END PROJECT

        //PROJECT NOTES DB CONNECTIONS
        // JMUCARE STATUS : complete
        public static SqlDataReader NotesReader()
        {//doesnt need parameterized, no user input in the form of freefrom text
            SqlCommand cmdNotesRead = new SqlCommand();
            cmdNotesRead.Connection = Lab2DBConnection;
            cmdNotesRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNotesRead.CommandText = "SELECT * FROM ProjectNote";
            cmdNotesRead.Connection.Open();

            SqlDataReader tempReader = cmdNotesRead.ExecuteReader();

            return tempReader;
        }
        // JMUCARE STATUS : complete
        public static SqlDataReader SingleNoteReader(int Project_ID)
        {//doesnt need parameterized, no user input in the form of freefrom text
            SqlCommand cmdNoteRead = new SqlCommand();
            cmdNoteRead.Connection = Lab2DBConnection;
            cmdNoteRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNoteRead.CommandText = "SELECT * FROM ProjectNote WHERE Project_ID = " + Project_ID;
            cmdNoteRead.Connection.Open();

            SqlDataReader tempReader = cmdNoteRead.ExecuteReader();
            return tempReader;
        }
        // JMUCARE STATUS : complete
        public static void ReadOneNote(ProjectNote n)
        {//doesnt need parameterized, no user input in the form of freefrom text
            String sqlQuery = "SELECT " + n.ProjectNote_ID + n.Timestamp + n.Content + "FROM ProjectNote WHERE Project_ID=" + n.Project_ID;

            SqlCommand cmdNoteRead = new SqlCommand();
            cmdNoteRead.Connection = Lab2DBConnection;
            cmdNoteRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNoteRead.CommandText = sqlQuery;
            cmdNoteRead.Connection.Open();

            cmdNoteRead.ExecuteNonQuery();
        }
        // JMUCARE STATUS : complete
        public static void InsertProjectNote(ProjectNote n)
        { //chat helped make this format consistent with my others
            SqlCommand cmdInsertNote = new SqlCommand();
            cmdInsertNote.Connection = Lab2DBConnection;
            cmdInsertNote.Connection.ConnectionString = Lab2DBConnString;
            cmdInsertNote.CommandText = "INSERT INTO ProjectNote (Project_ID, Timestamp, Content) VALUES (@ProjectID, @Timestamp, @Content)";

            cmdInsertNote.Parameters.AddWithValue("@ProjectID", n.Project_ID);
            cmdInsertNote.Parameters.AddWithValue("@Timestamp", n.Timestamp);
            cmdInsertNote.Parameters.AddWithValue("@Content", n.Content);

            cmdInsertNote.Connection.Open();
            cmdInsertNote.ExecuteNonQuery();
        }
        //END NOTE

        //DASHBOARD DB CONNECTIONS
        // JMUCARE STATUS : complete
        public static SqlDataReader ProjectDashboardReader(string userEmail)
        {
            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = "SELECT Project.Project_ID, Project.Title, Project.Status, Project.Due_Date " +
                "FROM Project " +
                "JOIN Users ON Project.LeadUser = Users.User_ID " +
                "WHERE LOWER(Users.Email) = LOWER(@Email)";

            cmdProjectRead.Parameters.AddWithValue("@Email", userEmail);

            cmdProjectRead.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();

            return tempReader;
        }
        //END DASHBOARD

        //TASK DB CONNECTIONS - Data class and all updated
        // JMUCARE STATUS : complete
        public static SqlDataReader SingleTaskReader(int Project_ID)
        {
            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = "SELECT * FROM Task " +
                                         "INNER JOIN Users ON Task.AssignedUser = Users.User_ID " +
                                         "INNER JOIN Project ON Task.Project_ID = Project.Project_ID " +
                                         "WHERE Task.Project_ID = " + Project_ID;
            cmdProjectRead.Connection.Open();

            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();
            return tempReader;
        }

        // JMUCARE STATUS : complete
        public static void InsertTask(Tasks t)
        {
            //chat helped me ensure consistent formatting with my other methods
            SqlCommand cmdInsertTask = new SqlCommand();
            cmdInsertTask.Connection = Lab2DBConnection;
            cmdInsertTask.Connection.ConnectionString = Lab2DBConnString;
            cmdInsertTask.CommandText = "INSERT INTO Task (Project_ID, AssignedUser, Title, Description, Priority, Status, Due_Date) " +
                                         "VALUES (@ProjectID, @AssignedUser, @Title, @Description, @Priority, @Status, @DueDate)";

            cmdInsertTask.Parameters.AddWithValue("@ProjectID", t.Project_ID);
            cmdInsertTask.Parameters.AddWithValue("@AssignedUser", t.AssignedUser);
            cmdInsertTask.Parameters.AddWithValue("@Title", t.Title);
            cmdInsertTask.Parameters.AddWithValue("@Description", t.Description);
            cmdInsertTask.Parameters.AddWithValue("@Priority", t.Priority);
            cmdInsertTask.Parameters.AddWithValue("@Status", t.Status);
            //Kept getting overflow error so used chat to help trace the error back to this
            cmdInsertTask.Parameters.AddWithValue("@DueDate",
                t.Due_Date.HasValue && t.Due_Date.Value > (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue
                ? (object)t.Due_Date.Value
        :       DBNull.Value);


            cmdInsertTask.Connection.Open();
            cmdInsertTask.ExecuteNonQuery();
        }

        // JMUCARE STATUS : complete
        public static SqlDataReader TaskReader(int projectid)
        {
            //no need to parameterize just a reader
            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = "SELECT * FROM Task " +
                                         "INNER JOIN Users ON Task.AssignedUser = Users.User_ID " +
                                         "INNER JOIN Project ON Task.Project_ID = Project.Project_ID " +
                                         "WHERE Task.Project_ID = " + projectid;
            cmdProjectRead.Connection.Open();

            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();

            return tempReader;
        }

        public static void UpdateTaskStatus(int taskId, string newStatus)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "UPDATE Task SET Status = @Status WHERE Task_ID = @TaskID";

            cmd.Parameters.AddWithValue("@Status", newStatus);
            cmd.Parameters.AddWithValue("@TaskID", taskId);

            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void UpdateTaskAssignment(int taskId, int assignedUserId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "UPDATE Task SET AssignedUser = @AssignedUser WHERE Task_ID = @TaskID";

            cmd.Parameters.AddWithValue("@AssignedUser", assignedUserId);
            cmd.Parameters.AddWithValue("@TaskID", taskId);

            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static SqlDataReader GetTaskNotes(int taskID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            //make sure connection is closed
            cmd.Connection.Close();
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "SELECT * FROM TaskNote WHERE Task_ID = @TaskID ORDER BY Timestamp DESC";

            cmd.Parameters.AddWithValue("@TaskID", taskID);

            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static void InsertTaskNote(int taskID, string content)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "INSERT INTO TaskNote (Content, Task_ID) VALUES (@Content, @Task_ID)";

            cmd.Parameters.AddWithValue("@Content", content);
            cmd.Parameters.AddWithValue("@Task_ID", taskID);

            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        // JMUCARE STATUS : complete
        public static SqlDataReader UsersReader()
        {
            //no need to parameterize just a reader
            SqlCommand cmdFacultyRead = new SqlCommand();
            cmdFacultyRead.Connection = Lab2DBConnection;
            cmdFacultyRead.Connection.ConnectionString = Lab2DBConnString;
            cmdFacultyRead.CommandText = "SELECT Users.User_ID, Users.FirstName, Users.LastName, Users.Role_ID, Users.Organization, Role.RoleType, Users.Email, Users.Status " +
                                         "FROM Users " +
                                         "JOIN Role ON Users.Role_ID = Role.Role_ID";
            cmdFacultyRead.Connection.Open();

            SqlDataReader tempReader = cmdFacultyRead.ExecuteReader();
            return tempReader;
        }


        //END TASKS

        //SPONSOR ORG DB CONNECTIONS
        //updated       
        public static void InsertNewOrg(SponsorOrgs s)
        {
            SqlCommand cmdInsertOrg = new SqlCommand();
            cmdInsertOrg.Connection = Lab2DBConnection;
            cmdInsertOrg.Connection.ConnectionString = Lab2DBConnString;

            cmdInsertOrg.CommandText = "INSERT INTO SponsorOrg (OrgName, Sector, Status_Flag) " +
                                       "VALUES (@OrgName, @Sector, @StatusFlag)";

            cmdInsertOrg.Parameters.AddWithValue("@OrgName", s.OrgName);
            cmdInsertOrg.Parameters.AddWithValue("@Sector", s.Sector);
            cmdInsertOrg.Parameters.AddWithValue("@StatusFlag", s.Status_Flag);

            cmdInsertOrg.Connection.Open();
            cmdInsertOrg.ExecuteNonQuery();
            cmdInsertOrg.Connection.Close();

            //question
            //int newSponsorOrgID = Convert.ToInt32(cmdOrgReader.ExecuteScalar());
        }

        //updated
        public static SqlDataReader OrgReader()
        {
            SqlCommand cmdOrgRead = new SqlCommand();
            cmdOrgRead.Connection = Lab2DBConnection;
            cmdOrgRead.Connection.ConnectionString = Lab2DBConnString;

            cmdOrgRead.CommandText =
                "SELECT SponsorOrg.*, Partner.User_ID, Users.Email " +
                "FROM SponsorOrg " +
                "LEFT JOIN Partner ON SponsorOrg.SponsorOrg_ID = Partner.SponsorOrg_ID " +
                "LEFT JOIN Users ON Partner.User_ID = Users.User_ID";


            cmdOrgRead.Connection.Open(); 

            SqlDataReader tempReader = cmdOrgRead.ExecuteReader();

            return tempReader;
        }

        //updated
        public static SqlDataReader SingleOrgReader(int SponsorOrg_ID)
        {
            SqlCommand cmdOrgRead = new SqlCommand();
            cmdOrgRead.Connection = Lab2DBConnection;
            cmdOrgRead.Connection.ConnectionString = Lab2DBConnString;
            cmdOrgRead.CommandText = "SELECT SponsorOrg.*, Partner.User_ID, Users.Email FROM SponsorOrg " +
                                     "LEFT JOIN Partner ON SponsorOrg.SponsorOrg_ID = Partner.SponsorOrg_ID " +
                                     "LEFT JOIN Users ON Partner.User_ID = Users.User_ID " +
                                     "WHERE SponsorOrg.SponsorOrg_ID = " + SponsorOrg_ID;
            cmdOrgRead.Connection.Open();

            SqlDataReader tempReader = cmdOrgRead.ExecuteReader();
            return tempReader;
        }
        //updated
        public static void UpdateOrg(SponsorOrgs s)
        {
            String updateOrgQuery = "UPDATE SponsorOrg SET OrgName='" + s.OrgName + "', ";
            updateOrgQuery += "Sector='" + s.Sector + "', ";
            updateOrgQuery += "Status_Flag='" + s.Status_Flag + "' ";
            updateOrgQuery += "WHERE SponsorOrg_ID = " + s.SponsorOrg_ID;
            SqlCommand cmdOrgUpdate = new SqlCommand(updateOrgQuery, Lab2DBConnection);
            cmdOrgUpdate.Connection.ConnectionString = Lab2DBConnString;
            cmdOrgUpdate.Connection.Open();
            cmdOrgUpdate.ExecuteNonQuery();
            cmdOrgUpdate.Connection.Close();
        }


        //END ORG


        //              MESSAGES - chat helped me change and update these
        //JMUCARE Status : Complete
        public static SqlDataReader GetAllUsersExcept(int currentUserId)
        {
            SqlCommand cmdUsers = new SqlCommand();
            cmdUsers.Connection = Lab2DBConnection;
            cmdUsers.Connection.ConnectionString = Lab2DBConnString;
            cmdUsers.CommandText = // chat helped
                "SELECT u.User_ID, u.FirstName, u.LastName, r.RoleType " +
                "FROM Users AS u " +
                "JOIN Role AS r ON u.Role_ID = r.Role_ID " + // Make sure you're joining Role table
                "WHERE u.User_ID != @CurrentUserId"; // Ensure no match for the current user
            cmdUsers.Parameters.AddWithValue("@CurrentUserId", currentUserId);

            cmdUsers.Connection.Open();
            return cmdUsers.ExecuteReader(); // Close in PageModel
        }

        //updated
        public static SqlDataReader GetMessagesBetweenUsers(int currentUserId, int otherUserId)
        {
            SqlCommand cmdMessages = new SqlCommand();
            cmdMessages.Connection = Lab2DBConnection;
            cmdMessages.Connection.ConnectionString = Lab2DBConnString;
            cmdMessages.CommandText = "SELECT m.Sender_ID, Users.FirstName + ' ' + Users.LastName AS SenderName, " +
                                      "m.Content, m.Timestamp " +
                                      "FROM Messages m " +
                                      "JOIN Users ON m.Sender_ID = Users.User_ID " +
                                      "WHERE (m.Sender_ID = @CurrentUserId AND m.Receiver_ID = @OtherUserId) " +
                                      "OR (m.Sender_ID = @OtherUserId AND m.Receiver_ID = @CurrentUserId) " +
                                      "ORDER BY m.Timestamp ASC";

            cmdMessages.Parameters.AddWithValue("@CurrentUserId", currentUserId);
            cmdMessages.Parameters.AddWithValue("@OtherUserId", otherUserId);

            cmdMessages.Connection.Open();
            return cmdMessages.ExecuteReader(); // Connection closed in PageModel
        }

        //updated
        public static void InsertMessage(int currentUserId, int otherUserId, string content)
        {
            SqlCommand cmdInsert = new SqlCommand();
            cmdInsert.Connection = Lab2DBConnection;
            cmdInsert.Connection.ConnectionString = Lab2DBConnString;
            cmdInsert.CommandText = "INSERT INTO Messages (Sender_ID, Receiver_ID, Content, Timestamp) VALUES (@CurrentUser, @OtherUser, @Content, @Timestamp)";

            cmdInsert.Parameters.AddWithValue("@CurrentUser", currentUserId);
            cmdInsert.Parameters.AddWithValue("@OtherUser", otherUserId);
            cmdInsert.Parameters.AddWithValue("@Content", content);
            cmdInsert.Parameters.AddWithValue("@Timestamp", DateTime.Now);

            cmdInsert.Connection.Open();
            cmdInsert.ExecuteNonQuery();
            // Do NOT close connection here! Will be closed in PageModel
        }

        //updated
        public static SqlDataReader GetUserNameById(int userId)
        {
            SqlCommand cmdUser = new SqlCommand();
            cmdUser.Connection = Lab2DBConnection;
            cmdUser.Connection.ConnectionString = Lab2DBConnString;
            cmdUser.CommandText = "SELECT FirstName + ' ' + LastName FROM Users WHERE User_ID= @UserId";
            cmdUser.Parameters.AddWithValue("@UserId", userId);

            cmdUser.Connection.Open();
            return cmdUser.ExecuteReader(); // Close in PageModel
        }


        // END MESSAGES


        // LOGIN & USERS

        //updated
        public static void CreateHashedUser(Users user, string Password)
        {
            string hashedPassword = PasswordHash.HashPassword(Password);

            // First insert into HashedCredentials in AUTH
            using (SqlConnection authConn = new SqlConnection(AuthConnString))
            {
                authConn.Open();
                SqlCommand cmdAuth = new SqlCommand(
                    "INSERT INTO HashedCredentials (Email, Password) VALUES (@Email, @Password)",
                    authConn
                );
                cmdAuth.Parameters.AddWithValue("@Email", user.Email);
                cmdAuth.Parameters.AddWithValue("@Password", hashedPassword);
                cmdAuth.ExecuteNonQuery();
            }

            // Then insert into Users in JMUCARE
            using (SqlConnection careConn = new SqlConnection(Lab2DBConnString))
            {
                careConn.Open();
                SqlCommand cmdUser = new SqlCommand(
                    "INSERT INTO Users (FirstName, LastName, Email, Status, Organization, Role_ID) " +
                    "VALUES (@FirstName, @LastName, @Email, @Status, @Organization, @RoleID)",
                    careConn
                );

                cmdUser.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmdUser.Parameters.AddWithValue("@LastName", user.LastName);
                cmdUser.Parameters.AddWithValue("@Email", user.Email);
                cmdUser.Parameters.AddWithValue("@Status", user.Status);
                cmdUser.Parameters.AddWithValue("@Organization", string.IsNullOrEmpty(user.Organization) ? DBNull.Value : user.Organization);
                cmdUser.Parameters.AddWithValue("@RoleID", user.Role_ID);

                cmdUser.ExecuteNonQuery();
            }
        }


        public static SqlDataReader SingleUserReader(string email)
            //used copilot to troubleshoot and it changed the format of the query
        {
            SqlCommand cmdUserRead = new SqlCommand();
            cmdUserRead.Connection = Lab2DBConnection;
            cmdUserRead.Connection.ConnectionString = Lab2DBConnString;
            cmdUserRead.CommandText = @"
                SELECT u.Email, u.FirstName, u.LastName, u.Status, u.Organization, u.Role_ID, r.RoleType
                FROM Users u
                JOIN Role r ON u.Role_ID = r.Role_ID
                WHERE u.Email = @Email";
            cmdUserRead.Parameters.AddWithValue("@Email", email);
            cmdUserRead.Connection.Open();

            SqlDataReader userReader = cmdUserRead.ExecuteReader();
            return userReader;
        }



        public static void UpdateUser(Users user)
        {
            SqlCommand cmdUpdateUser = new SqlCommand();
            cmdUpdateUser.Connection = Lab2DBConnection;
            cmdUpdateUser.Connection.ConnectionString = Lab2DBConnString;
            cmdUpdateUser.CommandText = "UPDATE Users SET FirstName = @FirstName, LastName = @LastName, Status = @Status, Organization = @Organization, Role_ID = @RoleID WHERE Email = @Email";

            cmdUpdateUser.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmdUpdateUser.Parameters.AddWithValue("@LastName", user.LastName);
            cmdUpdateUser.Parameters.AddWithValue("@Status", user.Status);
            cmdUpdateUser.Parameters.AddWithValue("@Organization", user.Organization);
            cmdUpdateUser.Parameters.AddWithValue("@RoleID", user.Role_ID);
            cmdUpdateUser.Parameters.AddWithValue("@Email", user.Email);

            cmdUpdateUser.Connection.Open();
            cmdUpdateUser.ExecuteNonQuery();
        }


        public static int SecureLogin(string Email, string Password)
        {
            string loginQuery = "SELECT Email, Password FROM HashedCredentials WHERE Email = @Email ";
            //Used Copilot to figure out why it wasn't getting past the query. I just had tweak my query slightly

            SqlCommand cmdLogin = new SqlCommand();
            cmdLogin.Connection = AuthDBConnection;
            cmdLogin.Connection.ConnectionString = AuthConnString;

            cmdLogin.CommandText = loginQuery;
            cmdLogin.Parameters.AddWithValue("@Email", Email);
            cmdLogin.Parameters.AddWithValue("@Password", Password);

            cmdLogin.Connection.Open();

            // ExecuteScalar() returns back data type Object
            // Use a typecast to convert this to an int.
            // Method returns first column of first row.
            SqlDataReader hashReader = cmdLogin.ExecuteReader();
            if (hashReader.Read())
            {
                string correctHash = hashReader["Password"].ToString();

                if (PasswordHash.ValidatePassword(Password, correctHash))
                {
                    return 1;
                }
            }
            return 0;
        }

        // JMUCARE Status : complete
        public static SqlDataReader GetUserDetailsByEmail(string email)
        {
            SqlCommand cmdUser = new SqlCommand();
            cmdUser.Connection = Lab2DBConnection;
            cmdUser.Connection.ConnectionString = Lab2DBConnString;
            cmdUser.CommandText =
                "SELECT u.User_id, u.FirstName, u.LastName, u.Email, Role.RoleType " +
                "FROM Users AS u " +
                "JOIN Role ON u.Role_ID = Role.Role_ID " +
                "WHERE u.Email = @Email";
            cmdUser.Parameters.AddWithValue("@Email", email);

            cmdUser.Connection.Open();
            return cmdUser.ExecuteReader(); // Caller must close connection after reading

        }


        // END LOGIN & User


        // GRANT NOTES 

        // JMUCARE status : complete no change
        public static SqlDataReader GrantNotesReader()
        {
            SqlCommand cmdNotesRead = new SqlCommand();
            cmdNotesRead.Connection = Lab2DBConnection;
            cmdNotesRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNotesRead.CommandText = "SELECT * FROM GrantNote";
            cmdNotesRead.Connection.Open();

            SqlDataReader tempReader = cmdNotesRead.ExecuteReader();

            return tempReader;
        }

        // JMUCARE status : complete
        public static SqlDataReader SingleGrantNoteReader(int Grant_id)
        {
            SqlCommand cmdNoteRead = new SqlCommand();
            cmdNoteRead.Connection = Lab2DBConnection;
            cmdNoteRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNoteRead.CommandText = "SELECT * FROM GrantNote WHERE Grant_ID = " + Grant_id;
            cmdNoteRead.Connection.Open();

            SqlDataReader tempReader = cmdNoteRead.ExecuteReader();
            return tempReader;
        }

        // JMUCARE status : complete
        public static void ReadOneGrantNote(GrantNotes g)
        {
            String sqlQuery = "SELECT " + g.GrantNote_ID + g.Timestamp + g.Content + "FROM GrantNote WHERE Grant_ID=" + g.Grant_ID;

            SqlCommand cmdNoteRead = new SqlCommand();
            cmdNoteRead.Connection = Lab2DBConnection;
            cmdNoteRead.Connection.ConnectionString = Lab2DBConnString;
            cmdNoteRead.CommandText = sqlQuery;
            cmdNoteRead.Connection.Open();

            cmdNoteRead.ExecuteNonQuery();
        }

        // JMUCARE status : complete
        public static void InsertGrantNote(GrantNotes g)
        {
            String sqlQuery = "INSERT INTO GrantNote (Grant_ID, Timestamp, Content) VALUES('";
            sqlQuery += g.Grant_ID + "', '";
            sqlQuery += g.Timestamp + "', '";
            sqlQuery += g.Content + "')";

            SqlCommand cmdNoteReader = new SqlCommand();
            cmdNoteReader.Connection = Lab2DBConnection;
            cmdNoteReader.Connection.ConnectionString = Lab2DBConnString;
            cmdNoteReader.CommandText = sqlQuery;
            cmdNoteReader.Connection.Open();

            cmdNoteReader.ExecuteNonQuery();
        }
        //END NOTE


        // MY DASHBOARD / HOME

        public static SqlDataReader GetImportantDatesForCurrentMonth()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            // chat helped me modify this so it only shows events from the current month
            string sqlQuery = @"
                SELECT Date, DateType, Description
                FROM (
                    SELECT Submission_Date AS Date, 'Submission Date' AS DateType, CONCAT('Grant: ', Title) AS Description
                    FROM Grants
                    WHERE Submission_Date IS NOT NULL

                    UNION ALL

                    SELECT Award_Date AS Date, 'Award Date' AS DateType, CONCAT('Grant: ', Title) AS Description
                    FROM Grants
                    WHERE Award_Date IS NOT NULL

                    UNION ALL

                    SELECT Deadline AS Date, 'Deadline' AS DateType, CONCAT('Grant: ', Title) AS Description
                    FROM Grants
                    WHERE Deadline IS NOT NULL

                    UNION ALL

                    SELECT StartDate AS Date, 'Start Date' AS DateType, CONCAT('Project: ', Title) AS Description
                    FROM Project
                    WHERE StartDate IS NOT NULL

                    UNION ALL

                    SELECT EndDate AS Date, 'End Date' AS DateType, CONCAT('Project: ', Title) AS Description
                    FROM Project
                    WHERE EndDate IS NOT NULL

                    UNION ALL

                    SELECT Due_Date AS Date, 'Due Date' AS DateType, CONCAT('Project: ', Title) AS Description
                    FROM Project
                    WHERE Due_Date IS NOT NULL
                ) AS CombinedDates
                WHERE MONTH(Date) = MONTH(GETDATE()) AND YEAR(Date) = YEAR(GETDATE())
                ORDER BY Date ASC;
            ";

            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static SqlDataReader GetUserTaskProgress(int userId)
        { //chat and I got in arguments about this query, it helped some, I disagreed some, this is where we ended up
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            cmd.CommandText = @"
                SELECT 
                    User_ID, 
                    UserName, 
                    CAST((1.0 * CompletedTasks / NULLIF(TotalTasks, 0)) * 100 AS DECIMAL(5,2)) AS PercentComplete
                FROM (
                    SELECT 
                        u.User_ID,
                        u.FirstName + ' ' + u.LastName AS UserName,
                        COUNT(t.Task_ID) AS TotalTasks,
                        SUM(CASE WHEN t.Status = 'Complete' THEN 1 ELSE 0 END) AS CompletedTasks
                    FROM Task t
                    JOIN Users u ON u.User_ID = t.AssignedUser
                    WHERE t.AssignedUser = @UserID
                    GROUP BY u.User_ID, u.FirstName, u.LastName
                ) AS TaskSummary;
            ";

            cmd.Parameters.AddWithValue("@UserID", userId);

            Lab2DBConnection.Open();
            return cmd.ExecuteReader();
        }


        public static SqlDataReader TaskReaderByUser(int userID) //chat helped make edits and implement my query
        {
            SqlCommand cmdUserTaskRead = new SqlCommand();
            cmdUserTaskRead.Connection = Lab2DBConnection;
            cmdUserTaskRead.Connection.ConnectionString = Lab2DBConnString;
            cmdUserTaskRead.CommandText = "SELECT t.Title, t.Description, p.Title AS ProjectTitle, " +
                                          "t.Priority, t.Status, t.Date_Assigned, t.Due_Date " +
                                          "FROM Task AS t " +
                                          "INNER JOIN Users AS u ON u.User_ID = t.AssignedUser " +
                                          "INNER JOIN Project AS p ON t.Project_ID = p.Project_ID " +
                                          "WHERE u.User_ID = " + userID;

            cmdUserTaskRead.Connection.Open();

            SqlDataReader tempReader = cmdUserTaskRead.ExecuteReader();

            return tempReader;
        }

        public static SqlDataReader GrantsByUser(int userId)
        { //chat helped some with the query 
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            cmd.CommandText = @"
                SELECT g.Grant_ID, g.Title, g.Status
                FROM GrantUser gu
                JOIN Grants g ON gu.Grant_ID = g.Grant_ID
                WHERE gu.User_ID = @UserID";

            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Connection.Open();
            return cmd.ExecuteReader();
        }


        ///  END MY DASHBOARD



        //pretty sure this is good?
        public static SqlDataReader GeneralReaderQuery(string sqlQuery)
        {

            SqlCommand cmdProjectRead = new SqlCommand();
            cmdProjectRead.Connection = Lab2DBConnection;
            cmdProjectRead.Connection.ConnectionString = Lab2DBConnString;
            cmdProjectRead.CommandText = sqlQuery;
            cmdProjectRead.Connection.Open();
            SqlDataReader tempReader = cmdProjectRead.ExecuteReader();

            return tempReader;

        }

       
        public static SqlDataReader StoredProcedureLogin(string Email)
        {
            Lab2DBConnection.ConnectionString = AuthConnString;
            Lab2DBConnection.Open();
            SqlCommand cmdLogin = new SqlCommand();
            cmdLogin.Connection = Lab2DBConnection;
            cmdLogin.CommandText = "sp_Lab3Login";
            cmdLogin.CommandType = System.Data.CommandType.StoredProcedure;
            cmdLogin.Parameters.AddWithValue("@Email", Email);
            return cmdLogin.ExecuteReader();
        }

        public static SqlDataReader FilteredGrantsReader(string? Status, string? Pursue,double? MinAmount, double? MaxAmount,DateTime? AwardStartDate, DateTime? AwardEndDate,DateTime? DeadlineStartDate, DateTime? DeadlineEndDate,int? userId, string role)
        {
            SqlConnection conn = new SqlConnection(Lab2DBConnString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;

            string sqlQuery = "SELECT * FROM Grants WHERE 1=1";

            if (!string.IsNullOrEmpty(Status))
            {
                sqlQuery += " AND Status = @Status";
                cmd.Parameters.AddWithValue("@Status", Status);
            }

            if (!string.IsNullOrEmpty(Pursue))
            {
                sqlQuery += " AND Pursue = @Pursue";
                cmd.Parameters.AddWithValue("@Pursue", Pursue);
            }

            if (MinAmount.HasValue)
            {
                sqlQuery += " AND AmountAwarded >= @MinAmount";
                cmd.Parameters.AddWithValue("@MinAmount", MinAmount.Value);
            }

            if (MaxAmount.HasValue)
            {
                sqlQuery += " AND AmountAwarded <= @MaxAmount";
                cmd.Parameters.AddWithValue("@MaxAmount", MaxAmount.Value);
            }

            if (AwardStartDate.HasValue)
            {
                sqlQuery += " AND Award_Date >= @AwardStartDate";
                cmd.Parameters.AddWithValue("@AwardStartDate", AwardStartDate.Value);
            }

            if (AwardEndDate.HasValue)
            {
                sqlQuery += " AND Award_Date <= @AwardEndDate";
                cmd.Parameters.AddWithValue("@AwardEndDate", AwardEndDate.Value);
            }

            if (DeadlineStartDate.HasValue)
            {
                sqlQuery += " AND Deadline >= @DeadlineStartDate";
                cmd.Parameters.AddWithValue("@DeadlineStartDate", DeadlineStartDate.Value);
            }

            if (DeadlineEndDate.HasValue)
            {
                sqlQuery += " AND Deadline <= @DeadlineEndDate";
                cmd.Parameters.AddWithValue("@DeadlineEndDate", DeadlineEndDate.Value);
            }

            // Chat helped with this Query in order to make sure users only see grants they are assigned to unless Admin or Center Director
            if (role != "Center Director" && role != "Admin Staff" && userId.HasValue)
            {
                sqlQuery += " AND Grant_ID IN (SELECT Grant_ID FROM GrantUser WHERE User_ID = @UserId)";
                cmd.Parameters.AddWithValue("@UserId", userId.Value);
            }

            cmd.CommandText = sqlQuery;
            conn.Open();
            return cmd.ExecuteReader();
        }




        public static SqlDataReader GetSponsorOrgs()
        {
            SqlCommand cmd = new SqlCommand("SELECT SponsorOrg_ID, OrgName FROM SponsorOrg", Lab2DBConnection);
            Lab2DBConnection.Open();
            return cmd.ExecuteReader();
        }

        // Executes the SearchData stored procedure 
        // Chat helped understand the use of (object)? for non string values 
        public static SqlDataReader RunSearchProcedure(DateTime? start, DateTime? end, string projectStatus, string grantStatus, string faculty, string partner)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "SearchData";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@StartDate", (object)start ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object)end ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProjectStatus", projectStatus);
            cmd.Parameters.AddWithValue("@GrantStatus", grantStatus);
            cmd.Parameters.AddWithValue("@FacultyName", faculty);
            cmd.Parameters.AddWithValue("@ExternalPartnerName", partner);

            cmd.Connection.Open();
            return cmd.ExecuteReader();
        }
        // PARTNER PORTAL
        public static SqlDataReader GetPartnerPortalInfo(string email)
        {
            SqlCommand cmdPartnerRead = new SqlCommand();
            cmdPartnerRead.Connection = Lab2DBConnection;
            cmdPartnerRead.Connection.ConnectionString = Lab2DBConnString;

            cmdPartnerRead.CommandText =
                "SELECT so.OrgName, so.Status_Flag " +
                "FROM Users u " +
                "JOIN Partner p ON u.User_ID = p.User_ID " +
                "JOIN SponsorOrg so ON p.SponsorOrg_ID = so.SponsorOrg_ID " +
                "WHERE u.Email = @Email";

            cmdPartnerRead.Parameters.AddWithValue("@Email", email);

            cmdPartnerRead.Connection.Open();
            SqlDataReader tempReader = cmdPartnerRead.ExecuteReader();
            return tempReader;
        }
        public static void SaveFileUpload(string fileName, string note, int? projectId, int? grantId, int sponsorOrgId, string fileType, int userId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.CommandText = "INSERT INTO FileUploads (FileName, Note, Project_ID, Grant_ID, SponsorOrg_ID, FileType, User_ID) " +
							  "VALUES (@FileName, @Note, @Project_ID, @Grant_ID, @SponsorOrg_ID, @FileType, @User_ID)";

            cmd.Parameters.AddWithValue("@FileName", fileName);
			cmd.Parameters.AddWithValue("@FileType", (object)fileType ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@Note", note ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Project_ID", (object?)projectId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Grant_ID", (object?)grantId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SponsorOrg_ID", (object?)sponsorOrgId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@User_ID", userId);


            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
        }
        public static SqlDataReader GetFilesUploadedByUser(int userId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            cmd.CommandText = @"
                SELECT FileName, UploadDate, Note, FileType
                FROM FileUploads
                WHERE User_ID = @UserID
                ORDER BY UploadDate DESC";

            cmd.Parameters.AddWithValue("@UserID", userId);

            cmd.Connection.Open();
            return cmd.ExecuteReader();
        }

        public static SqlDataReader GetUploadsForSponsorOrg(int sponsorOrgId)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = Lab2DBConnection;
			cmd.Connection.ConnectionString = Lab2DBConnString;

			cmd.CommandText = @"
                SELECT FileName, UploadDate, Note, FileType 
                FROM FileUploads 
                WHERE SponsorOrg_ID = @SponsorOrg_ID";

			cmd.Parameters.AddWithValue("@SponsorOrg_ID", sponsorOrgId);

			cmd.Connection.Open();
			return cmd.ExecuteReader();
		}


		public static SqlDataReader GetSponsorOrgIdByEmail(string email)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            cmd.CommandText = "SELECT SponsorOrg.SponsorOrg_ID " +
                              "FROM SponsorOrg " +
                              "JOIN Partner ON SponsorOrg.SponsorOrg_ID = Partner.SponsorOrg_ID " +
                              "JOIN Users ON Partner.User_ID = Users.User_ID " +
                              "WHERE Users.Email = @Email";

            cmd.Parameters.AddWithValue("@Email", email);

            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            return reader; 
        }

        public static SqlDataReader GrantGraphReader(DateTime? startDate, DateTime? endDate)
        {
            SqlConnection conn = new SqlConnection(Lab2DBConnString);
            conn.Open();

            string query = @"
        SELECT Award_Date, AmountAwarded 
        FROM Grants 
        WHERE Award_Date BETWEEN @StartDate AND @EndDate 
        AND AmountAwarded IS NOT NULL";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

            DBClass.Lab2DBConnection = conn; 
            return cmd.ExecuteReader();
        }
        public static SqlDataReader GetLastUploadDateReader(int sponsorOrgId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;

            cmd.CommandText = @"
                SELECT MAX(UploadDate) AS LastUpload
                FROM FileUploads
                WHERE SponsorOrg_ID = @SponsorOrg_ID";

            cmd.Parameters.AddWithValue("@SponsorOrg_ID", sponsorOrgId);
            cmd.Connection.Open();
            return cmd.ExecuteReader();
        }

        //users and permissions
        //chat was used in this section for general troubleshooting and debugging but the logic is all mine
        public static SqlDataReader GetUserProjects(int userId)
        {
            SqlCommand cmdUserProjects = new SqlCommand();
            cmdUserProjects.Connection = Lab2DBConnection;
            cmdUserProjects.Connection.ConnectionString = Lab2DBConnString;
            cmdUserProjects.CommandText =
                "SELECT Project.Project_ID, Project.Title, Permissions.permission, ProjectUser.perm_ID " +
                "FROM ProjectUser " +
                "JOIN Project ON ProjectUser.Project_ID = Project.Project_ID " +
                "JOIN Permissions ON ProjectUser.perm_ID = Permissions.perm_ID " +
                "WHERE ProjectUser.User_ID = @UserID";
            cmdUserProjects.Parameters.AddWithValue("@UserID", userId);

            cmdUserProjects.Connection.Open();
            SqlDataReader tempReader = cmdUserProjects.ExecuteReader();
            return tempReader;
        }



        public static SqlDataReader GetUserGrants(int userId)
        {
            SqlCommand cmdUserGrants = new SqlCommand();
            cmdUserGrants.Connection = Lab2DBConnection;
            cmdUserGrants.Connection.ConnectionString = Lab2DBConnString;
            cmdUserGrants.CommandText =
                "SELECT Grants.Grant_ID, Grants.Title, Permissions.permission, GrantUser.perm_ID " +
                "FROM GrantUser " +
                "JOIN Grants ON GrantUser.Grant_ID = Grants.Grant_ID " +
                "JOIN Permissions ON GrantUser.perm_ID = Permissions.perm_ID " +
                "WHERE GrantUser.User_ID = @UserID";

            cmdUserGrants.Parameters.AddWithValue("@UserID", userId);

            cmdUserGrants.Connection.Open();
            SqlDataReader tempReader = cmdUserGrants.ExecuteReader();
            return tempReader;
        }


        public void UpdateProjectPermission(int permId, int UserID, int Project_ID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.Connection.Open();

            cmd.CommandText = "UPDATE ProjectUser SET perm_ID = @PermID WHERE User_ID = @UserID AND Project_ID = @ProjectID";

            cmd.Parameters.AddWithValue("@PermID", permId);
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@ProjectID", Project_ID);


            cmd.ExecuteNonQuery();
          
        }


        public void UpdateGrantPermission(int permId, int UserID, int Grant_ID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Lab2DBConnection;
            cmd.Connection.ConnectionString = Lab2DBConnString;
            cmd.Connection.Open();


            cmd.CommandText = "UPDATE GrantUser SET perm_ID = @PermID WHERE User_ID = @UserID AND Grant_ID = @GrantID";

            cmd.Parameters.AddWithValue("@PermID", permId);
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@GrantID", Grant_ID);

            cmd.ExecuteNonQuery();
         
        }

		public static SqlDataReader GetAllProjectPermissionsForUser(int userId)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = Lab2DBConnection;
			cmd.Connection.ConnectionString = Lab2DBConnString;

			cmd.CommandText = @"
        SELECT Project_ID, perm_ID 
        FROM ProjectUser 
        WHERE User_ID = @UserID";

			cmd.Parameters.AddWithValue("@UserID", userId);

			cmd.Connection.Open();
			SqlDataReader tempReader = cmd.ExecuteReader();
			return tempReader;
		}


        public static SqlDataReader GetAllGrantPermissionsForUser(int userId)
        {
            SqlCommand cmd = new SqlCommand();

            // Check if connection is closed before assigning connection string
            if (Lab2DBConnection.State != ConnectionState.Closed)
            {
                Lab2DBConnection.Close();
            }

            Lab2DBConnection.ConnectionString = Lab2DBConnString;
            cmd.Connection = Lab2DBConnection;

            cmd.CommandText = @"
                SELECT Grant_ID, perm_ID 
                FROM GrantUser 
                WHERE User_ID = @UserID";

            cmd.Parameters.AddWithValue("@UserID", userId);

            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static SqlDataReader GetPermissionForGrantCommand(int UserID, int GrantID)
		{
			SqlCommand cmd = new SqlCommand();

			cmd.Connection = Lab2DBConnection;
			cmd.Connection.ConnectionString = Lab2DBConnString;
			cmd.CommandText = @"
        SELECT perm_ID 
        FROM GrantUser 
        WHERE User_ID = @UserID AND Grant_ID = @GrantID";

			cmd.Parameters.AddWithValue("@UserID", UserID);
			cmd.Parameters.AddWithValue("@GrantID", GrantID);

			cmd.Connection.Open(); 
			SqlDataReader tempReader = cmd.ExecuteReader(); 
			return tempReader;
		}

		public static SqlDataReader GetPermissionForProjCommand(int UserID, int ProjectID)
		{
			SqlCommand cmd = new SqlCommand();

			cmd.Connection = Lab2DBConnection;
			cmd.Connection.ConnectionString = Lab2DBConnString;
			cmd.CommandText = @"
        SELECT perm_ID 
        FROM ProjectUser 
        WHERE User_ID = @UserID AND Project_ID = @ProjectID";

			cmd.Parameters.AddWithValue("@UserID", UserID);
			cmd.Parameters.AddWithValue("@ProjectID", ProjectID);

			cmd.Connection.Open();
			SqlDataReader tempReader = cmd.ExecuteReader();
			return tempReader;
		}


	}
}
