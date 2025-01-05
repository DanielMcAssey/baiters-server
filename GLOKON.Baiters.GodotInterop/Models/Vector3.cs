namespace GLOKON.Baiters.GodotInterop.Models
{
    public class Vector3(float x, float y, float z)
    {
        public static readonly Vector3 Zero = new(0, 0, 0);

        public float x = x;
        public float y = y;
        public float z = z;

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, float scalar)
        {
            return new(a.x * scalar, a.y * scalar, a.z * scalar);
        }

        public static Vector3 operator /(Vector3 a, float scalar)
        {
            if (scalar == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            return new(a.x / scalar, a.y / scalar, a.z / scalar);
        }

        /// <summary>
        /// Calculates the dot product of two Vector3's
        /// </summary>
        /// <param name="a">Vector3 one</param>
        /// <param name="b">Vector3 two</param>
        /// <returns>Dot Product result of two Vector3's</returns>
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Calculate the cross product of two Vector3's
        /// </summary>
        /// <param name="a">Vector3 one</param>
        /// <param name="b">Vector3 two</param>
        /// <returns>Cross Product result of two Vector3's</returns>
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );
        }

        /// <summary>
        /// Calculates Magnitude of Vector3
        /// </summary>
        /// <returns>Magnitude of Vector3</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Normalizes Vector3
        /// </summary>
        /// <returns>Normalized Vector3</returns>
        /// <exception cref="InvalidOperationException">Throws if zero vector</exception>
        public Vector3 Normalized()
        {
            float magnitude = Magnitude();
            if (magnitude == 0)
            {
                throw new InvalidOperationException("Cannot normalize a zero vector.");
            }

            return this / magnitude;
        }

        /// <summary>
        /// Show Vector3 as a formatted string
        /// </summary>
        /// <returns>Vector3 as a string</returns>
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
