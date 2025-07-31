namespace Lab2.Pages.DataClasses
{
    public class GrantNotes //do we not have grant notes anymore? we def dont have the sql for it
    {
        public int Grant_ID { get; set; }
        public int GrantNote_ID { get; set; }
        public DateTime? Timestamp { get; set; }
        public String? Content { get; set; }
    }
}
