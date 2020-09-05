using KSP.Localization;

namespace SpaceAge
{
    internal class SpaceAgeChronicleSettings : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("#SpaceAge_Settings_ShowAppLauncherButton", toolTip = "#SpaceAge_Settings_ShowAppLauncherButton_desc")]
        public bool ShowAppLauncherButton = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_ShowNotifications", toolTip = "#SpaceAge_Settings_ShowNotifications_desc")]
        public bool ShowNotifications = false;

        [GameParameters.CustomIntParameterUI("#SpaceAge_Settings_ChronicleLinesPerPage", toolTip = "#SpaceAge_Settings_ChronicleLinesPerPage_desc", minValue = 5, maxValue = 25, stepSize = 5)]
        public int ChronicleLinesPerPage = 10;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_NewestFirst", toolTip = "#SpaceAge_Settings_NewestFirst_desc")]
        public bool NewestFirst = true;

        [GameParameters.CustomIntParameterUI("#SpaceAge_Settings_AchievementsPerPage", toolTip = "#SpaceAge_Settings_AchievementsPerPage_desc", minValue = 5, maxValue = 25, stepSize = 5)]
        public int AchievementsPerPage = 10;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_ImportStockAchievements", toolTip = "#SpaceAge_Settings_ImportStockAchievements_desc")]
        public bool ImportStockAchievements = false;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_UnwarpOnEvents", toolTip = "#SpaceAge_Settings_UnwarpOnEvents_desc")]
        public bool UnwarpOnEvents = false;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackAchievements", toolTip = "#SpaceAge_Settings_TrackAchievements_desc")]
        public bool TrackAchievements = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackLaunch", toolTip = "#SpaceAge_Settings_TrackLaunch_desc")]
        public bool TrackLaunch = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackReachSpace", toolTip = "#SpaceAge_Settings_TrackReachSpace_desc")]
        public bool TrackReachSpace = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackBurns", toolTip = "#SpaceAge_Settings_TrackBurns_desc")]
        public bool TrackBurns = true;

        [GameParameters.CustomFloatParameterUI("#SpaceAge_Settings_MinBurnDuration", toolTip = "#SpaceAge_Settings_MinBurnDuration_desc", minValue = 0, maxValue = 60)]
        public int MinBurnDuration = 5;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackOrbit", toolTip = "#SpaceAge_Settings_TrackOrbit_desc")]
        public bool TrackOrbit = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackReentry", toolTip = "#SpaceAge_Settings_TrackReentry_desc")]
        public bool TrackReentry = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackDocking", toolTip = "#SpaceAge_Settings_TrackDocking_desc")]
        public bool TrackDocking = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackSOIChange", toolTip = "#SpaceAge_Settings_TrackSOIChange_desc")]
        public bool TrackSOIChange = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackLanding", toolTip = "#SpaceAge_Settings_TrackLanding_desc")]
        public bool TrackLanding = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackTakeoffs", toolTip = "#SpaceAge_Settings_TrackTakeoffs_desc")]
        public bool TrackTakeoffs = true;

        [GameParameters.CustomFloatParameterUI("#SpaceAge_Settings_MinJumpDuration", toolTip = "#SpaceAge_Settings_MinJumpDuration_desc", minValue = 0, maxValue = 120, stepCount = 21)]
        public float MinJumpDuration = 30;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackRecovery", toolTip = "#SpaceAge_Settings_TrackRecovery_desc")]
        public bool TrackRecovery = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackReturnFrom", toolTip = "#SpaceAge_Settings_TrackReturnFrom_desc")]
        public bool TrackReturnFrom = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackDestroy", toolTip = "#SpaceAge_Settings_TrackDestroy_desc")]
        public bool TrackDestroy = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackDeath", toolTip = "#SpaceAge_Settings_TrackDeath_desc")]
        public bool TrackDeath = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackFlagPlant", toolTip = "#SpaceAge_Settings_TrackFlagPlant_desc")]
        public bool TrackFlagPlant = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackAnomalyDiscovery", toolTip = "#SpaceAge_Settings_TrackAnomalyDiscovery_desc")]
        public bool TrackAnomalyDiscovery = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackFacilityUpgraded", toolTip = "#SpaceAge_Settings_TrackFacilityUpgraded_desc")]
        public bool TrackFacilityUpgraded = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackStructureCollapsed", toolTip = "#SpaceAge_Settings_TrackStructureCollapsed_desc")]
        public bool TrackStructureCollapsed = true;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_TrackTechnologyResearched", toolTip = "#SpaceAge_Settings_TrackTechnologyResearched_desc")]
        public bool TrackTechnologyResearched = true;

        [GameParameters.CustomFloatParameterUI("#SpaceAge_Settings_FundsPerScore", toolTip = "#SpaceAge_Settings_FundsPerScore_desc", minValue = 0, maxValue = 10000, stepCount = 101)]
        public float FundsPerScore = 0;

        [GameParameters.CustomFloatParameterUI("#SpaceAge_Settings_SciencePerScore", toolTip = "#SpaceAge_Settings_SciencePerScore_desc", minValue = 0, maxValue = 50)]
        public float SciencePerScore = 0;

        [GameParameters.CustomFloatParameterUI("#SpaceAge_Settings_RepPerScore", toolTip = "#SpaceAge_Settings_RepPerScore_desc", minValue = 0, maxValue = 50)]
        public float RepPerScore = 0;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_DebugMode", toolTip = "#SpaceAge_Settings_DebugMode_desc")]
        public bool DebugMode = false;

        [GameParameters.CustomParameterUI("#SpaceAge_Settings_ResetSettings", toolTip = "#SpaceAge_Settings_ResetSettings_desc")]
        public bool ResetSettings = false;

        public SpaceAgeChronicleSettings() => Reset();

        public static SpaceAgeChronicleSettings Instance => HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>();

        public override string Title => Localizer.Format("#SpaceAge_Settings_Title");

        public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;

        public override string Section => "Space Age";

        public override string DisplaySection => Section;

        public override int SectionOrder => 1;

        public override bool HasPresets => false;

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
            TrackBurns = true;
            MinBurnDuration = 5;
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
    }
}
