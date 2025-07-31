namespace Lab2.Pages.DataClasses
{
    public class ProjectNote
    {
        public int ProjectNote_ID { get; set; }
        public DateTime? Timestamp { get; set; }
        public String? Content { get; set; }
        public int Project_ID { get; set; }
    }
}
