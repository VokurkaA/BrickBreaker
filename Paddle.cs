using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BrickBreaker
{
    internal class Paddle
    {
        public Rectangle rectangle { get; }
        public Vector2D Size { get; private set; }
        public Vector2D Position { get; private set; }
        private double MoveIncrement { get; }

        public Paddle()
        {
            Size = new Vector2D(75, 5);
            Position = new Vector2D(GamePage.CanvasWidth / 2 - Size.X / 2, GamePage.CanvasHeight - 50);
            rectangle = new()
            {
                Height = Size.Y,
                Width = Size.X,
                Fill = Brushes.White
            };
            MoveIncrement = 2.5;
        }
        public double Move(Direction moveDirection)
        {
            if (moveDirection != Direction.None)
            {
                if (moveDirection == Direction.Right && Position.X + Size.X < GamePage.CanvasWidth)
                    return Position.X += MoveIncrement;
                else if (moveDirection == Direction.Left && Position.X > 0)
                    return Position.X -= MoveIncrement;
            }
            return Position.X;
        }
    }
}
