namespace Lab2.Pages.DataClasses
{
    public class Grants
    {
        public int Grant_ID { get; set; }
        public String? Title { get; set; }
        public String? Category { get; set; }
        public String? Type { get; set; }
        public DateTime? Submission_Date { get; set; }
        public DateTime? Award_Date { get; set; }
        public DateTime? Deadline {  get; set; }
        public double? AmountRequested { get; set; }
        public double? AmountAwarded { get; set; }
        public String? Status { get; set; }
        public int? SponsorOrg_ID { get; set; }
        public String? Pursue { get; set; }
    }
}
