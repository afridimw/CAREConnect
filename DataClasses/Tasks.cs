namespace Lab2.Pages.DataClasses
{
    public class Tasks
    {
        
        public int Task_ID { get; set; }
        public int AssignedUser { get; set; }
        public String? Title { get; set; }
        public String? Description { get; set; }
        public String? Priority { get; set; }
        public String? Status { get; set; }
        public DateTime? Due_Date { get; set; }
        public int Project_ID { get; set; }
        public String? ProjectTitle { get; set; }
    }

}

