using KSP.Localization;
using System;
using UnityEngine;

namespace SpaceAge
{
    /// <summary>
    /// Log levels:
    /// <list type="bullet">
    /// <item><definition>None: do not log</definition></item>
    /// <item><definition>Error: log only errors</definition></item>
    /// <item><definition>Important: log only errors and important information</definition></item>
    /// <item><definition>Debug: log all information</definition></item>
    /// </list>
    /// </summary>
    internal enum LogLevel { None = 0, Error, Important, Debug };

    internal static class Core
    {
        /// <summary>
        /// Current <see cref="LogLevel"/>: either Debug or Important
        /// </summary>
        internal static LogLevel Level => SpaceAgeChronicleSettings.Instance.DebugMode ? LogLevel.Debug : LogLevel.Important;

        public static double GetCost(this Vessel v)
        {
            double cost = 0;
            Core.Log("Calculating cost of " + v.vesselName);
            foreach (Part p in v.Parts)
            {
                Core.Log("Part " + p.name + ": part cost = " + p.partInfo.cost + "; module costs = " + p.GetModuleCosts(0));
                cost += p.partInfo.cost;
                cost += p.GetModuleCosts(0);
                foreach (PartResource resource in p.Resources)
                {
                    double resourceCost = resource.amount * resource.info.unitCost;
                    if (resource.amount != 0)
                        Log(resource.amount + " of " + resource.resourceName + " costs " + resourceCost);
                    cost += resourceCost;
                }
            }
            Core.Log("Total cost is " + cost);
            return cost;
        }

        public static string GetBodyDisplayName(string bodyName) => FlightGlobals.GetBodyByName(bodyName)?.displayName ?? bodyName;

        public static void ShowNotification(string msg)
        {
            if (SpaceAgeChronicleSettings.Instance.ShowNotifications)
                ScreenMessages.PostScreenMessage(msg);
        }

        /// <summary>
        /// Parses UT into a string (e.g. "Y23 D045"), hides zero elements
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <param name="showSeconds">If false, seconds will be displayed only if time is less than 1 minute; otherwise always</param>
        /// <returns></returns>
        public static string ParseUT(double time, bool showSeconds = false)
        {
            if (Double.IsNaN(time))
                return "—";
            double t = time;
            int y, d, m, h;
            y = (int)Math.Floor(t / KSPUtil.dateTimeFormatter.Year) + 1;
            t -= (y - 1) * KSPUtil.dateTimeFormatter.Year;
            d = (int)Math.Floor(t / KSPUtil.dateTimeFormatter.Day) + 1;
            t -= (d - 1) * KSPUtil.dateTimeFormatter.Day;
            h = (int)Math.Floor(t / 3600);
            t -= h * 3600;
            m = (int)Math.Floor(t / 60);
            t -= m * 60;
            return showSeconds
                ? Localizer.Format("#SpaceAge_DateTime_Sec", y, d.ToString("D3"), h, m.ToString("D2"), ((int)t).ToString("D2"))
                : Localizer.Format("#SpaceAge_DateTime_NoSec", y, d.ToString("D3"), h, m.ToString("D2"));
        }

        public static string GetString(this ConfigNode n, string key, string defaultValue = null) => n.HasValue(key) ? n.GetValue(key) : defaultValue;

        public static double GetDouble(this ConfigNode n, string key, double defaultValue = 0)
              => Double.TryParse(n.GetValue(key), out double val) ? val : defaultValue;

        public static int GetInt(this ConfigNode n, string key, int defaultValue = 0)
               => Int32.TryParse(n.GetValue(key), out int val) ? val : defaultValue;

        public static uint GetUInt(this ConfigNode n, string key, uint defaultValue = 0)
            => UInt32.TryParse(n.GetValue(key), out uint val) ? val : defaultValue;

        public static bool GetBool(this ConfigNode n, string key, bool defaultValue = false)
               => Boolean.TryParse(n.GetValue(key), out bool val) ? val : defaultValue;

        /// <summary>
        /// Write into output_log.txt
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        internal static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        {
            if (messageLevel <= Level)
                Debug.Log("[SpaceAge] " + message);
        }
    }
}
