namespace CampaignAnalyzer.DataModels
{
	public class Placement
	{
		public string id { get; set; }
		public decimal costPerClick { get; set; }
		public decimal clickThroughDay1 { get; set; }
		public decimal clickThroughDay7 { get; set; }
		public decimal conversionsPerClick { get; set; }
	}
}
