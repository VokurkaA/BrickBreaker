using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace BrickBreaker
{
    internal class Arrow
    {
        public System.Windows.Shapes.Rectangle arrow { get; }
        private Vector2D Size { get; }
        private double BaseSpeed { get => 50; }
        public double Angle { get; private set; }
        private Direction direction { get; set; }
        public Arrow()
        {
            Angle = 0;
            direction = Direction.Left;
            Size = new Vector2D(10, 60);
            arrow = new System.Windows.Shapes.Rectangle
            {
                Width = Size.X,
                Height = Size.Y,
                Fill = Brushes.Red
            };
        }

        public void Rotate(double deltaTime)
        {
            double moveBy = BaseSpeed * deltaTime;
            if (direction == Direction.Left)
            {
                if (Angle + moveBy > 45)
                {
                    direction = Direction.Right;
                    return;
                }
                Angle += moveBy;
            }
            else 
            {
                if (Angle - moveBy < -45)
                {
                    direction = Direction.Left;
                    return;
                }
                Angle -= moveBy;
            }
            Game.UiDispatcher.Invoke(() =>
            {
                RotateTransform rotateTransform = new RotateTransform(Angle, Size.X / 2, 0);
                arrow.RenderTransform = rotateTransform;
            });
        }
    }
}
