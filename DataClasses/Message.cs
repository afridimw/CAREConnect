namespace Lab2.Pages.DataClasses
{
    public class Message
    {
        public int Message_ID { get; set; }
        public int Sender_ID { get; set; }
        public int Receiver_ID { get; set; }
        public String? Content { get; set; }
        public DateTime Timestamp { get; set; }
        

        // Nullable SenderName, since it's only used for display purposes
        public string? SenderName { get; set; }
    }
}
