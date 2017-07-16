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

		#endregion
	}
}
