using CampaignAnalyzer.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CampaignAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			// Goal 1
			List<Campaign> campaigns = ImportCampaignsFromFile();
			List<Placement> placements = ImportPlacementsFromFile();

			if(campaigns.Count == 0 || placements.Count == 0)
			{
				Console.WriteLine("Data files are invalid. Please check format and retry");
			}
			else
			{
				// Goal 2
				Goal2(placements);
				Console.WriteLine("");

				// Goal 3
				Goal3(campaigns, placements);
				Console.WriteLine("");







			}

			Console.WriteLine("Press any key to exit");
			Console.ReadLine();
		}
		
		#region Helper Methods

		public static List<Campaign> ImportCampaignsFromFile()
		{
			Console.WriteLine("Reading Campaigns");
			List<Campaign> campaigns = new List<Campaign>();
			using (StreamReader file = new StreamReader(@"c:\testdata\campaigns.json"))
			{
				string currentLine;
				while ((currentLine = file.ReadLine()) != null)
				{
					campaigns.Add(JsonConvert.DeserializeObject<Campaign>(currentLine));
				}
			}
			return campaigns;
		}

		public static List<Placement> ImportPlacementsFromFile()
		{
			Console.WriteLine("Reading Placements");
			List<Placement> placements = new List<Placement>();
			using (StreamReader file = new StreamReader(@"c:\testdata\placements.json"))
			{
				string currentLine;
				while ((currentLine = file.ReadLine()) != null)
				{
					placements.Add(JsonConvert.DeserializeObject<Placement>(currentLine));
				}
			}
			return placements;
		}
		
		public static decimal? GetEstimatedClicksForDay(Placement placement, int day)
		{
			decimal clicks = placement.clickThroughDay1 * day;

			if (day == 0)
			{
				Console.WriteLine("Error: Days start at index 1, please enter a number greater than 0");
				return null;
			}
			else if (day < 7 && clicks >= placement.clickThroughDay7)
			{
				// If day 1-6 and clicks exceed day 7 then we've overestimated and need to return the max for that range
				return placement.clickThroughDay7;
			}
			else if (day == 7)
			{
				return placement.clickThroughDay7;
			}
			else
			{
				return clicks;
			}
		}

		public static void Goal2(List<Placement> placements)
		{
			Console.WriteLine("Getting Estimated cumulative clicks for each Placement at day 2, 4, 6");
			foreach (var placement in placements)
			{
				Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 2: {GetEstimatedClicksForDay(placement, 2)}");
				Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 4: {GetEstimatedClicksForDay(placement, 4)}");
				Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 6: {GetEstimatedClicksForDay(placement, 6)}");
			}
		}

		public static void Goal3(List<Campaign> campaigns, List<Placement> placements)
		{
			int placementCounter = 0;
			foreach(var campaign in campaigns)
			{
				if(placementCounter >= placements.Count)
				{
					placementCounter = 0;
				}

				// Make sure we get a placement that's in cost budget
				decimal? cost = GetEstimatedClicksForDay(placements[placementCounter], campaign.lengthInDays) * placements[placementCounter].costPerClick;
				int startingPlacementCounter = placementCounter;
				while(cost == null || cost > campaign.budget)
				{
					placementCounter++;
					if(placementCounter == startingPlacementCounter)
					{
						//Easiest way to end this right here
						throw new Exception($"Could not find a campaign and placement match. Everything for Campaign {campaign.name} is too expensive");
					}
					else if (placementCounter >= placements.Count)
					{
						placementCounter = 0;
					}
					cost = GetEstimatedClicksForDay(placements[placementCounter], campaign.lengthInDays) * placements[placementCounter].costPerClick;
				}

				//Calculate and output
				decimal? totalConversions = GetEstimatedClicksForDay(placements[placementCounter], campaign.lengthInDays) * placements[placementCounter].conversionsPerClick;
				Console.WriteLine($"Using PlacementID: {placements[placementCounter].id} for Campaign: {campaign.name}.");
				Console.WriteLine($"Daily cost is {cost / campaign.lengthInDays}.");
				Console.WriteLine($"Total conversions: {totalConversions}");
				Console.WriteLine($"Cost per conversion: {GetEstimatedClicksForDay(placements[placementCounter], campaign.lengthInDays) / totalConversions}");
				Console.WriteLine($"Remaining Budget is {campaign.budget - cost}.{Environment.NewLine}");
				placementCounter++;
			}

		}

		#endregion
	}
}
