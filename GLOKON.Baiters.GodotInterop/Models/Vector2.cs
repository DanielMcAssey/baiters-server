namespace GLOKON.Baiters.GodotInterop.Models
{
    public class Vector2(float x, float y)
    {
        public static readonly Vector2 Zero = new(0, 0);

        public float x = x;
        public float y = y;

        public static Vector2 operator *(Vector2 a, float scalar)
        {
            return new(a.x * scalar, a.y * scalar);
        }

        /// <summary>
        /// Calculates Magnitude of Vector2
        /// </summary>
        /// <returns>Magnitude of Vector2</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Normalizes Vector2
        /// </summary>
        /// <returns>Normalized Vector2</returns>
        /// <exception cref="InvalidOperationException">Throws if zero vector</exception>
        public Vector2 Normalized()
        {
            float magnitude = Magnitude();
            if (magnitude == 0)
            {
                throw new InvalidOperationException("Cannot normalize a zero vector.");
            }

            return new(x / magnitude, y / magnitude);
        }

        /// <summary>
        /// Calculates the angle in radians between -π and π
        /// </summary>
        /// <returns>Angle in radians</returns>
        public float Angle()
        {
            return (float)Math.Atan2(y, x);
        }

        /// <summary>
        /// Calculates the angle in degrees
        /// </summary>
        /// <returns>Angle in degrees</returns>
        public float AngleInDegrees()
        {
            return Angle() * (180f / (float)Math.PI);
        }

        /// <summary>
        /// Rotates the Vector2 by provided angle
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Rotated Vector2</returns>
        public Vector2 Rotate(float angle)
        {
            float cosTheta = (float)Math.Cos(angle);
            float sinTheta = (float)Math.Sin(angle);

            return new(x * cosTheta - y * sinTheta, x * sinTheta + y * cosTheta);
        }

        /// <summary>
        /// Rotates the Vector2 by provided angle
        /// </summary>
        /// <param name="angleDegrees">Angle in degrees</param>
        /// <returns>Rotated Vector2</returns>
        public Vector2 RotateInDegrees(float angleDegrees)
        {
            return Rotate(angleDegrees * ((float)Math.PI / 180f));
        }

        /// <summary>
        /// Show Vector2 as a formatted string
        /// </summary>
        /// <returns>Vector2 as a string</returns>
        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
