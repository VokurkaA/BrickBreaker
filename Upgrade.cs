using System;
using System.Collections.Generic;
using System.Linq;
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
        public Ellipse ellipse { get; }
        public Double ExecutionTime { get; }
        private static int Diameter { get => 15; }
        public Upgrade(double executionTime)
        {
            ExecutionTime = executionTime;
            ellipse = new Ellipse
            {
                Height = Diameter,
                Width = Diameter,
                Fill = Brushes.Gray,
                Opacity = 0.5
            };
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
        public static void New(double currentTime, Paddle paddle, List<Ball> balls, Canvas gameCanvas)
        {
            double probability = Rnd.NextDouble();
            if (probability > 0.1)
                return;
            if (probability > 0.075)
                Game.UpgradeQueue.Enqueue(new AddThreeBalls(currentTime, paddle, balls, gameCanvas));
            if (probability > 0.05)
                Game.UpgradeQueue.Enqueue(new MultiplyBalls(currentTime, balls, gameCanvas));
            if (probability > 0.025)
            {
                Game.UpgradeQueue.Enqueue(new EnlargePlatform(currentTime, paddle));
                Game.UpgradeQueue.Enqueue(new ShrinkPlatform(currentTime + 10, paddle));
            }
            Game.UpgradeQueue.Enqueue(new DoubleDemage(currentTime));
            Game.UpgradeQueue.Enqueue(new ResetDemage(currentTime));
        }
    }
    internal class AddThreeBalls : Upgrade
    {
        private Paddle paddle { get; }
        private List<Ball> Balls { get; set; }
        private Canvas GameCanvas { get; }
        public AddThreeBalls(double currentTime, Paddle paddle, List<Ball> balls, Canvas gameCanvas) : base(currentTime)
        {
            this.paddle = paddle;
            Balls = balls;
            GameCanvas = gameCanvas;
        }
        public override void Execute()
        {
            double angleInRadians = Math.PI / 4;
            Vector2D start = new(
                paddle.Position.X + paddle.Size.X / 2,
                paddle.Position.Y + paddle.Size.Y
            );

            Game.UiDispatcher.Invoke(() =>
            {
                Ball ball1 = new(new Vector2D(start.X, start.Y), new Vector2D(Math.Cos(angleInRadians), -Math.Sin(angleInRadians)));
                Balls.Add(ball1);
                Canvas.SetTop(ball1.ellipse, start.Y);
                Canvas.SetLeft(ball1.ellipse, start.X);
                GameCanvas.Children.Add(ball1.ellipse);

                Ball ball2 = new(new Vector2D(start.X, start.Y), new Vector2D(0, -1));
                Balls.Add(ball2);
                Canvas.SetTop(ball2.ellipse, start.Y);
                Canvas.SetLeft(ball2.ellipse, start.X);
                GameCanvas.Children.Add(ball2.ellipse);

                Ball ball3 = new(new Vector2D(start.X, start.Y), new Vector2D(Math.Cos(-angleInRadians), -Math.Sin(-angleInRadians)));
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
        public MultiplyBalls(double currentTime, List<Ball> balls, Canvas gameCanvas) : base(currentTime)
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
        public EnlargePlatform(double executionTime, Paddle paddle) : base(executionTime)
        {
            this.paddle = paddle;
        }

        public override void Execute()
        {
            if (paddle.Position.X - paddle.Size.X > 0)
            {
                if (paddle.Position.X + paddle.Size.X * 2 > GamePage.CanvasWidth)
                    paddle.Position.X = GamePage.CanvasWidth - paddle.Size.X * 2;
                else
                    paddle.Position.X -= paddle.Size.X;
            }
            else
                paddle.Position.X = 0;

            paddle.Size.X *= 2;
            Game.UiDispatcher.Invoke(() =>
            {
                Canvas.SetLeft(paddle.rectangle, paddle.Position.X);
            });
        }
    }
    internal class ShrinkPlatform : Upgrade
    {
        private Paddle paddle { get; }

        public ShrinkPlatform(double executionTime, Paddle paddle) : base(executionTime)
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
            });
        }
    }
    internal class DoubleDemage : Upgrade
    {
        public DoubleDemage(double executionTime) : base(executionTime)
        {
        }
        public override void Execute()
        {
            Ball.DemageMultiplier *= 2;
        }
    }
    internal class ResetDemage : Upgrade
    {
        public ResetDemage(double executionTime) : base(executionTime)
        {
        }
        public override void Execute()
        {
            Ball.DemageMultiplier = 1;
        }
    }
}