namespace Lab2.Pages.DataClasses
{
    public class FileUpload
    {
        public int Upload_ID { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string Note { get; set; }

        public int? Project_ID { get; set; }
        public int? Grant_ID { get; set; }
        public int? SponsorOrg_ID { get; set; }
		public string FileType { get; set; }

	}
}
