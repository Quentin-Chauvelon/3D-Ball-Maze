using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using BallMaze;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

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
        /// Checks if two DateTime objects are within a certain timeframe.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="timeframe">The timeframe in seconds</param>
        /// <returns></returns>
        public static bool DateInTimeframe(this DateTime start, DateTime end, int timeframe)
        {
            return (end - start).TotalSeconds < timeframe;
        }


        /// <summary>
        /// Checks if a DateTime object is within a certain timeframe.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="timeframe">The timeframe in seconds</param>
        /// <returns></returns>
        public static bool DateInTimeframe(this DateTime start, int timeframe)
        {
            return start.DateInTimeframe(DateTime.UtcNow, timeframe);
        }


        /// <summary>
        /// Returns the date at midnight UTC of the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime MidnightUtc(this DateTime date)
        {
            return date.Date.AddDays(1);
        }


        /// <summary>
        /// Returns the date at the start of the week of the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime EndOfWeek(this DateTime date)
        {
            int days = date.DayOfWeek - DayOfWeek.Monday;

            if (days < 0)
                days += 7;

            return date.AddDays(-1 * days).Date.AddDays(7);
        }


        /// <summary>
        /// Checks if two floats are equal within a certain epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AlmostEquals(this float a, float b, float epsilon = 0.0001f)
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
        public static bool AlmostEquals(this double a, double b, double epsilon = 0.0001)
        {
            return Math.Abs(a - b) < epsilon;
        }


        /// <summary>
        /// Rounds each component of the given vector to the nearest integer.
        /// If a nearest value is provided, it will round to the nearest multiple of that value. For
        /// example, if nearest is 0.5, it will round to the nearest half.
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Round(this Vector3 vector3, float nearest = 1f)
        {
            if (nearest == 1f)
            {
                return new Vector3(
                    Mathf.Round(vector3.x),
                    Mathf.Round(vector3.y),
                    Mathf.Round(vector3.z)
                );
            }
            else
            {
                // Dividing then multiplying by nearest will round to the nearest multiple of nearest
                return new Vector3(
                    Mathf.Round(vector3.x / nearest) * nearest,
                    Mathf.Round(vector3.y / nearest) * nearest,
                    Mathf.Round(vector3.z / nearest) * nearest
                );
            }
        }


        /// <summary>
        /// Similar to Round, but only rounds the X and Z components of the vector.
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="nearest"></param>
        /// <returns></returns>
        public static Vector3 RoundXZ(this Vector3 vector3, float nearest = 1f)
        {
            Vector3 roundedVector3 = vector3.Round(nearest);

            return new Vector3(roundedVector3.x, vector3.y, roundedVector3.z);
        }


        public static Vector3 Truncate(this Vector3 vector3)
        {
            return new Vector3(
                (float)Math.Truncate(vector3.x),
                (float)Math.Truncate(vector3.y),
                (float)Math.Truncate(vector3.z)
            );
        }


        /// <summary>
        /// Gets the name and color from the given DailyLevelDifficulty enum.
        /// This is used to display difficulties in the UI with a friendly name and color.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static (string name, Color color) GetDifficultyInfo(this DailyLevelDifficulty difficulty)
        {
            switch (difficulty)
            {
                case DailyLevelDifficulty.VeryEasy:
                    return ("Very easy", new Color(0.235f, 0.765f, 0.22f));
                case DailyLevelDifficulty.Easy:
                    return ("Easy", new Color(1f, 0.898f, 0f));
                case DailyLevelDifficulty.Medium:
                    return ("Medium", new Color(1f, 0.592f, 0f));
                case DailyLevelDifficulty.Hard:
                    return ("Hard", new Color(1f, 0f, 0f));
                case DailyLevelDifficulty.Extreme:
                    return ("Extreme", new Color(0.686f, 0f, 0.757f));
                case DailyLevelDifficulty.Unknown:
                    return ("", new Color(0.51f, 0.51f, 0.51f));
                default:
                    return ("", new Color(0.51f, 0.51f, 0.51f));
            }
        }


        public static async UniTask TweenToPosition(this VisualElement element, float endPosition, float duration = 0.5f, Ease ease = Ease.OutExpo)
        {
            Debug.Log($"tweening to {endPosition} in {duration} seconds");

            // Skip the first 15% of the screen height since it's the permanent UI, then move the UI to the center of the 85% left
            await DOTween.To(() => element.style.top.value.value, x => element.style.top = x, endPosition, duration).SetEase(ease).AsyncWaitForCompletion();
        }
    }
}