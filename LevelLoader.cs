using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrickBreaker
{
    internal static class LevelLoader
    {
        private static string FilePath { get => "levelData.json"; }
        public static List<Brick[,]> Levels { get; private set; } = new();
        public static int SelectedLevel { get; set; } = 0;
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
                                int j = 0;
                                foreach (int[] column in level)
                                {
                                    if (j >= column.Length)
                                        break;

                                    int item = column[j];
                                    if (item == 0)
                                        bricks[j, i] = null;
                                    else if (item == 10)
                                        bricks[j, i] = new UndestroyableBrick(new Vector2D(i, j));
                                    else
                                        bricks[j, i] = new Brick(item, new Vector2D(i, j));
                                    j++;
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