using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class SpaceAgeChronicleSettings : GameParameters.CustomParameterNode
    {
        public override string Title
        { get { return "Chronicle Settings"; } }

        public override GameParameters.GameMode GameMode
        { get { return GameParameters.GameMode.ANY; } }

        public override string Section
        { get { return "Space Age"; } }

        public override string DisplaySection
        { get { return Section; } }

        public override int SectionOrder
        { get { return 1; } }

        public override bool HasPresets
        { get { return false; } }

        [GameParameters.CustomParameterUI("Use Blizzy's Toolbar", toolTip = "Show icon in Blizzy's Toolbar, if available, instead of stock AppLauncher")]
        public bool UseBlizzysToolbar = true;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Verbose logging, obligatory for bug submissions")]
        public bool debugMode = true;

        [GameParameters.CustomParameterUI("Show Notifications", toolTip = "Show on-screen notifications when new items added to the Chronicle")]
        public bool showNotifications = false;

        [GameParameters.CustomIntParameterUI("Chronicle Records per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int chronicleLinesPerPage = 10;

        [GameParameters.CustomParameterUI("Newest First", toolTip = "Show most recent events first in the Chronicle")]
        public bool newestFirst = true;

        [GameParameters.CustomIntParameterUI("Achievements per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int achievementsPerPage = 10;

        public static int AchievementsPerPage
        {
            get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().achievementsPerPage; }
            set { HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().achievementsPerPage = value; }
        }

        [GameParameters.CustomParameterUI("Track Launch Events", toolTip = "Track and save Launch events in the Chronicle")]
        public bool trackLaunch = true;

        [GameParameters.CustomParameterUI("Track Orbit Events", toolTip = "Track and save Orbit events in the Chronicle")]
        public bool trackOrbit = false;

        [GameParameters.CustomParameterUI("Track Landing Events", toolTip = "Track and save Landing events in the Chronicle")]
        public bool trackLanding = true;

        [GameParameters.CustomFloatParameterUI("Min Landing Interval", toolTip = "Min time between landings for them to be counted as separate, in seconds", minValue = 0, maxValue = 300, stepCount = 31)]
        public float minLandingInterval = 60;

        public static float MinLandingInterval
        {
            get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().minLandingInterval; }
            set { HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().minLandingInterval = value; }
        }

        [GameParameters.CustomParameterUI("Track Recovery Events", toolTip = "Track and save Recovery events in the Chronicle")]
        public bool trackRecovery = true;

        [GameParameters.CustomParameterUI("Track Destroy Events", toolTip = "Track and save Destroy events in the Chronicle")]
        public bool trackDestroy = true;

        [GameParameters.CustomParameterUI("Track Death Events", toolTip = "Track and save Death events in the Chronicle")]
        public bool trackDeath = true;

        [GameParameters.CustomParameterUI("Track Flag Plant Events", toolTip = "Track and save Flag Plant events in the Chronicle")]
        public bool trackFlagPlant = true;

        [GameParameters.CustomParameterUI("Track Facility Upgraded Events", toolTip = "Track and save Facility Upgraded events in the Chronicle")]
        public bool trackFacilityUpgraded = true;

        [GameParameters.CustomParameterUI("Track Structure Collapsed Events", toolTip = "Track and save Structure Collapsed events in the Chronicle")]
        public bool trackStructureCollapsed = true;

        [GameParameters.CustomParameterUI("Track Tech Researched Events", toolTip = "Track and save Technology Researched events in the Chronicle")]
        public bool trackTechnologyResearched = true;

        [GameParameters.CustomParameterUI("Track SOI Change Events", toolTip = "Track and save SOI Change events in the Chronicle")]
        public bool trackSOIChange = true;
    }
}
