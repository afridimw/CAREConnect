namespace Lab2.Pages.DataClasses
{
    public class Users
    {
        public int User_ID { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string Organization { get; set; }
        public int Role_ID { get; set; }
        public string RoleType { get; set; } // this will hold RoleType from the Role table

    }
}
