using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace BrickBreaker
{
    internal static class Statistics
    {
        public static double TimePlayed { get; set; }
        public static int BrickDestroyed { get; set; }
        public static int GamesPlayed { get; set; }
        public static int GamesWon { get; set; }
        public static int GamesLost { get; set; }
        public static int UpgradesCollected { get; set; }
        private static string FilePath { get => "statistics.json"; }
        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    StatisticsBlueprint? stats = JsonSerializer.Deserialize<StatisticsBlueprint>(File.ReadAllText(FilePath));
                    TimePlayed = stats.TimePlayed;
                    BrickDestroyed = stats.BrickDestroyed;
                    GamesPlayed = stats.GamesPlayed;
                    GamesWon = stats.GamesWon;
                    GamesLost = stats.GamesLost;
                    UpgradesCollected = stats.UpgradesCollected;
                }
                else
                    New();
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error occurred while saving statistics: " + ex.Message);
                New();
            }
        }
        private static void New()
        {
            TimePlayed = 0;
            BrickDestroyed = 0;
            GamesPlayed = 0;
            GamesWon = 0;
            GamesLost = 0;
            UpgradesCollected = 0;
            Save();
        }
        public static void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(new { TimePlayed, BrickDestroyed, GamesPlayed, GamesWon, GamesLost, UpgradesCollected }));
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error occurred while saving statistics: " + ex.Message);
            }
        }
    }
    internal class StatisticsBlueprint
    {
        public double TimePlayed { get; set; }
        public int BrickDestroyed { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int UpgradesCollected { get; set; }

        public StatisticsBlueprint(double timePlayed, int brickDestroyed, int gamesPlayed, int gamesWon, int gamesLost, int upgradesCollected)
        {
            TimePlayed = timePlayed;
            BrickDestroyed = brickDestroyed;
            GamesPlayed = gamesPlayed;
            GamesWon = gamesWon;
            GamesLost = gamesLost;
            UpgradesCollected = upgradesCollected;
        }
    }
}