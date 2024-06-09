using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
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
        private List<Ball> Balls { get; set; }
        private Brick[,] Bricks { get; set; }
        private List<Brick> EdgeBricks { get; set; }
        private Canvas GameCanvas { get; }
        private Stopwatch ElapsedTime { get; }
        private double LastTime { get; set; }
        private Arrow arrow { get; set; }
        private TextBlock TextElement { get; set; }

        public Game(Canvas gameCanvas, Brick[,] brickLayout)
        {
            GameCanvas = gameCanvas;
            ElapsedTime = new Stopwatch();
            LastTime = 0;
            UiDispatcher = Dispatcher.CurrentDispatcher;
            PlayState = GameState.Playing;
            arrow = new();

            GamePaddle = new();
            Canvas.SetLeft(GamePaddle.rectangle, GamePaddle.Position.X);
            Canvas.SetTop(GamePaddle.rectangle, GamePaddle.Position.Y);
            gameCanvas.Children.Add(GamePaddle.rectangle);

            TextElement = new TextBlock
            {
                Text = "Move to begin.",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(GameCanvas.Width / 4, GameCanvas.Height / 4, 0, 0)
            };

            Balls = [];

            Bricks = brickLayout;
            EdgeBricks = [];
            for (int i = 0; i < Bricks.GetLength(0); i++)
            {
                for (int j = 0; j < Bricks.GetLength(1); j++)
                {
                    Brick brick = Bricks[i, j];
                    if (brick == null)
                        continue;
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
                while (PlayState == GameState.Playing)
                {
                    Step();
                    Task.Delay(5).Wait();
                }
                Statistics.TimePlayed += ElapsedTime.Elapsed.TotalSeconds;
                if (PlayState == GameState.Lost)
                {
                    Statistics.GamesLost++;
                    UiDispatcher.Invoke(() =>
                    {
                        TextElement.Text = "Game over.";
                        GameCanvas.Children.Add(TextElement);
                    });
                }
                else if (PlayState == GameState.Won)
                {
                    Statistics.GamesWon++;
                    UpgradeQueue.Clear();
                    UiDispatcher.Invoke(() =>
                    {
                        TextElement.Text = "You won";
                        GameCanvas.Children.Add(TextElement);
                    });
                }
                Statistics.Save();
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

        private void UpdateUiBall(Ball ball)
        {
            UiDispatcher.Invoke(() =>
            {
                Canvas.SetTop(ball.ellipse, ball.Position.Y);
                Canvas.SetLeft(ball.ellipse, ball.Position.X);
            });
        }
        private void UpdateUiPaddle(double position)
        {
            UiDispatcher.Invoke(() =>
            {
                Canvas.SetLeft(GamePaddle.rectangle, position);
            });
        }
        private bool Step()
        {
            List<int> removedBallsIndex = new List<int>();
            List<Brick> removedBricks = new List<Brick>();

            double deltaTime = (ElapsedTime.Elapsed.TotalSeconds - LastTime);
            LastTime = ElapsedTime.Elapsed.TotalSeconds;

            double xPaddle = GamePaddle.Position.X;
            double dxPaddle = GamePaddle.Move(MoveDirection);
            if (Math.Abs(dxPaddle - xPaddle) > 0.0)
                UpdateUiPaddle(dxPaddle);

            for (int i = Balls.Count - 1; i >= 0; i--)
            {
                Ball ball = Balls[i];
                (bool isAlive, Brick? removedBrick) = ball.Move(Math.Min(deltaTime, 0.05), EdgeBricks.ToArray(), GamePaddle);
                if (!isAlive)
                    removedBallsIndex.Add(i);
                if (removedBrick != null)
                    removedBricks.Add(removedBrick);

                UpdateUiBall(ball);
            }

            if (removedBallsIndex.Count > 0)
            {
                foreach (int ballIndex in removedBallsIndex)
                {
                    UiDispatcher.Invoke(() =>
                    {
                        GameCanvas.Children.Remove(Balls[ballIndex].ellipse);
                    });
                    Balls.RemoveAt(ballIndex);
                }
            }
            if (removedBricks.Count > 0)
            {
                foreach (Brick brick in removedBricks)
                {
                    UiDispatcher.Invoke(() =>
                    {
                        GameCanvas.Children.Remove(brick.rectangle);
                    });
                    int x = (int)brick.position.X;
                    int y = (int)brick.position.Y;
                    EdgeBricks.Remove(Bricks[y, x]);
                    Bricks[y, x] = null;
                    if (y > 0 && Bricks[y - 1, x] != null && !EdgeBricks.Contains(Bricks[y - 1, x]))
                        EdgeBricks.Add(Bricks[y - 1, x]);
                    if (y < Bricks.GetLength(0) - 1 && Bricks[y + 1, x] != null && !EdgeBricks.Contains(Bricks[y + 1, x]))
                        EdgeBricks.Add(Bricks[y + 1, x]);
                    if (x > 0 && Bricks[y, x - 1] != null && !EdgeBricks.Contains(Bricks[y, x - 1]))
                        EdgeBricks.Add(Bricks[y, x - 1]);
                    if (x < Bricks.GetLength(1) - 1 && Bricks[y, x + 1] != null && !EdgeBricks.Contains(Bricks[y, x + 1]))
                        EdgeBricks.Add(Bricks[y, x + 1]);
                }
            }
            if (Balls.Count == 0)
                PlayState = GameState.Lost;
            foreach (Brick brick in Bricks)
            {
                if (brick != null)
                    return true;
            }
            PlayState = GameState.Won;
            return true;
        }
    }
}