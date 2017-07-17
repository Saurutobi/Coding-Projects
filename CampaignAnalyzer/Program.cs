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

				// Goal 3b
				Goal3b(campaigns, placements);
				Console.WriteLine("");
			}

			Console.WriteLine("Press any key to exit");
			Console.ReadLine();
		}

		#region Goals

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
			foreach (var campaign in campaigns)
			{
				if (placementCounter >= placements.Count)
				{
					placementCounter = 0;
				}

				// Make sure we get a placement that's in cost budget
				decimal? cost = GetEstimatedClicksForDay(placements[placementCounter], campaign.lengthInDays) * placements[placementCounter].costPerClick;
				int startingPlacementCounter = placementCounter;
				while (cost == null || cost > campaign.budget)
				{
					placementCounter++;
					if (placementCounter == startingPlacementCounter)
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

		public static void Goal3b(List<Campaign> campaigns, List<Placement> placements)
		{
			foreach (var campaign in campaigns)
			{
				Console.WriteLine($"On Campaign: {campaign.name}");
				decimal? currentBudgetPerDay = campaign.budget / campaign.lengthInDays;
				decimal? totalBudgetRemaining = campaign.budget;
				decimal? actualConversion = 0;
				int placementCounter = 0;
				bool insufficientFunds = false;

				// day starts at 1 because clicksforday start at 1
				for (int day = 1; day <= campaign.lengthInDays; day++)
				{
					Console.WriteLine($"Day {day}:");
					// Cycle through
					// Start at day 1 each time multi-day campaign might be a little too complex for now
					decimal? dailyCostForPlacement = placements[placementCounter].clickThroughDay1 * placements[placementCounter].costPerClick;
					int startingPlacementCounter = placementCounter;
					while ((dailyCostForPlacement == null || dailyCostForPlacement > currentBudgetPerDay) && !insufficientFunds)
					{
						placementCounter++;
						if (placementCounter == startingPlacementCounter)
						{
							if (day == campaign.lengthInDays)
							{
								// Last day of campaign
								dailyCostForPlacement = totalBudgetRemaining;
								Console.WriteLine("Insufficient funds remain for last day, spending it all.");
								insufficientFunds = true;
								break;
							}
							// Easiest way to end this right here, campaign failed early
							throw new Exception($"Could not find a placement match for the daily budget. Everything for Campaign {campaign.name} is too expensive.");
						}
						else if (placementCounter >= placements.Count)
						{
							placementCounter = 0;
						}
						if (!insufficientFunds)
						{
							dailyCostForPlacement = placements[placementCounter].clickThroughDay1 * placements[placementCounter].costPerClick;
						}
					}
					totalBudgetRemaining = totalBudgetRemaining - dailyCostForPlacement;

					// Either get a portion of clicks based on $$ and thus conversion, or get the full day's conversion
					actualConversion += insufficientFunds ? dailyCostForPlacement / placements[placementCounter].costPerClick * placements[placementCounter].conversionsPerClick : placements[placementCounter].clickThroughDay1 * placements[placementCounter].conversionsPerClick;

					Console.WriteLine($"Using PlacementID: {placements[placementCounter].id}. Cost is {dailyCostForPlacement}");
					Console.WriteLine($"Total budget remaining: {totalBudgetRemaining}");

					// Adjust daily budget based on days remaining and cost used for current day, unless it's the last day
					if (campaign.lengthInDays != day)
					{
						currentBudgetPerDay = totalBudgetRemaining / (campaign.lengthInDays - day);
					}

					placementCounter = placementCounter + 1 >= placements.Count ? 0 : placementCounter + 1;
				}

				// Check Conversion Goal
				if (campaign.conversionGoal < actualConversion)
				{
					Console.WriteLine($"Campaign goal of {campaign.conversionGoal} conversion surpassed. Actual conversion: {actualConversion}");
				}
				else
				{
					Console.WriteLine($"Campaign goal of {campaign.conversionGoal} conversion failed. Actual conversion: {actualConversion}. {Environment.NewLine}");
				}
			}
		}

		#endregion

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
