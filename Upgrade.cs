using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrickBreaker
{
    internal abstract class Upgrade : IComparable
    {
        private static readonly Random Rnd = new();
        public static int Diameter { get => 20; }
        private static int BaseSpeed { get => 15; }
        public Ellipse ellipse { get; private set; }
        internal Vector2D Position { get; private set; }
        public double ExecutionTime { get; internal set; }
        public Upgrade(double currentTime, Vector2D position)
        {
            ExecutionTime = currentTime;
            Position = position;
            Game.UiDispatcher.Invoke(() =>
            {
                ellipse = new Ellipse
                {
                    Height = Diameter,
                    Width = Diameter,
                    Fill = Brushes.Gray,
                    Opacity = 0.5
                };
            });
        }
        public bool Move(double deltaTime)
        {
            Position.Y += (BaseSpeed * deltaTime) * 2;
            Game.UiDispatcher.Invoke(() => { Canvas.SetTop(ellipse, Position.Y); });
            if (Position.Y <= 0)
                return false;
            return true;
        }
        public abstract void Execute();
        public int CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            if (obj is Upgrade otherUpgrade)
                return ExecutionTime.CompareTo(otherUpgrade.ExecutionTime);
            throw new ArgumentException("Object is not an of type \"Upgrade\"");
        }
        public static bool New(Vector2D position, Canvas gameCanvas, List<Upgrade> upgrades, double currentTime, Paddle paddle, List<Ball> balls)
        {
            if (Rnd.NextDouble() > 0.05)
                return false;

            double probability = Rnd.NextDouble();
            Upgrade upgrade;
            upgrade = new EnlargePlatform(position, currentTime, paddle);
            if (probability > 0.75)
                upgrade = new AddThreeBalls(position, paddle, balls, gameCanvas);
            else if (probability > 0.5)
                upgrade = new MultiplyBalls(position, balls, gameCanvas);
            else if (probability > 0.25)
                upgrade = new EnlargePlatform(position, currentTime, paddle);
            else
                upgrade = new DoubleDemage(position, currentTime);

            Game.UiDispatcher.Invoke(() =>
            {
                Canvas.SetTop(upgrade.ellipse, position.Y);
                Canvas.SetLeft(upgrade.ellipse, position.X);
                gameCanvas.Children.Add(upgrade.ellipse);
            });
            upgrades.Add(upgrade);
            return true;
        }
    }
    internal class AddThreeBalls : Upgrade
    {
        private Paddle paddle { get; }
        private List<Ball> Balls { get; set; }
        private Canvas GameCanvas { get; }
        public AddThreeBalls(Vector2D position, Paddle paddle, List<Ball> balls, Canvas gameCanvas) : base(0, position)
        {
            this.paddle = paddle;
            Balls = balls;
            GameCanvas = gameCanvas;
        }
        public override void Execute()
        {
            Vector2D start = new(
                paddle.Position.X + paddle.Size.X / 2,
                paddle.Position.Y - (Ball.Diameter + paddle.Size.Y)
            );

            Game.UiDispatcher.Invoke(() =>
            {
                Ball ball1 = new(new Vector2D(start.X, start.Y), new Vector2D(0, -1));
                Balls.Add(ball1);
                Canvas.SetTop(ball1.ellipse, start.Y);
                Canvas.SetLeft(ball1.ellipse, start.X);
                GameCanvas.Children.Add(ball1.ellipse);

                Ball ball2 = new(new Vector2D(start.X, start.Y), new Vector2D(0.5, -1));
                Balls.Add(ball2);
                Canvas.SetTop(ball2.ellipse, start.Y);
                Canvas.SetLeft(ball2.ellipse, start.X);
                GameCanvas.Children.Add(ball2.ellipse);

                Ball ball3 = new(new Vector2D(start.X, start.Y), new Vector2D(-0.5, -1));
                Balls.Add(ball3);
                Canvas.SetTop(ball3.ellipse, start.Y);
                Canvas.SetLeft(ball3.ellipse, start.X);
                GameCanvas.Children.Add(ball3.ellipse);
            });
        }
    }
    internal class MultiplyBalls : Upgrade
    {
        private Ball[] BallsCopy { get; }
        private Canvas GameCanvas { get; }
        private List<Ball> Balls { get; set; }
        public MultiplyBalls(Vector2D position, List<Ball> balls, Canvas gameCanvas) : base(0, position)
        {
            GameCanvas = gameCanvas;
            Balls = balls;
            BallsCopy = balls.ToArray();

        }
        public override void Execute()
        {
            foreach (Ball ball in BallsCopy)
            {
                if (Balls.Count >= 50)
                    return;

                double angleInRadians = Math.Atan2(ball.Velocity.Y, ball.Velocity.X);
                double angleLeft = angleInRadians - Math.PI / 4;
                double angleRight = angleInRadians + Math.PI / 4;

                Vector2D newVelocityLeft = new Vector2D(Math.Cos(angleLeft), Math.Sin(angleLeft)) * ball.Velocity.Length();
                Vector2D newVelocityRight = new Vector2D(Math.Cos(angleRight), Math.Sin(angleRight)) * ball.Velocity.Length();

                Game.UiDispatcher.Invoke(() =>
                {
                    Ball ball1 = new(ball.Position, newVelocityLeft);
                    Balls.Add(ball1);
                    Canvas.SetTop(ball1.ellipse, ball.Position.Y);
                    Canvas.SetLeft(ball1.ellipse, ball.Position.X);
                    GameCanvas.Children.Add(ball1.ellipse);

                    Ball ball2 = new(ball.Position, newVelocityRight);
                    Balls.Add(ball2);
                    Canvas.SetTop(ball2.ellipse, ball.Position.Y);
                    Canvas.SetLeft(ball2.ellipse, ball.Position.X);
                    GameCanvas.Children.Add(ball2.ellipse);
                });
            }
        }
    }
    internal class EnlargePlatform : Upgrade
    {
        private Paddle paddle { get; }
        public EnlargePlatform(Vector2D position, double currentTime, Paddle paddle) : base(currentTime, position)
        {
            this.paddle = paddle;
        }
        
        public override void Execute()
        {
            int index = Game.UpgradeQueue.ToList().FindIndex(item => item is ShrinkPlatform);
            if (index == -1)
                Game.UpgradeQueue.Enqueue(new ShrinkPlatform(Position, ExecutionTime + 25, paddle));
            else
            {
                Upgrade[] upgradeArray = Game.UpgradeQueue.ToArray();
                upgradeArray[index].ExecutionTime += 10;
                Game.UpgradeQueue = new Queue<Upgrade>(upgradeArray);
                return;
            }
            paddle.Position.X -= paddle.Size.X / 2;
            paddle.Size.X *= 2;
            if (paddle.Position.X < 0)
                paddle.Position.X = 0;
            else if (paddle.Position.X + paddle.Size.X > GamePage.CanvasWidth)
                paddle.Position.X = GamePage.CanvasWidth - paddle.Size.X;

            Game.UiDispatcher.Invoke(() =>
            {
                Canvas.SetLeft(paddle.rectangle, paddle.Position.X);
                paddle.rectangle.Width = paddle.Size.X;
            });
        }
    }
    internal class ShrinkPlatform : Upgrade
    {
        private Paddle paddle { get; }

        public ShrinkPlatform(Vector2D position, double executionTime, Paddle paddle) : base(executionTime, position)
        {
            this.paddle = paddle;
        }

        public override void Execute()
        {
            paddle.Size.X /= 2;
            paddle.Position.X += paddle.Size.X / 2;
            Game.UiDispatcher.Invoke(() =>
            {
                Canvas.SetLeft(paddle.rectangle, paddle.Position.X);
                paddle.rectangle.Width = paddle.Size.X;
            });
        }
    }
    internal class DoubleDemage : Upgrade
    {
        public DoubleDemage(Vector2D position, double currentTime) : base(currentTime, position)
        {
        }
        public override void Execute()
        {
            int index = Game.UpgradeQueue.ToList().FindIndex(item => item is ResetDemage);
            if (index == -1)
                Game.UpgradeQueue.Enqueue(new ResetDemage(Position, ExecutionTime + 25));
            else
            {
                Upgrade[] upgradeArray = Game.UpgradeQueue.ToArray();
                upgradeArray[index].ExecutionTime += 10;
                Game.UpgradeQueue = new Queue<Upgrade>(upgradeArray);
            }
            Ball.DemageMultiplier *= 2;
        }
    }
    internal class ResetDemage : Upgrade
    {
        public ResetDemage(Vector2D position, double executionTime) : base(executionTime, position)
        {
        }
        public override void Execute()
        {
            Ball.DemageMultiplier = 1;
        }
    }
}