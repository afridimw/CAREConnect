namespace Lab2.Pages.DataClasses
{
    public class Projects
    {
      
        public int Project_ID { get; set; }
        public String? Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Due_Date { get; set; }
        public Decimal Budget { get; set; }
        public String? FundingSource { get; set; }
        public int LeadUser { get; set; }
        public String? Description { get; set; }
        public String? Department { get; set; }
        public String? Status { get; set; }
        public int Grant_ID { get; set; }

    }
}
