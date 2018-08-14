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

        [GameParameters.CustomParameterUI("Use Blizzy's Toolbar", toolTip = "Show icon in Blizzy's Toolbar, if available, instead of stock AppLauncher")]
        public bool UseBlizzysToolbar = true;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Verbose logging, obligatory for bug submissions")]
        public bool debugMode = false;

        [GameParameters.CustomParameterUI("Show Notifications", toolTip = "Show on-screen notifications when new items are added to the Chronicle")]
        public bool showNotifications = false;

        [GameParameters.CustomIntParameterUI("Chronicle Records per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int chronicleLinesPerPage = 10;

        [GameParameters.CustomParameterUI("Newest First", toolTip = "Show most recent events first in the Chronicle")]
        public bool newestFirst = true;

        [GameParameters.CustomIntParameterUI("Achievements per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int achievementsPerPage = 10;

        public static int AchievementsPerPage
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().achievementsPerPage;
            set => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().achievementsPerPage = value;
        }

        [GameParameters.CustomParameterUI("Import Stock Achievements", toolTip = "Try to import applicable achievements saved by stock ProgressTracking system. Use when the mod is installed/changed mid-game")]
        public bool importStockAchievements = false;

        [GameParameters.CustomParameterUI("Unwarp on Chronicle Events", toolTip = "End time warp when tracked Chronicle events occur")]
        public bool unwarpOnEvents = false;

        [GameParameters.CustomParameterUI("Display Achievements in the Chronicle", toolTip = "Track and save Space Age achievements (except \"totals\") in the Chronicle")]
        public bool trackAchievements = true;

        [GameParameters.CustomParameterUI("Track Launch Events", toolTip = "Track and save Launch events in the Chronicle")]
        public bool trackLaunch = true;

        [GameParameters.CustomParameterUI("Track Reach Space Events", toolTip = "Track and save Reach Space events in the Chronicle")]
        public bool trackReachSpace = true;

        [GameParameters.CustomParameterUI("Track Orbit Events", toolTip = "Track and save Orbit events in the Chronicle")]
        public bool trackOrbit = true;

        [GameParameters.CustomParameterUI("Track Reentry Events", toolTip = "Track and save atrmospheric (re)entry events in the Chronicle")]
        public bool trackReentry = true;

        [GameParameters.CustomParameterUI("Track Docking Events", toolTip = "Track and save Docking events in the Chronicle")]
        public bool trackDocking = true;

        [GameParameters.CustomParameterUI("Track SOI Change Events", toolTip = "Track and save SOI Change events in the Chronicle")]
        public bool trackSOIChange = true;

        [GameParameters.CustomParameterUI("Track Landing Events", toolTip = "Track and save Landing events in the Chronicle")]
        public bool trackLanding = true;

        [GameParameters.CustomFloatParameterUI("Min Landing Interval", toolTip = "Min time between landings for them to be counted as separate, in seconds", minValue = 0, maxValue = 300, stepCount = 31)]
        public float minLandingInterval = 60;

        public static float MinLandingInterval
        {
            get => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().minLandingInterval;
            set => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().minLandingInterval = value;
        }

        [GameParameters.CustomParameterUI("Track Recovery Events", toolTip = "Track and save Recovery events in the Chronicle")]
        public bool trackRecovery = true;

        [GameParameters.CustomParameterUI("Track \"Return From\" Events", toolTip = "Track and save Return From Orbit/Surface events in the Chronicle")]
        public bool trackReturnFrom = true;

        [GameParameters.CustomParameterUI("Track Destroy Events", toolTip = "Track and save Destroy events in the Chronicle")]
        public bool trackDestroy = true;

        [GameParameters.CustomParameterUI("Track Death Events", toolTip = "Track and save Death events in the Chronicle")]
        public bool trackDeath = true;

        [GameParameters.CustomParameterUI("Track Flag Plant Events", toolTip = "Track and save Flag Plant events in the Chronicle")]
        public bool trackFlagPlant = true;

        [GameParameters.CustomParameterUI("Track Anomaly Discovery Events", toolTip = "Track and save anomaly discovery events in the Chronicle")]
        public bool trackAnomalyDiscovery = true;

        [GameParameters.CustomParameterUI("Track Facility Upgraded Events", toolTip = "Track and save Facility Upgraded events in the Chronicle")]
        public bool trackFacilityUpgraded = true;

        [GameParameters.CustomParameterUI("Track Structure Collapsed Events", toolTip = "Track and save Structure Collapsed events in the Chronicle")]
        public bool trackStructureCollapsed = true;

        [GameParameters.CustomParameterUI("Track Tech Researched Events", toolTip = "Track and save Technology Researched events in the Chronicle")]
        public bool trackTechnologyResearched = true;
    }
}
