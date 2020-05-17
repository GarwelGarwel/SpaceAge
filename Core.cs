using UnityEngine;
using System;

namespace SpaceAge
{
    class Core
    {
        public static double VesselCost(Vessel v)
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
        public static LogLevel Level => SpaceAgeChronicleSettings.Instance.DebugMode ? LogLevel.Debug : LogLevel.Important;

        /// <summary>
        /// Write into output_log.txt
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        public static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        {
            if (messageLevel <= Level)
                Debug.Log("[SpaceAge] " + message);
        }
    }
}
