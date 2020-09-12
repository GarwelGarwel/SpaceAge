using KSP.Localization;
using System.Linq;
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

        /// <summary>
        /// Determines if events of the given vessel should be tracked
        /// </summary>
        /// <param name="v"></param>
        /// <param name="mustBeActive"></param>
        /// <returns></returns>
        public static bool IsTrackable(this Vessel v, bool mustBeActive)
          => v != null
          && v.vesselType != VesselType.Debris
          && v.vesselType != VesselType.EVA
          && v.vesselType != VesselType.Flag
          && v.vesselType != VesselType.SpaceObject
          && v.vesselType != VesselType.Unknown
          && (!mustBeActive || v == FlightGlobals.ActiveVessel);

        public static double GetCost(this Vessel v) =>
            v.Parts.Sum(p => p.partInfo.cost + p.GetModuleCosts(0) + p.Resources.Sum(pr => pr.amount * pr.info.unitCost));

        public static bool IsBurning(this Vessel v) => v.FindPartModulesImplementing<ModuleEngines>().Any(module => module.GetCurrentThrust() > 0);

        public static bool IsLandedOrSplashed(this Vessel.Situations situation) => (situation & (Vessel.Situations.LANDED | Vessel.Situations.SPLASHED)) != 0;

        public static string GetBodyDisplayName(string bodyName) => bodyName != null ? (FlightGlobals.GetBodyByName(bodyName)?.displayName ?? bodyName) : bodyName;

        public static void ShowNotification(string msg)
        {
            if (SpaceAgeChronicleSettings.Instance.ShowNotifications)
                ScreenMessages.PostScreenMessage(msg);
        }

        public static void ParseTime(long time, out int y, out int d, out int h, out int m, out int s, bool interval = false, bool parseYears = true)
        {
            if (parseYears)
            {
                y = (int)(time / KSPUtil.dateTimeFormatter.Year);
                time -= y * KSPUtil.dateTimeFormatter.Year;
                if (!interval)
                    y++;
            }
            else y = 0;
            d = (int)time / KSPUtil.dateTimeFormatter.Day;
            time -= d * KSPUtil.dateTimeFormatter.Day;
            h = (int)time / 3600;
            time -= h * 3600;
            m = (int)time / 60;
            s = (int)time - m * 60;
        }

        /// <summary>
        /// Parses UT into a string (e.g. "Y23 D045")
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <param name="showSeconds">If false, seconds will be displayed only if time is less than 1 minute; otherwise always</param>
        /// <returns></returns>
        public static string PrintUT(long time, bool showSeconds = false)
        {
            if (time < 0)
                return "—";
            ParseTime(time, out int y, out int d, out int h, out int m, out int s);
            return showSeconds
                ? Localizer.Format("#SpaceAge_DateTime_Sec", y, d.ToString("D3"), h, m.ToString("D2"), s.ToString("D2"))
                : Localizer.Format("#SpaceAge_DateTime_NoSec", y, d.ToString("D3"), h, m.ToString("D2"));
        }

        /// <summary>
        /// Translates number of seconds into a string of T+[[ddd:]hh:]mm:ss
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string PrintMET(long time)
        {
            string res;
            if (time < 0)
            {
                res = "T-";
                time = -time;
            }
            else res = "T+";
            ParseTime(time, out int y, out int d, out int h, out int m, out int s, true, false);
            if (d > 0)
                res += $"{d:D3}:";
            if (h > 0 || d > 0)
            {
                if (h < 10 && KSPUtil.dateTimeFormatter.Day > KSPUtil.dateTimeFormatter.Hour * 10)
                    res += "0";
                res += $"{h}:";
            }
            if (m < 10)
                res += "0";
            res += $"{m}:{s:D2}";
            return res;
        }

        public static string GetString(this ConfigNode n, string key, string defaultValue = null) => n.HasValue(key) ? n.GetValue(key) : defaultValue;

        public static double GetDouble(this ConfigNode n, string key, double defaultValue = 0) =>
            double.TryParse(n.GetValue(key), out double val) ? val : defaultValue;

        public static int GetInt(this ConfigNode n, string key, int defaultValue = 0) =>
            int.TryParse(n.GetValue(key), out int val) ? val : defaultValue;

        public static long GetLongOrDouble(this ConfigNode n, string key, long defaultValue = 0) =>
            long.TryParse(n.GetValue(key), out long val) ? val : (long)n.GetDouble(key, defaultValue);

        public static bool GetBool(this ConfigNode n, string key, bool defaultValue = false) =>
            bool.TryParse(n.GetValue(key), out bool val) ? val : defaultValue;

        /// <summary>
        /// Write into KSP.log
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        internal static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        {
            if (messageLevel <= Level)
            {
                if (messageLevel == LogLevel.Error)
                    message = $"ERROR: {message}";
                Debug.Log($"[SpaceAge] {message}");
            }
        }
    }
}
