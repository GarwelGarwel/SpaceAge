using UnityEngine;
using System;

namespace SpaceAge
{
    class Core
    {
        // DOES NOT WORK
        public static double VesselCost(Vessel v)
        {
            double c = 0;
            Core.Log("Calculating costs of " + v.vesselName);
            foreach (Part p in v.Parts)
            {
                Core.Log("Part " + p.name + ": module costs = " + p.GetModuleCosts(0) + "; proto costs = " + p.protoPartSnapshot.moduleCosts);
                c += p.GetModuleCosts(0);
            }
            return c;
        }

        public static void ShowNotification(string msg)
        { if (HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().showNotifications) ScreenMessages.PostScreenMessage(msg); }

        /// <summary>
        /// Parses UT into a string (e.g. "Y23 D045"), hides zero elements
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <param name="showSeconds">If false, seconds will be displayed only if time is less than 1 minute; otherwise always</param>
        /// <returns></returns>
        public static string ParseUT(double time, bool showSeconds = false)
        {
            if (Double.IsNaN(time)) return "—";
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
            return "Y" + y + " D" + d.ToString("D3") + " " + h + ":" + m.ToString("D2") + (showSeconds ? (":" + ((int) t).ToString("D2")) : "");
        }

        public static string GetString(ConfigNode n, string key, string defaultValue = null) => n.HasValue(key) ? n.GetValue(key) : defaultValue;

        public static double GetDouble(ConfigNode n, string key, double defaultValue = 0)
        {
            double res;
            try { res = Double.Parse(n.GetValue(key)); }
            catch (Exception) { res = defaultValue; }
            return res;
        }

        public static int GetInt(ConfigNode n, string key, int defaultValue = 0)
        {
            int res;
            try { res = Int32.Parse(n.GetValue(key)); }
            catch (Exception) { res = defaultValue; }
            return res;
        }

        public static bool GetBool(ConfigNode n, string key, bool defaultValue = false)
        {
            bool res;
            try { res = Boolean.Parse(n.GetValue(key)); }
            catch (Exception) { res = defaultValue; }
            return res;
        }

        public static bool UseBlizzysToolbar => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().UseBlizzysToolbar;

        public static bool NewestFirst => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().newestFirst;

        /// <summary>
        /// Log levels:
        /// <list type="bullet">
        /// <item><definition>None: do not log</definition></item>
        /// <item><definition>Error: log only errors</definition></item>
        /// <item><definition>Important: log only errors and important information</definition></item>
        /// <item><definition>Debug: log all information</definition></item>
        /// </list>
        /// </summary>
        public enum LogLevel { None, Error, Important, Debug };

        /// <summary>
        /// Current <see cref="LogLevel"/>: either Debug or Important
        /// </summary>
        public static LogLevel Level => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().debugMode ? LogLevel.Debug : LogLevel.Important;

        /// <summary>
        /// Write into output_log.txt
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        public static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        { if (messageLevel <= Level) Debug.Log("[SpaceAge] " + message); }
    }
}
