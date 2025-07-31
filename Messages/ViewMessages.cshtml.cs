using Lab2.Pages.DB;
using Lab2.Pages.DataClasses;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Lab2.Pages.Messages
{
    //chat helped a lot with errors I was having on this model, some additional logic was added using chat, we had a lot of trouble with the sender reciever ids ended up settling on an "other user" and "logged in user"
    public class ViewMessagesModel : PageModel
    {
        public List<Message> Messages { get; set; }

        public int LoggedInUserId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }

        [BindProperty]
        public string NewMessageContent { get; set; }

        public ViewMessagesModel()
        {
            Messages = new List<Message>();
        }

        public IActionResult OnGet(int otherUserId)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Email") == null)
            {
                HttpContext.Session.SetString("LoginError", "You must log in to access this page!");
                return RedirectToPage("/Login/ParameterizedLogin"); // Redirect to login page
            }

            // Retrieve logged-in user ID from session
            if (HttpContext.Session.GetInt32("UserID") != null)
            {
                LoggedInUserId = (int)HttpContext.Session.GetInt32("UserID");
            }
            else
            {
                Response.Redirect("/Login/ParameterizedLogin");
            }

            // Store OtherUserId in session
            HttpContext.Session.SetInt32("OtherUserId", otherUserId);
            OtherUserId = otherUserId;


            // Retrieve Other User's Name
            SqlDataReader userReader = DBClass.GetUserNameById(OtherUserId);
            if (userReader.Read())
            {
                OtherUserName = userReader[0].ToString();
            }
            DBClass.Lab2DBConnection.Close();

            // Retrieve messages between LoggedInUserId and ReceiverId
            SqlDataReader messageReader = DBClass.GetMessagesBetweenUsers(LoggedInUserId, OtherUserId);
            while (messageReader.Read())
            {
                Messages.Add(new Message
                {
                    Sender_ID = Convert.ToInt32(messageReader["Sender_id"]),
                    Timestamp = DateTime.Parse(messageReader["Timestamp"].ToString()),
                    Content = messageReader["Content"].ToString(),
                    SenderName = messageReader["SenderName"].ToString()
                });
            }
            DBClass.Lab2DBConnection.Close();

            return Page();
        }

        public IActionResult OnPost()
        {
            // Ensure user is logged in
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToPage("/Login/ParameterizedLogin");
            }

            LoggedInUserId = (int)HttpContext.Session.GetInt32("UserID");

            // Retrieve OtherUserId from session if it's not passed in the request
            OtherUserId = HttpContext.Session.GetInt32("OtherUserId") ?? 0;

            if (OtherUserId == 0)
            {
                Console.WriteLine("Error: OtherUserId is missing or invalid.");
                return Page();
            }


            // Insert new message
            if (!string.IsNullOrWhiteSpace(NewMessageContent))
            {
                DBClass.InsertMessage(LoggedInUserId, OtherUserId, NewMessageContent);
                DBClass.Lab2DBConnection.Close();
            }
            

            return RedirectToPage("ViewMessages", new { otherUserId = OtherUserId });
        }
    }
}
