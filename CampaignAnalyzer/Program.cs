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
			List<Campaign> campaigns = ImportCampaignsFromFile();
			List<Placement> placements = ImportPlacementsFromFile();

			if(campaigns.Count == 0 || placements.Count == 0)
			{
				Console.WriteLine("Data files are invalid. Please check format and retry");
			}
			else
			{
				Console.WriteLine("Getting Estimated cumulative clicks for each Placement at day 2, 4, 6");
				foreach(var placement in placements)
				{
					Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 2: {GetEstimatedClicksForDay(placement, 2)}");
					Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 4: {GetEstimatedClicksForDay(placement, 4)}");
					Console.WriteLine($"PlacementID: {placement.id}, Estimated cumulative clicks for day 6: {GetEstimatedClicksForDay(placement, 6)}");
				}
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

		#endregion
	}
}
