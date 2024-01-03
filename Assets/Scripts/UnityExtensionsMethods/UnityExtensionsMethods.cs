using System;
using UnityEngine;

namespace UnityExtensionMethods
{
    public static class UnityExtensionsMethods
    {
        /// <summary>
        /// Converts a DateTime object to a Unix timestamp.
        /// </summary>
        /// <param name="date">The date to convert to a Unix timestamp</param>
        /// <returns>The number of seconds that have passed since the epoch</returns>
        public static int DateToUnixTimestamp(this DateTime date)
        {
            return (int)Math.Floor((date - DateTime.UnixEpoch).TotalSeconds);
        }


        /// <summary>
        /// Converts a Unix timestamp to a DateTime object.
        /// </summary>
        /// <param name="timestamp">The Unix timestamp to convert to a date</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDate(this int timestamp)
        {
            return DateTime.UnixEpoch.AddSeconds(timestamp);
        }


        /// <summary>
        /// Checks if two floats are equal within a certain epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool FloatEquals(this float a, float b, float epsilon = 0.0001f)
        {
            return Math.Abs(a - b) < epsilon;
        }

        
        /// <summary>
        /// Checks if two doubles are equal within a certain epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool DoubleEquals(this double a, double b, double epsilon = 0.0001)
        {
            return Math.Abs(a - b) < epsilon;
        }


        /// <summary>
        /// Rounds each component of the given vector.
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Round(this Vector3 vector3)
        {
            return new Vector3(
                (float)Math.Round(vector3.x),
                (float)Math.Round(vector3.y),
                (float)Math.Round(vector3.z)
            );
        }
    }
}