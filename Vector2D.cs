using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker
{
    internal class Vector2D
    {
        private static readonly Dictionary<Direction, Vector2D> directionNormals = new Dictionary<Direction, Vector2D>
    {
        { Direction.Top, new Vector2D(0, -1) },
        { Direction.Left, new Vector2D(-1, 0) },
        { Direction.Bottom, new Vector2D(0, 1) },
        { Direction.Right, new Vector2D(1, 0) }
    };
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Normalizes the vector to unit length.
        /// </summary>
        public Vector2D Normalize()
        {
            double lengthSquared = LengthSquared();
            if (lengthSquared > 0)
            {
                double inverseSqrt = FastInverseSqrt(lengthSquared);
                return new Vector2D(X * inverseSqrt, Y * inverseSqrt);
            }
            return new Vector2D(0, 0);
        }

        /// <summary>
        /// Reflects the vector with respect to a given normal vector.
        /// </summary>
        /// <param name="normal">The normal vector of the plane.</param>
        /// <returns>The reflected vector.</returns>
        public Vector2D Reflect(Direction direction)
        {
            Vector2D normal = directionNormals[direction];
            double dotProduct = DotProduct(this, normal);
            Vector2D reflection = this - (normal * (2 * dotProduct));
            return reflection;
        }


        /// <summary>
        /// Computes the length squared of the vector.
        /// </summary>
        /// <returns>The length squared of the vector.</returns>
        private double LengthSquared()
        {
            return X * X + Y * Y;
        }
        public double Length()
        {
            return LengthSquared();
        }

        /// <summary>
        /// Computes an approximation of the inverse square root of a given number.
        /// This method is based on "Quake's fast inverse square root" algorithm.
        /// </summary>
        /// <param name="x">The number to compute the inverse square root of.</param>
        /// <returns>An approximation of the inverse square root of the given number.</returns>
        private static double FastInverseSqrt(double x)
        {
            double xhalf = 0.5 * x;
            long i = BitConverter.DoubleToInt64Bits(x);
            i = 0x5FE6EB50C7B537A9 - (i >> 1);
            x = BitConverter.Int64BitsToDouble(i);
            x = x * (1.5 - xhalf * x * x);
            return x;
        }
        public static Vector2D FromAngle(double angle)
        {
            angle %= 360;
            double angleInRadians = angle * Math.PI / 180.0;

            double x = Math.Cos(angleInRadians);
            double y = Math.Sin(angleInRadians);

            return new Vector2D(x, y);
        }
        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2D operator *(Vector2D v, double scalar)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }
        public static Vector2D operator /(Vector2D v, double divider)
        {
            return new Vector2D(v.X / divider, v.Y / divider);
        }
        public static double DotProduct(Vector2D v1, Vector2D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
    }
}