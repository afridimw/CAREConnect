namespace Lab2.Pages.DataClasses
{
    public class SponsorOrgs
    {
        public int SponsorOrg_ID { get; set; }
        public String? OrgName { get; set; }
		public String? Sector { get; set; }
		public String? Status_Flag { get; set; }
        public bool IsInactiveByUpload { get; set; }

    }
}
