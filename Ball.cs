using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace BrickBreaker
{
    internal class Ball : IDisposable
    {

        public Ball(Vector2D position, Vector2D velocity)
        {
            Position = position;
            Velocity = velocity;
            IsAlive = true;
            ellipse = new Ellipse
            {
                Height = Diameter,
                Width = Diameter,
                Fill = Brushes.Red
            };
        }
        private static readonly int baseDemage = 1;
        public static int DemageMultiplier { get; set; } = 1;
        public static int Demage { get => baseDemage * DemageMultiplier; }
        public static int Diameter { get => 15; }
        private static int Radius { get => Diameter / 2; }
        public bool IsAlive { get; private set; }
        public Vector2D Position { get; private set; }
        public Vector2D Velocity { get; private set; }
        public Ellipse ellipse { get; }
        public (bool, Brick?) Move(double deltaTime, Brick[] edgeBricks, Paddle paddle, Canvas gameCanvas, List<Upgrade> upgrades, double currentTime, List<Ball> balls)
        {
            Position += (Velocity * deltaTime).Normalize() * 1.5;

            if (Position.Y > GamePage.CanvasHeight - Diameter)
                Dispose();

            return (IsAlive, CheckForCollisions(edgeBricks, paddle, gameCanvas, upgrades, currentTime, balls));
        }

        private Brick? CheckForCollisions(Brick[] edgeBricks, Paddle paddle, Canvas gameCanvas, List<Upgrade> upgrades, double currentTime, List<Ball> balls)
        {
            Direction reflectionDirection = BallHitsPaddle(paddle);
            if (reflectionDirection != Direction.None)
                Velocity = Velocity.Reflect(reflectionDirection);

            foreach (Brick brick in edgeBricks)
            {
                reflectionDirection = BallHitsFrom(brick);
                if (reflectionDirection != Direction.None)
                {
                    Velocity = Velocity.Reflect(reflectionDirection);
                    if (brick.Hit(gameCanvas, upgrades, currentTime, paddle, balls))
                        return brick;
                    return null;
                }
            }

            if (Position.X >= GamePage.CanvasWidth - Diameter)
                Velocity = Velocity.Reflect(Direction.Right);
            if (Position.X <= 0)
                Velocity = Velocity.Reflect(Direction.Left);
            if (Position.Y <= 0)
                Velocity = Velocity.Reflect(Direction.Top);
            return null;
        }
        public void Dispose()
        {
            IsAlive = false;
            GC.SuppressFinalize(this);
        }
        private Direction BallHitsPaddle(Paddle paddle)
        {
            if (Position.Y + Diameter < paddle.Position.Y)
                return Direction.None;

            Vector2D distance = new(
                Math.Abs(Position.X + Radius - (paddle.Position.X + paddle.Size.X / 2)),
                Math.Abs(Position.Y + Radius - (paddle.Position.Y + paddle.Size.Y / 2))
            );

            if (distance.X <= (paddle.Size.X / 2 + Radius) && distance.Y <= (paddle.Size.Y / 2 + Radius))
            {
                if (Position.Y + Radius < paddle.Position.Y)
                    return Direction.Top;
                if (Position.X + Radius < paddle.Position.X)
                    return Direction.Left;
                if (Position.X > paddle.Position.X + paddle.Size.X)
                    return Direction.Right;
            }
            return Direction.None;
        }

        private Direction BallHitsFrom(Brick brick)
        {
            Vector2D distance = new Vector2D(
                Position.X - Math.Max(brick.Left, Math.Min(Position.X, brick.Left + Brick.Width)),
                Position.Y - Math.Max(brick.Top, Math.Min(Position.Y, brick.Top + Brick.Height))
            );
            if ((distance.X * distance.X) + (distance.Y * distance.Y) < (Radius * Radius))
            {
                if (Math.Abs(distance.Y) < Diameter && Position.Y < brick.Top + Brick.Height / 2)
                    return Direction.Top;
                if (Math.Abs(distance.Y) < Diameter && Position.Y > brick.Top + Brick.Height / 2)
                    return Direction.Bottom;
                if (Math.Abs(distance.X) < Diameter && Position.X < brick.Left + Brick.Width / 2)
                    return Direction.Left;
                return Direction.Right;
            }
            return Direction.None;
        }
    }
}