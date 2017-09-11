using UnityEngine;
using KSP.UI.Screens;

namespace SpaceAge
{
    class Core
    {
        public static double VesselCost(Vessel v)
        {
            double c = 0;
            foreach (Part p in v.Parts)
                c += p.protoPartSnapshot.moduleCosts;
            return c;
        }

        public static void ShowNotification(string msg)
        { if (HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().showNotifications) ScreenMessages.PostScreenMessage(msg); }

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
        public static LogLevel Level
        { get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().debugMode ? LogLevel.Debug : LogLevel.Important; } }

        /// <summary>
        /// Write into output_log.txt
        /// </summary>
        /// <param name="message">Text to log</param>
        /// <param name="messageLevel"><see cref="LogLevel"/> of the entry</param>
        public static void Log(string message, LogLevel messageLevel = LogLevel.Debug)
        { if (messageLevel <= Level) Debug.Log("[SpaceAge] " + message); }

        public static bool UseBlizzysToolbar
        { get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().UseBlizzysToolbar; } }

        public static bool NewestFirst
        { get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().newestFirst; } }
    }
}
