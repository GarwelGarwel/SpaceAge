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
    internal enum LogLevel
    {
        None = 0,
        Error,
        Important,
        Debug
    };

    internal static class Core
    {
        /// <summary>
        /// Current <see cref="LogLevel"/>: either Debug or Important
        /// </summary>
        internal static LogLevel Level => SpaceAgeChronicleSettings.Instance.DebugMode ? LogLevel.Debug : LogLevel.Important;

        public static IDateTimeFormatter DateTimeFormatter =>
            SpaceAgeChronicleSettings.Instance.UseStockDateTimeFormat ? KSPUtil.dateTimeFormatter : GarwelDateTimeFormatter.Instance;

        /// <summary>
        /// Determines if events of the given vessel should be tracked
        /// </summary>
        /// <param name="v"></param>
        /// <param name="mustBeActive"></param>
        /// <returns></returns>
        public static bool IsTrackable(this Vessel v, bool mustBeActive) =>
            v != null
            && v.vesselType != VesselType.Debris
            && v.vesselType != VesselType.EVA
            && v.vesselType != VesselType.Flag
            && v.vesselType != VesselType.SpaceObject
            && v.vesselType != VesselType.Unknown
            && (!mustBeActive || v == FlightGlobals.ActiveVessel);

        public static double GetCost(this Vessel v) =>
            v.Parts.Sum(p => p.partInfo.cost + p.GetModuleCosts(0) + p.Resources.Sum(pr => pr.amount * pr.info.unitCost));

        public static double GetMass(this Vessel v) =>
            v.totalMass - v.Parts.Where(part => part.partName == "PotatoRoid" || part.partName == "PotatoComet").Sum(part => part.mass);

        public static bool IsBurning(this Vessel v) => v.FindPartModulesImplementing<ModuleEngines>().Any(module => module.GetCurrentThrust() > 0);

        public static bool IsLandedOrSplashed(this Vessel.Situations situation) =>
            (situation & (Vessel.Situations.LANDED | Vessel.Situations.SPLASHED)) != 0;

        public static string GetBodyDisplayName(string bodyName) =>
            bodyName != null ? (FlightGlobals.GetBodyByName(bodyName)?.displayName ?? bodyName) : bodyName;

        public static void ShowNotification(string msg)
        {
            if (SpaceAgeChronicleSettings.Instance.ShowNotifications)
                ScreenMessages.PostScreenMessage(msg);
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
