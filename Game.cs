using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrickBreaker
{
    internal class Game
    {
        public static Dispatcher UiDispatcher { get; private set; } = Dispatcher.CurrentDispatcher;
        public static Queue<Upgrade> UpgradeQueue { get; set; } = new();
        private Direction MoveDirection { get; set; }
        private GameState PlayState { get; set; }
        private Paddle GamePaddle { get; }
        private List<Upgrade> Upgrades { get; set; }
        private List<Ball> Balls { get; set; }
        private Brick[,] Bricks { get; set; }
        private List<Brick> EdgeBricks { get; set; }
        private Canvas GameCanvas { get; }
        private Stopwatch ElapsedTime { get; }
        private double LastTime { get; set; }
        private Arrow arrow { get; set; }
        private TextBlock TextElement { get; set; }
        public static int DestroyableBricksCount { get; set; }

        public Game(Canvas gameCanvas, Brick[,] brickLayout)
        {
            GameCanvas = gameCanvas;
            ElapsedTime = new Stopwatch();
            LastTime = 0;
            UiDispatcher = Dispatcher.CurrentDispatcher;
            PlayState = GameState.Playing;
            arrow = new();
            Upgrades = new();

            GamePaddle = new();
            Canvas.SetLeft(GamePaddle.rectangle, GamePaddle.Position.X);
            Canvas.SetTop(GamePaddle.rectangle, GamePaddle.Position.Y);
            gameCanvas.Children.Add(GamePaddle.rectangle);

            TextElement = new TextBlock
            {
                Text = "Move to begin.",
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(GameCanvas.Width / 4, GameCanvas.Height / 4, 0, 0)
            };

            Balls = [];

            Bricks = brickLayout;
            EdgeBricks = [];
            DestroyableBricksCount = 0;
            for (int i = 0; i < Bricks.GetLength(0); i++)
            {
                for (int j = 0; j < Bricks.GetLength(1); j++)
                {
                    Brick brick = Bricks[i, j];
                    if (brick == null)
                        continue;

                    if (brick is not UndestroyableBrick)
                        DestroyableBricksCount++;

                    Canvas.SetTop(brick.rectangle, brick.Top);
                    Canvas.SetLeft(brick.rectangle, brick.Left);
                    GameCanvas.Children.Add(brick.rectangle);

                    if (Brick.IsEdgeBrick(Bricks, i, j) && !EdgeBricks.Contains(brick))
                        EdgeBricks.Add(brick);
                }
            }
        }

        public void Start()
        {
            Statistics.GamesPlayed++;
            ElapsedTime.Start();
            Task.Run(() => ListenForUserInput());
            Task.Run(() =>
            {
                StartArrow();
                while (PlayState == GameState.Playing)
                {
                    Step();
                    Task.Delay(5).Wait();
                }

                Statistics.TimePlayed += ElapsedTime.Elapsed.TotalSeconds;
                if (PlayState == GameState.Lost)
                    HandleLoose();
                else if (PlayState == GameState.Won)
                    HandleWin();
                End();
            });
        }
        private void Step()
        {
            double deltaTime = (ElapsedTime.Elapsed.TotalSeconds - LastTime);
            LastTime = ElapsedTime.Elapsed.TotalSeconds;

            UpdatePaddle();
            UpdateBalls(deltaTime);
            UpdateUpgrades(deltaTime);

            ApplyUpgrades();

            CheckGameEndConditions();
        }
        private void ApplyUpgrades()
        {
            if (UpgradeQueue.Count == 0)
                return;
            UpgradeQueue = new Queue<Upgrade>(new SortedSet<Upgrade>(UpgradeQueue));
            while (UpgradeQueue.Count > 0 && UpgradeQueue.Peek().ExecutionTime <= ElapsedTime.Elapsed.TotalSeconds)
                UpgradeQueue.Dequeue().Execute();
        }
        private void End()
        {
            Statistics.Save();
            Task.Delay(750).Wait();
            UiDispatcher.Invoke(() =>
            {
                GameCanvas.Children.Add(new TextBlock
                {
                    Text = "Press any key to continue...",
                    FontSize = 25,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(GameCanvas.Width / 4, GameCanvas.Height / 3, 0, 0)
                });
            });
            bool keyPressed = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.KeyDown += (sender, e) =>
                {
                    keyPressed = true;
                };
            });
            while (!keyPressed)
                Task.Delay(5).Wait();
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameCanvas.Children.Clear();
                ((MainWindow)Application.Current.MainWindow).GoBack();
            });
        }
        private void StartArrow()
        {
            UiDispatcher.Invoke(() =>
            {
                Canvas.SetTop(arrow.arrow, GameCanvas.Height / 2);
                Canvas.SetLeft(arrow.arrow, GameCanvas.Width / 2);
                GameCanvas.Children.Add(arrow.arrow);
                GameCanvas.Children.Add(TextElement);
            });
            bool keyPressed = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Left || e.Key == Key.Right)
                        keyPressed = true;
                };
            });

            while (!keyPressed)
            {
                double deltaTime = (ElapsedTime.Elapsed.TotalSeconds - LastTime);
                LastTime = ElapsedTime.Elapsed.TotalSeconds;
                arrow.Rotate(Math.Min(deltaTime, 0.05));
                Task.Delay(5).Wait();
            }

            UiDispatcher.Invoke(() =>
            {
                GameCanvas.Children.Remove(TextElement);
                GameCanvas.Children.Remove(arrow.arrow);
                Balls.Add(new Ball(new Vector2D(GameCanvas.Width / 2, GameCanvas.Height / 2), Vector2D.FromAngle(arrow.Angle + 90)));
                GameCanvas.Children.Add(Balls[0].ellipse);
            });
        }
        private void HandleLoose()
        {
            Statistics.GamesLost++;
            UiDispatcher.Invoke(() =>
            {
                TextElement.Text = "Game over.";
                GameCanvas.Children.Add(TextElement);
            });
        }
        private void HandleWin()
        {
            Statistics.GamesWon++;
            UpgradeQueue.Clear();
            UiDispatcher.Invoke(() =>
            {
                TextElement.Text = "You won";
                GameCanvas.Children.Add(TextElement);
            });
        }
        private void ListenForUserInput()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Left)
                        MoveDirection = Direction.Left;
                    if (e.Key == Key.Right)
                        MoveDirection = Direction.Right;
                };

                Application.Current.MainWindow.KeyUp += (sender, e) =>
                {
                    if (e.Key == Key.Left || e.Key == Key.Right)
                        MoveDirection = Direction.None;
                };
            });
        }
        private void UpdateBalls(double deltaTime)
        {
            List<int> removedBallsIndex = new List<int>();
            List<Brick> removedBricks = new List<Brick>();

            for (int i = Balls.Count - 1; i >= 0; i--)
            {
                Ball ball = Balls[i];
                (bool isAlive, Brick? removedBrick) = ball.Move(Math.Min(deltaTime, 0.05), EdgeBricks.ToArray(), GamePaddle, GameCanvas, Upgrades, ElapsedTime.Elapsed.TotalSeconds, Balls);
                if (!isAlive)
                    removedBallsIndex.Add(i);
                if (removedBrick != null)
                    removedBricks.Add(removedBrick);

                UiDispatcher.Invoke(() =>
                {
                    Canvas.SetTop(ball.ellipse, ball.Position.Y);
                    Canvas.SetLeft(ball.ellipse, ball.Position.X);
                });
            }
            if (removedBallsIndex.Count > 0)
            {
                foreach (int ballIndex in removedBallsIndex)
                {
                    UiDispatcher.Invoke(() => GameCanvas.Children.Remove(Balls[ballIndex].ellipse));
                    Balls.RemoveAt(ballIndex);
                }
            }
            if (removedBricks.Count > 0)
            {
                foreach (Brick brick in removedBricks)
                {
                    UiDispatcher.Invoke(() => GameCanvas.Children.Remove(brick.rectangle));
                    int x = (int)brick.position.X;
                    int y = (int)brick.position.Y;
                    EdgeBricks.Remove(Bricks[y, x]);
                    Bricks[y, x] = null;
                    UpdateEdgeBricks(y, x);
                }
            }
        }
        private void UpdatePaddle()
        {
            double xPaddle = GamePaddle.Position.X;
            double dxPaddle = GamePaddle.Move(MoveDirection);
            if (Math.Abs(dxPaddle - xPaddle) > 0.0)
                UiDispatcher.Invoke(() => Canvas.SetLeft(GamePaddle.rectangle, dxPaddle));

        }
        private void UpdateUpgrades(double deltaTime)
        {
            if (Upgrades.Count == 0)
                return;
            List<Upgrade> removedUpgrades = [];
            foreach (Upgrade upgrade in Upgrades)
            {
                if (!upgrade.Move(deltaTime))
                    removedUpgrades.Add(upgrade);
                else
                {
                    //if ((upgrade.Position.Y + Upgrade.Diameter <= GamePaddle.Position.Y) && (upgrade.Position.Y >= GamePaddle.Position.Y + GamePaddle.Size.Y))
                    if (upgrade.Position.X <= GamePaddle.Position.X + GamePaddle.Size.X && upgrade.Position.X + Upgrade.Diameter >= GamePaddle.Position.X)
                    {
                        if (upgrade.Position.Y + Upgrade.Diameter >= GamePaddle.Position.Y && upgrade.Position.Y <= GamePaddle.Position.Y + GamePaddle.Size.Y)
                        {
                            Statistics.UpgradesCollected++;
                            upgrade.Execute();
                            removedUpgrades.Add(upgrade);
                            UiDispatcher.Invoke(() => GameCanvas.Children.Remove(upgrade.ellipse));
                        }
                    }
                }
            }
            if (removedUpgrades.Count > 0)
            {
                foreach (Upgrade upgrade in removedUpgrades)
                {
                    UiDispatcher.Invoke(() => GameCanvas.Children.Remove(upgrade.ellipse));
                    Upgrades.Remove(upgrade);
                }
            }
        }
        private void UpdateEdgeBricks(int y, int x)
        {
            if (y > 0 && Bricks[y - 1, x] != null && !EdgeBricks.Contains(Bricks[y - 1, x]))
                EdgeBricks.Add(Bricks[y - 1, x]);
            if (y < Bricks.GetLength(0) - 1 && Bricks[y + 1, x] != null && !EdgeBricks.Contains(Bricks[y + 1, x]))
                EdgeBricks.Add(Bricks[y + 1, x]);
            if (x > 0 && Bricks[y, x - 1] != null && !EdgeBricks.Contains(Bricks[y, x - 1]))
                EdgeBricks.Add(Bricks[y, x - 1]);
            if (x < Bricks.GetLength(1) - 1 && Bricks[y, x + 1] != null && !EdgeBricks.Contains(Bricks[y, x + 1]))
                EdgeBricks.Add(Bricks[y, x + 1]);
        }
        private void CheckGameEndConditions()
        {
            if (Balls.Count == 0)
                PlayState = GameState.Lost;

            else if (DestroyableBricksCount == 0)
                PlayState = GameState.Won;
        }
    }
}