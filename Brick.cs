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
        public Rectangle rectangle { get; }
        private int Hp { get; set; }
        public static int Height { get => 20; }
        public static int Width { get => Height * 2; }
        public Vector2D position { get; }
        public double Top { get => position.Y * Height; }
        public double Left { get => position.X * Width; }

        public Brick(Brush color, Vector2D position)
        {
            Hp = 1;
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
        public virtual bool Hit()
        {
            Hp -= Ball.Demage;
            if (Hp <= 0)
            {
                //Upgrade.New();
                Statistics.BrickDestroyed++;
                return true;
            }
            return false;
        }
    }

    internal class UndestroyableBrick : Brick
    {
        public UndestroyableBrick(Vector2D position) : base(Brushes.DarkGray, position) { }

        public override bool Hit() => false;
    }
}