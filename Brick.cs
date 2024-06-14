using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BrickBreaker
{
    internal class Brick
    {
        private readonly string[] colorStops = ["#000000", "#00FF00", "#20DF00", "#40BF00", "#609F00", "#808000", "#9F6000", "#BF4000", "#DF2000", "#FF0000", "#222222"];
        public Rectangle rectangle { get; }
        private int Hp { get; set; }
        public static int Height { get => 20; }
        public static int Width { get => Height * 2; }
        public Vector2D position { get; }
        public double Top { get => position.Y * Height; }
        public double Left { get => position.X * Width; }

        public Brick(int hp, Vector2D position)
        {
            SolidColorBrush color = new BrushConverter().ConvertFrom(colorStops[hp]) as SolidColorBrush ?? Brushes.Red;
            Hp = hp;
            this.position = position;
            rectangle = new Rectangle()
            {
                Height = Height,
                Width = Width,
                Fill = color,
                Stroke = Brushes.Black
            };
        }
        public static bool IsEdgeBrick(Brick[,] bricks, int top, int left)
        {
            int height = bricks.GetLength(0) - 1;
            int width = bricks.GetLength(1) - 1;

            if (bricks[top, left] == null)
                return false;
            if (top == height && bricks[top, left] != null)
                return true;
            if (top > 0 && bricks[top - 1, left] == null)
                return true;
            if (top < height && bricks[top + 1, left] == null)
                return true;
            if (left > 0 && bricks[top, left - 1] == null)
                return true;
            if (left < width && bricks[top, left + 1] == null)
                return true;
            return false;
        }

        /// <summary>
        /// Damages the brick.
        /// </summary>
        /// <returns>Returns true if brick breaks, else false</returns>
        public virtual bool Hit(Canvas gameCanvas, List<Upgrade> upgrades, double currentTime, Paddle paddle, List<Ball> balls)
        {
            Hp -= Ball.Demage;
            if (Hp <= 0)
            {
                Upgrade.New(new Vector2D(Left + Width / 2 - Ball.Diameter / 2, Top + Height / 2), gameCanvas, upgrades, currentTime, paddle, balls);
                Statistics.BrickDestroyed++;
                Game.DestroyableBricksCount--;
                return true;
            }
            Game.UiDispatcher.Invoke(() => rectangle.Fill = new BrushConverter().ConvertFrom(colorStops[Hp]) as SolidColorBrush ?? Brushes.Red);
            return false;
        }
    }

    internal class UndestroyableBrick : Brick
    {
        public UndestroyableBrick(Vector2D position) : base(10, position) { }

        public override bool Hit(Canvas gameCanvas, List<Upgrade> upgrades, double currentTime, Paddle paddle, List<Ball> balls) => false;
    }
}