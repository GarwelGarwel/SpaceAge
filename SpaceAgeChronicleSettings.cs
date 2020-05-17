using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class SpaceAgeChronicleSettings : GameParameters.CustomParameterNode
    {
        public override string Title => "Chronicle Settings";

        public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;

        public override string Section => "Space Age";

        public override string DisplaySection => Section;

        public override int SectionOrder => 1;

        public override bool HasPresets => false;

        public static SpaceAgeChronicleSettings Instance => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>();

        [GameParameters.CustomParameterUI("Show AppLauncher Button", toolTip = "Show button in the stock AppLauncher bar")]
        public bool ShowAppLauncherButton = true;

        [GameParameters.CustomParameterUI("Show Notifications", toolTip = "Show on-screen notifications when new items are added to the Chronicle")]
        public bool ShowNotifications = false;

        [GameParameters.CustomIntParameterUI("Chronicle Records per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int ChronicleLinesPerPage = 10;

        [GameParameters.CustomParameterUI("Newest First", toolTip = "Show most recent events first in the Chronicle")]
        public bool NewestFirst = true;

        [GameParameters.CustomIntParameterUI("Achievements per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int AchievementsPerPage = 10;

        [GameParameters.CustomParameterUI("Import Stock Achievements", toolTip = "Try to import applicable achievements saved by stock ProgressTracking system. Use when the mod is installed/changed mid-game")]
        public bool ImportStockAchievements = false;

        [GameParameters.CustomParameterUI("Unwarp on Chronicle Events", toolTip = "End time warp when tracked Chronicle events occur")]
        public bool UnwarpOnEvents = false;

        [GameParameters.CustomParameterUI("Display Achievements in the Chronicle", toolTip = "Track and save Space Age achievements (except \"totals\") in the Chronicle")]
        public bool TrackAchievements = true;

        [GameParameters.CustomParameterUI("Track Launch Events", toolTip = "Track and save Launch events in the Chronicle")]
        public bool TrackLaunch = true;

        [GameParameters.CustomParameterUI("Track Reach Space Events", toolTip = "Track and save Reach Space events in the Chronicle")]
        public bool TrackReachSpace = true;

        [GameParameters.CustomParameterUI("Track Orbit Events", toolTip = "Track and save Orbit events in the Chronicle")]
        public bool TrackOrbit = true;

        [GameParameters.CustomParameterUI("Track Reentry Events", toolTip = "Track and save atrmospheric (re)entry events in the Chronicle")]
        public bool TrackReentry = true;

        [GameParameters.CustomParameterUI("Track Docking Events", toolTip = "Track and save Docking events in the Chronicle")]
        public bool TrackDocking = true;

        [GameParameters.CustomParameterUI("Track SOI Change Events", toolTip = "Track and save SOI Change events in the Chronicle")]
        public bool TrackSOIChange = true;

        [GameParameters.CustomParameterUI("Track Landing Events", toolTip = "Track and save Landing events in the Chronicle")]
        public bool TrackLanding = true;

        [GameParameters.CustomFloatParameterUI("Min Jump Duration to Track", toolTip = "Landings after jumps shorter than this will be ignored, in seconds", minValue = 0, maxValue = 120, stepCount = 21)]
        public float MinJumpDuration = 30;

        [GameParameters.CustomParameterUI("Track Recovery Events", toolTip = "Track and save Recovery events in the Chronicle")]
        public bool TrackRecovery = true;

        [GameParameters.CustomParameterUI("Track \"Return From\" Events", toolTip = "Track and save Return From Orbit/Surface events in the Chronicle")]
        public bool TrackReturnFrom = true;

        [GameParameters.CustomParameterUI("Track Destroy Events", toolTip = "Track and save Destroy events in the Chronicle")]
        public bool TrackDestroy = true;

        [GameParameters.CustomParameterUI("Track Death Events", toolTip = "Track and save Death events in the Chronicle")]
        public bool TrackDeath = true;

        [GameParameters.CustomParameterUI("Track Flag Plant Events", toolTip = "Track and save Flag Plant events in the Chronicle")]
        public bool TrackFlagPlant = true;

        [GameParameters.CustomParameterUI("Track Anomaly Discovery Events", toolTip = "Track and save anomaly discovery events in the Chronicle")]
        public bool TrackAnomalyDiscovery = true;

        [GameParameters.CustomParameterUI("Track Facility Upgraded Events", toolTip = "Track and save Facility Upgraded events in the Chronicle")]
        public bool TrackFacilityUpgraded = true;

        [GameParameters.CustomParameterUI("Track Structure Collapsed Events", toolTip = "Track and save Structure Collapsed events in the Chronicle")]
        public bool TrackStructureCollapsed = true;

        [GameParameters.CustomParameterUI("Track Tech Researched Events", toolTip = "Track and save Technology Researched events in the Chronicle")]
        public bool TrackTechnologyResearched = true;

        [GameParameters.CustomFloatParameterUI("Funds per Score Point", toolTip = "How much funds (money) are paid for every point of game score", minValue = 0, maxValue = 10000, stepCount = 100)]
        public float FundsPerScore = 0;

        [GameParameters.CustomFloatParameterUI("Science per Score Point", toolTip = "How much science is gained for every point of game score", minValue = 0, maxValue = 50)]
        public float SciencePerScore = 0;

        [GameParameters.CustomFloatParameterUI("Reputation per Score Point", toolTip = "How many reputation points are gained for every point of game score", minValue = 0, maxValue = 50)]
        public float RepPerScore = 0;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Verbose logging, obligatory for bug submissions")]
        public bool DebugMode = false;

        [GameParameters.CustomParameterUI("Reset Settings", toolTip = "Check and quit to game if you want Space Age settings to be reverted to their default values")]
        public bool ResetSettings = false;

        /// <summary>
        /// Resets settings to default values (ignores config)
        /// </summary>
        internal void Reset()
        {
            ShowAppLauncherButton = true;
            ShowNotifications = false;
            ChronicleLinesPerPage = 10;
            NewestFirst = true;
            AchievementsPerPage = 10;
            ImportStockAchievements = false;
            UnwarpOnEvents = false;
            TrackAchievements = true;
            TrackLaunch = true;
            TrackReachSpace = true;
            TrackOrbit = true;
            TrackReentry = true;
            TrackDocking = true;
            TrackSOIChange = true;
            TrackLanding = true;
            MinJumpDuration = 30;
            TrackRecovery = true;
            TrackReturnFrom = true;
            TrackDestroy = true;
            TrackDeath = true;
            TrackFlagPlant = true;
            TrackAnomalyDiscovery = true;
            TrackFacilityUpgraded = true;
            TrackStructureCollapsed = true;
            TrackTechnologyResearched = true;
            FundsPerScore = 0;
            SciencePerScore = 0;
            RepPerScore = 0;
            ResetSettings = false;
        }

        public SpaceAgeChronicleSettings() => Reset();
    }
}
