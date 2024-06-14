using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrickBreaker
{
    internal static class LevelLoader
    {
        private static string FilePath { get => @"..\..\..\levelData.json"; }
        private static List<Brick[,]> Levels { get; set; } = [];
        public static int SelectedLevel { get; set; } = 0;
        public static int LevelCount { get => Levels.Count; }
        public static Brick[,] GetLevel()
        {
            Brick[,] lvlCopy = new Brick[Levels[SelectedLevel].GetLength(0), Levels[SelectedLevel].GetLength(1)];

            for (int i = 0; i < Levels[SelectedLevel].GetLength(0); i++)
            {
                for (int j = 0; j < Levels[SelectedLevel].GetLength(1); j++)
                    lvlCopy[i, j] = Levels[SelectedLevel][i, j];
            }
            return lvlCopy;
        }
        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    int[][][]? levelArray = JsonSerializer.Deserialize<int[][][]>(File.ReadAllText(FilePath));
                    if (levelArray != null)
                    {
                        foreach (int[][] level in levelArray)
                        {
                            Brick[,] bricks = new Brick[10, 20];
                            for (int i = 0; i < 20; i++)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    int item = level[j][i];
                                    if (item == 0)
                                        bricks[j, i] = null;
                                    else if (item == 10)
                                        bricks[j, i] = new UndestroyableBrick(new Vector2D(i, j));
                                    else
                                        bricks[j, i] = new Brick(item, new Vector2D(i, j));
                                }
                            }
                            Levels.Add(bricks);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while loading levels: " + ex.Message);
            }
        }
    }
}