namespace Lab2.Pages.DataClasses
{
    public class TaskNote
    {
        public int TaskNote_ID { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? Content { get; set; }
        public int Task_ID {get; set;}
    }
}
