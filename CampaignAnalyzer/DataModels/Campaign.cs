namespace CampaignAnalyzer.DataModels
{
	public class Campaign
	{
		public string name { get; set; }
		public decimal budget { get; set; }
		public int conversionGoal { get; set; }
		public int lengthInDays { get; set; }
	}
}
