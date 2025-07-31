using Lab2.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;



namespace Lab2.Pages.Login
{
    public class ParameterizedLoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnGet(string? logout = null) // Make 'logout' nullable, chat suggested this
        {
            if (!string.IsNullOrEmpty(logout) && logout == "true")
            {
                HttpContext.Session.Clear();
                ViewData["LoginMessage"] = "Successfully Logged Out!";
            }
            return Page();

        }

        public IActionResult OnPost() // chat gpt changed the strucutre i had to fix and error with the login error message
        {
            bool userExists = false;
            int userId = 0;
            string userRole = "";

            SqlDataReader hashedreader = DBClass.StoredProcedureLogin(Email);
            if (hashedreader.Read())
            {
                string storedHash = hashedreader["Password"].ToString();

                if (PasswordHash.ValidatePassword(Password, storedHash))
                {
                    userExists = true;
                }
            }
            DBClass.Lab2DBConnection.Close();

            if (userExists)
            {
                HttpContext.Session.Clear();

                // Fetch user details 
                SqlDataReader userReader = DBClass.GetUserDetailsByEmail(Email);

                if (userReader.Read())
                {
                    userId = Convert.ToInt32(userReader["User_id"]);
                    userRole = userReader["RoleType"].ToString();

                    HttpContext.Session.SetInt32("UserID", userId);
                    HttpContext.Session.SetString("FirstName", userReader["FirstName"].ToString());
                    HttpContext.Session.SetString("LastName", userReader["LastName"].ToString());
                    HttpContext.Session.SetString("Email", userReader["Email"].ToString());
                    HttpContext.Session.SetString("RoleType", userRole);
                    HttpContext.Session.SetString("Role", userRole);
                }
                userReader.Close();
                DBClass.Lab2DBConnection.Close();

                // Admin Staff / Center Director Grant Matching
                if (userRole == "Admin Staff" || userRole == "Center Director")
                {
                    int? matchedUserId = null;
                    List<int> grantIds = new List<int>();

                    using (SqlDataReader grantsReader = DBClass.GrantsReader())
                    {
                        while (grantsReader.Read())
                        {
                            grantIds.Add(Convert.ToInt32(grantsReader["Grant_id"]));
                        }
                    }

                    DBClass.Lab2DBConnection.Close();

                    foreach (int grantId in grantIds)
                    {
                        SqlDataReader facultyReader = DBClass.UsersByGrant(grantId);
                        while (facultyReader.Read())
                        {
                            int tempUserID = Convert.ToInt32(facultyReader["User_id"]);
                            if (tempUserID == userId)
                            {
                                matchedUserId = tempUserID;
                                HttpContext.Session.SetInt32("UserID", matchedUserId.Value);
                                break;
                            }
                        }
                        facultyReader.Close();
                        if (matchedUserId.HasValue)
                        {
                            break;
                        }
                    }
                   DBClass.Lab2DBConnection.Close();
                }

                // Show success (optional)
                ViewData["LoginMessage"] = "Login Successful!";
                return RedirectToPage("/Index"); // redirect to dashboard
            }
            else
            {
                // Login failed
                ViewData["LoginError"] = "Email and/or Password Incorrect";
                DBClass.Lab2DBConnection.Close();
                return Page(); // stay on login page
            }
        }

    }
}