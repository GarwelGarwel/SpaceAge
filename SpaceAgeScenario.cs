using KSP.Localization;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.IO;
using UniLinq;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceAge
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    public class SpaceAgeScenario : ScenarioModule
    {
        enum Tab { Chronicle, Achievements, Score };

        enum TimeFormat { UT, MET };

        const float windowWidth = 600;

        static List<ProtoAchievement> protoAchievements;
        List<ChronicleEvent> chronicle = new List<ChronicleEvent>(), displayChronicle;
        Dictionary<string, VesselRecord> vessels = new Dictionary<string, VesselRecord>();
        Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();
        List<Achievement> scoreAchievements = new List<Achievement>();
        List<string> scoreRecordNames = new List<string>();
        List<string> scoreBodies = new List<string>();
        double score;
        double funds;

        VesselRecord logVessel = null;
        TimeFormat logTimeFormat = TimeFormat.UT;

        IButton toolbarButton;
        ApplicationLauncherButton appLauncherButton;

        Tab currentTab = Tab.Chronicle;
        int[] page = new int[3] { 1, 1, 1 };
        int chroniclePage = 0;
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;
        string textInput = "";
        string searchTerm = "";

        #region LIFE CYCLE

        double nextUpdate = 0;

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start", LogLevel.Important);

            displayChronicle = chronicle;

            // Adding event handlers
            GameEvents.onGameNewStart.Add(ResetSettings);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.VesselSituation.onReachSpace.Add(OnReachSpace);
            GameEvents.onStageActivate.Add(onStageActivate);
            GameEvents.onVesselRecovered.Add(OnVesselRecovery);
            GameEvents.VesselSituation.onReturnFromOrbit.Add(OnReturnFromOrbit);
            GameEvents.VesselSituation.onReturnFromSurface.Add(OnReturnFromSurface);
            GameEvents.onVesselWillDestroy.Add(OnVesselDestroy);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
            GameEvents.onFlagPlant.Add(OnFlagPlanted);
            GameEvents.OnKSCFacilityUpgraded.Add(OnFacilityUpgraded);
            GameEvents.OnKSCStructureCollapsed.Add(OnStructureCollapsed);
            GameEvents.OnTechnologyResearched.Add(OnTechnologyResearched);
            GameEvents.onVesselSOIChanged.Add(OnSOIChanged);
            GameEvents.onVesselSituationChange.Add(OnSituationChanged);
            GameEvents.onVesselDocking.Add(OnVesselDocking);
            GameEvents.onVesselsUndocking.Add(OnVesselsUndocking);
            GameEvents.OnFundsChanged.Add(OnFundsChanged);
            GameEvents.OnProgressComplete.Add(OnProgressCompleted);

            // Adding buttons to AppLauncher and Toolbar
            if (SpaceAgeChronicleSettings.Instance.ShowAppLauncherButton)
                RegisterAppLauncherButton();
            if (ToolbarManager.ToolbarAvailable)
            {
                Core.Log("Registering Toolbar button...");
                toolbarButton = ToolbarManager.Instance.add("SpaceAge", "SpaceAge");
                toolbarButton.Text = "Space Age";
                toolbarButton.TexturePath = "SpaceAge/icon24";
                toolbarButton.ToolTip = "Space Age";
                toolbarButton.OnClick += e =>
                {
                    if (window == null)
                        DisplayData();
                    else UndisplayData();
                };
            }

            funds = (Funding.Instance != null) ? Funding.Instance.Funds : double.NaN;

            InitializeDatabase();
            if (SpaceAgeChronicleSettings.Instance.ImportStockAchievements)
                ParseProgressTracking();
        }

        public void OnDisable()
        {
            Core.Log("SpaceAgeScenario.OnDisable");
            UndisplayData();

            // Removing event handlers
            GameEvents.onGameNewStart.Remove(ResetSettings);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.VesselSituation.onReachSpace.Remove(OnReachSpace);
            GameEvents.onStageActivate.Remove(onStageActivate);
            GameEvents.onVesselRecovered.Remove(OnVesselRecovery);
            GameEvents.VesselSituation.onReturnFromOrbit.Remove(OnReturnFromOrbit);
            GameEvents.VesselSituation.onReturnFromSurface.Remove(OnReturnFromSurface);
            GameEvents.onVesselWillDestroy.Remove(OnVesselDestroy);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);
            GameEvents.onFlagPlant.Remove(OnFlagPlanted);
            GameEvents.OnKSCFacilityUpgraded.Remove(OnFacilityUpgraded);
            GameEvents.OnKSCStructureCollapsed.Remove(OnStructureCollapsed);
            GameEvents.OnTechnologyResearched.Remove(OnTechnologyResearched);
            GameEvents.onVesselSOIChanged.Remove(OnSOIChanged);
            GameEvents.onVesselSituationChange.Remove(OnSituationChanged);
            GameEvents.onVesselDocking.Remove(OnVesselDocking);
            GameEvents.onVesselsUndocking.Remove(OnVesselsUndocking);
            GameEvents.OnFundsChanged.Remove(OnFundsChanged);
            GameEvents.OnProgressComplete.Remove(OnProgressCompleted);

            // Removing Toolbar & AppLauncher buttons
            if (toolbarButton != null)
                toolbarButton.Destroy();
            UnregisterAppLauncherButton();
        }

        public override void OnSave(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnSave");
            ConfigNode n = new ConfigNode("CHRONICLE");

            n.AddValue("logTimeFormat", logTimeFormat);

            foreach (ChronicleEvent e in chronicle)
                n.AddNode(e.ConfigNode);
            node.AddNode(n);
            Core.Log($"{n.CountNodes} events saved.");

            n = new ConfigNode("VESSELS");
            foreach (VesselRecord vessel in vessels.Values)
                n.AddNode(vessel.ConfigNode);
            node.AddNode(n);
            Core.Log($"{n.CountNodes} vessels saved.");

            n = new ConfigNode("ACHIEVEMENTS");
            foreach (Achievement a in achievements.Values)
                n.AddNode(a.ConfigNode);
            node.AddNode(n);
            Core.Log($"{n.CountNodes} achievements saved.");
        }

        public override void OnLoad(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnLoad");
            InitializeDatabase();

            if (node.HasValue("logTimeFormat"))
                Enum.TryParse<TimeFormat>(node.GetValue("logTimeFormat"), out logTimeFormat);

            chronicle.Clear();
            if (node.HasNode("CHRONICLE"))
            {
                ConfigNode[] chronicleNodes = node.GetNode("CHRONICLE").GetNodes("EVENT");
                Core.Log($"{chronicleNodes.Length} nodes found in CHRONICLE.");
                chronicle.AddRange(chronicleNodes.Select(n => new ChronicleEvent(n)));
            }
            displayChronicle = chronicle;

            vessels.Clear();
            if (node.HasNode("VESSELS"))
            {
                ConfigNode[] vesselsNodes = node.GetNode("VESSELS").GetNodes("VESSEL");
                Core.Log($"{vesselsNodes.Length} nodes found in VESSELS.");
                foreach (ConfigNode n in vesselsNodes)
                    AddVesselRecord(new VesselRecord(n));
            }

            achievements.Clear();
            if (node.HasNode("ACHIEVEMENTS"))
            {
                ConfigNode[] achievmentsNodes = node.GetNode("ACHIEVEMENTS").GetNodes("ACHIEVEMENT");
                Core.Log($"{achievmentsNodes.Length} nodes found in ACHIEVEMENTS.");
                double score = 0;
                foreach (ConfigNode n in achievmentsNodes)
                    try
                    {
                        Achievement a = new Achievement(n);
                        achievements.Add(a.FullName, a);
                        if (a.Proto.Score > 0)
                            Core.Log($"{a.FullDisplayValue}: {a.Score} points");
                        score += a.Score;
                    }
                    catch (ArgumentException e) { Core.Log(e.Message); }
                Core.Log($"Total score: {score}");
            }

            UpdateScoreAchievements();
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            double time = Planetarium.GetUniversalTime();

            // Only do checks once per 0.5 s
            if (time < nextUpdate)
                return;
            nextUpdate = time + 0.5;

            CheckTakeoff(false);
            CheckBurn();
        }

        void InitializeDatabase()
        {
            if (protoAchievements != null)
                return;
            protoAchievements = new List<ProtoAchievement>(GameDatabase.Instance.GetConfigNodes("PROTOACHIEVEMENT").Select(n => new ProtoAchievement(n)));
            Core.Log($"protoAchievements contains {protoAchievements.Count} records.");
        }

        /// <summary>
        /// Applying default settings, using config files if exist
        /// </summary>
        void ResetSettings()
        {
            Core.Log("ResetSettings", LogLevel.Important);
            SpaceAgeChronicleSettings.Instance.Reset();
            if (GameDatabase.Instance.ExistsConfigNode("SPACEAGE_CONFIG"))
            {
                ConfigNode config = GameDatabase.Instance.GetConfigNode("SPACEAGE_CONFIG");
                Core.Log($"SPACEAGE_CONFIG: {config}", LogLevel.Important);
                SpaceAgeChronicleSettings.Instance.FundsPerScore = (float)config.GetDouble("fundsPerScore", SpaceAgeChronicleSettings.Instance.FundsPerScore);
                SpaceAgeChronicleSettings.Instance.SciencePerScore = (float)config.GetDouble("sciencePerScore", SpaceAgeChronicleSettings.Instance.SciencePerScore);
                SpaceAgeChronicleSettings.Instance.RepPerScore = (float)config.GetDouble("repPerScore", SpaceAgeChronicleSettings.Instance.RepPerScore);
            }
            else Core.Log("SPACEAGE_CONFIG node not found.", LogLevel.Important);
        }

        /// <summary>
        /// Check if settings need to be reset; show or hide AppLauncher button if the settings have changed
        /// </summary>
        void OnGameSettingsApplied()
        {
            Core.Log("OnGameSettingsApplied", LogLevel.Important);
            if (SpaceAgeChronicleSettings.Instance.ResetSettings)
            {
                ResetSettings();
                ScreenMessages.PostScreenMessage(Localizer.Format("#SpaceAge_UI_SettingsReset"));
            }
            if (SpaceAgeChronicleSettings.Instance.ShowAppLauncherButton && appLauncherButton == null)
                RegisterAppLauncherButton();
            if (!SpaceAgeChronicleSettings.Instance.ShowAppLauncherButton)
                UnregisterAppLauncherButton();
            if (!SpaceAgeChronicleSettings.Instance.TrackBurns)
                burnStarted = double.NaN;
        }

        #endregion LIFE CYCLE

        #region LISTS & RECORDS

        public void AddChronicleEvent(ChronicleEvent e)
        {
            Core.ShowNotification(Localizer.Format("#SpaceAge_UI_EventDetected", e.Type));
            if (SpaceAgeChronicleSettings.Instance.UnwarpOnEvents && (TimeWarp.CurrentRateIndex != 0))
                TimeWarp.SetRate(0, true, !SpaceAgeChronicleSettings.Instance.ShowNotifications);
            chronicle.Add(e);
            foreach (string id in e.VesselIds)
                AddVesselRecord(new VesselRecord(id));
            Invalidate();
        }

        public void AddVesselRecord(VesselRecord vesselRecord)
        {
            if (!string.IsNullOrEmpty(vesselRecord.Id) && !vessels.ContainsKey(vesselRecord.Id))
                vessels.Add(vesselRecord.Id, vesselRecord);
        }

        public void DeleteUnusedVesselRecords()
        {
            foreach (string id in vessels.Keys.Where(id => !chronicle.Exists(ev => ev.HasVesselId(id))).ToList())
                vessels.Remove(id);
        }

        #endregion LISTS & RECORDS

        #region ACHIEVEMENTS METHODS

        int achievementsImported = 0;

        public static ProtoAchievement FindProtoAchievement(string name) => protoAchievements.Find(pa => pa.Name == name);

        public Achievement FindAchievement(string fullname) => achievements.ContainsKey(fullname) ? achievements[fullname] : null;

        void ParseProgressNodes(ConfigNode node, CelestialBody body)
        {
            Core.Log($"{node.name} config node contains {node.CountNodes} sub-nodes.");
            Achievement a;
            foreach (ProtoAchievement pa in protoAchievements
                .Where(pa => pa.StockSynonym != null && pa.StockSynonym.Length != 0 && node.HasNode(pa.StockSynonym)))
            {
                ConfigNode n = node.GetNode(pa.StockSynonym);
                Core.Log($"{pa.StockSynonym} node found for {pa.Name}.");
                Core.Log(n.ToString());
                a = new SpaceAge.Achievement(pa, body);
                if (n.HasValue("completed"))
                    a.Time = (long)n.GetDouble("completed");
                else if (n.HasValue("completedManned"))
                    a.Time = n.GetLongOrDouble("completedManned");
                else if (!pa.CrewedOnly && n.HasValue("completedUnmanned"))
                    a.Time = n.GetLongOrDouble("completedUnmanned");
                else
                {
                    Core.Log("Time value not found, achievement has not been completed.");
                    continue;
                }
                Core.Log($"Found candidate achievement: {KSPUtil.PrintDateCompact(a.Time, true)} {a.Title}");
                if (CheckAchievement(a))
                    achievementsImported++;
            }
        }

        void ParseProgressTracking()
        {
            ConfigNode trackingNode = HighLogic.CurrentGame.scenarios.Find(psm => psm.moduleName == "ProgressTracking").GetData();

            if (trackingNode == null)
            {
                Core.Log("ProgressTracking scenario not found!", LogLevel.Important);
                return;
            }

            if (trackingNode.HasNode("Progress"))
                trackingNode = trackingNode.GetNode("Progress");
            else
            {
                Core.Log("ProgressTracking scenario does not contain Progress node!", LogLevel.Important);
                Core.Log(trackingNode.ToString());
                return;
            }

            achievementsImported = 0;
            ParseProgressNodes(trackingNode, null);
            foreach (CelestialBody b in FlightGlobals.Bodies.Where(b => trackingNode.HasNode(b.name)))
                ParseProgressNodes(trackingNode.GetNode(b.name), b);

            if (achievementsImported > 0)
            {
                MessageSystem.Instance.AddMessage(new MessageSystem.Message(
                    Localizer.Format("#SpaceAge_UI_AchievementsImportTitle"),
                    Localizer.Format("#SpaceAge_UI_AchievementsImportMessage", achievementsImported),
                    MessageSystemButton.MessageButtonColor.YELLOW,
                    MessageSystemButton.ButtonIcons.MESSAGE));
                UpdateScoreAchievements();
            }
        }

        bool CheckAchievement(Achievement ach)
        {
            if (ach.Register(FindAchievement(ach.FullName)))
            {
                achievements[ach.FullName] = ach;
                return true;
            }
            return false;
        }

        void CheckAchievements(string ev, CelestialBody body = null, Vessel vessel = null, double value = 0, string hero = null)
        {
            if (ev == null)
                return;

            Core.Log($"CheckAchievements('{ev}', body = '{body?.name}', vessel = '{vessel?.vesselName}', value = {value}, hero = '{hero ?? "null"}')");

            bool scored = false;
            foreach (ProtoAchievement pa in protoAchievements.Where(pa => pa.OnEvents.Contains(ev)))
            {
                string msg = "";
                Achievement ach = new Achievement(pa, body, vessel, value, hero);
                if (CheckAchievement(ach))
                {
                    if (ach.Proto.Score > 0)
                    {
                        scored = true;
                        double score = ach.Score;
                        msg = $"\r\n{Localizer.Format("#SpaceAge_PS_PointsAdded", score)}";
                        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                        {
                            double f = score * SpaceAgeChronicleSettings.Instance.FundsPerScore;
                            if (f != 0)
                            {
                                Core.Log($"Adding {f} funds.");
                                Funding.Instance.AddFunds(f, TransactionReasons.Progression);
                                msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Funds", f.ToString("N0"))}";
                            }
                        }
                        if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER) || (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
                        {
                            float s = (float)score * SpaceAgeChronicleSettings.Instance.SciencePerScore;
                            if (s != 0)
                            {
                                Core.Log($"Adding {s} science.");
                                ResearchAndDevelopment.Instance.AddScience(s, TransactionReasons.Progression);
                                msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Science", s.ToString("N1"))}";
                            }
                        }
                        float r = (float)score * SpaceAgeChronicleSettings.Instance.RepPerScore;
                        if (r != 0)
                        {
                            Core.Log($"Adding {r} rep.");
                            Reputation.Instance.AddReputation(r, TransactionReasons.Progression);
                            msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Reputation", r.ToString("N0"))}";
                        }
                    }

                    if (pa.HasTime)
                    {
                        Core.Log($"{ach.FullName} completed.");
                        if (SpaceAgeChronicleSettings.Instance.TrackAchievements)
                            AddChronicleEvent(new ChronicleEvent("Achievement", "title", ach.Title, "value", ach.ShortDisplayValue, vessel));
                        MessageSystem.Instance.AddMessage(new MessageSystem.Message(
                            Localizer.Format("#SpaceAge_PS_Title"),
                            Localizer.Format("#SpaceAge_PS_AchievementCompleted", ach.Title) + msg,
                            MessageSystemButton.MessageButtonColor.YELLOW,
                            MessageSystemButton.ButtonIcons.ACHIEVE));
                    }
                }
            }
            if (scored)
                UpdateScoreAchievements();
        }

        void CheckAchievements(string ev, Vessel v) => CheckAchievements(ev, v.mainBody, v);

        void CheckAchievements(string ev, double v) => CheckAchievements(ev, null, null, v);

        void CheckAchievements(string ev, string hero) => CheckAchievements(ev, null, null, 0, hero);

        #endregion ACHIEVEMENTS METHODS

        #region UI METHODS

        List<Achievement> SortedAchievements
        {
            get
            {
                List<Achievement> res = new List<Achievement>(protoAchievements
                    .Where(pa => !pa.IsBodySpecific && achievements.ContainsKey(pa.Name))
                    .Select(pa => FindAchievement(pa.Name)));
                foreach (CelestialBody b in FlightGlobals.Bodies)
                    res.AddRange(protoAchievements
                        .Where(pa => pa.IsBodySpecific && achievements.ContainsKey(Achievement.GetFullName(pa.Name, b.name)))
                        .Select(pa => FindAchievement(Achievement.GetFullName(pa.Name, b.name))));
                return res;
            }
        }

        int Page
        {
            get => page[(int)currentTab];
            set => page[(int)currentTab] = value;
        }

        int LinesPerPage =>
            (currentTab == Tab.Chronicle) ? SpaceAgeChronicleSettings.Instance.ChronicleLinesPerPage : SpaceAgeChronicleSettings.Instance.AchievementsPerPage;

        int PageCount
        {
            get
            {
                int itemsNum = 0;
                switch (currentTab)
                {
                    case Tab.Chronicle:
                        itemsNum = displayChronicle.Count;
                        break;

                    case Tab.Achievements:
                        itemsNum = achievements.Count;
                        break;

                    case Tab.Score:
                        itemsNum = scoreBodies.Count;
                        break;
                }
                return (int)Math.Ceiling((double)itemsNum / LinesPerPage);
            }
        }

        public void DisplayData()
        {
            Core.Log("DisplayData", LogLevel.Important);
            CheckTakeoff(false);

            if (currentTab == Tab.Chronicle)
            {
                if (logVessel != null)
                {
                    displayChronicle = chronicle.FindAll(ev => ev.HasVesselId(logVessel.Id));
                    Core.Log($"Found {displayChronicle.Count} ship log records for {logVessel.Name}.");
                }
                else
                {
                    displayChronicle = chronicle.FindAll(ev => !ev.LogOnly);
                    Core.Log($"Found {displayChronicle.Count} chronicle records.");
                }
                if (searchTerm.Length != 0)
                {
                    string searchTermUppercase = searchTerm.ToUpperInvariant();
                    displayChronicle = displayChronicle.FindAll(ev => ev.Description.ToUpperInvariant().Contains(searchTermUppercase));
                    Core.Log($"Filtered {displayChronicle.Count} search results for '{searchTerm}'.");
                }
            }

            List<DialogGUIBase> grid;
            DialogGUIBase windowContent = null;

            if (Page > PageCount)
                Page = PageCount;
            if (PageCount == 0)
                Page = 1;
            int startingIndex = (Page - 1) * LinesPerPage;

            switch (currentTab)
            {
                case Tab.Chronicle:
                    grid = new List<DialogGUIBase>(LinesPerPage);
                    Core.Log($"Displaying events {startingIndex + 1} to {Math.Min(startingIndex + LinesPerPage, displayChronicle.Count)}...");
                    for (int i = startingIndex; i < Math.Min(startingIndex + LinesPerPage, displayChronicle.Count); i++)
                    {
                        ChronicleEvent ce = displayChronicle[ChronicleIndex(i)];
                        grid.Add(
                            new DialogGUIHorizontalLayout(
                                new DialogGUILabel($"<color=\"white\">{((logTimeFormat == TimeFormat.MET && logVessel != null) ? Core.PrintMET(ce.Time - logVessel.LaunchTime) : Core.PrintUT(ce.Time))}</color>", 90),
                                new DialogGUILabel(ce.Description, true),
                                ce.HasVesselId() ? new DialogGUIButton<ChronicleEvent>(Localizer.Format("#SpaceAge_UI_LogBtn"), ShowShipLog, ce, false) : new DialogGUIBase(),
                                new DialogGUIButton<int>("x", DeleteChronicleItem, ChronicleIndex(i))));
                    }
                    windowContent = new DialogGUIVerticalLayout(
                        logVessel != null
                        ? new DialogGUIHorizontalLayout(
                            TextAnchor.MiddleCenter,
                            new DialogGUIButton(logTimeFormat == TimeFormat.UT ? Localizer.Format("#SpaceAge_UI_UT") : Localizer.Format("#SpaceAge_UI_MET"), SwitchTimeFormat, false),
                            new DialogGUILabel($"<align=\"center\"><b>{Localizer.Format("#SpaceAge_UI_LogTitle", logVessel.Name)}</b>\n{Localizer.Format("#SpaceAge_UI_ShipInfo", Core.PrintUT(logVessel.LaunchTime, true))}</align>", true),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Back"), HideShipLog, false))
                        : new DialogGUIBase(),
                        new DialogGUIVerticalLayout(windowWidth - 10, 0, 5, new RectOffset(5, 5, 0, 0), TextAnchor.UpperLeft, grid.ToArray()),
                        HighLogic.LoadedSceneIsFlight
                        ? new DialogGUIBase()
                        : new DialogGUIHorizontalLayout(
                            windowWidth - 20,
                            10,
                            new DialogGUITextInput(textInput, false, 100, s => textInput = s),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Find"), Find),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Add"), AddCustomChronicleEvent),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Export"), ExportChronicle)));
                    break;

                case Tab.Achievements:
                    grid = new List<DialogGUIBase>(LinesPerPage * 3);
                    Core.Log($"Displaying achievements starting from {startingIndex} out of {achievements.Count}...");
                    List<Achievement> achList = SortedAchievements;
                    if ((achievements.Count == 0) || (achList.Count == 0))
                    {
                        Core.Log($"Can't display Achievements tabs. There are {achievements.Count} achievements and {achList.Count} protoachievements.", LogLevel.Error);
                        windowContent = new DialogGUILabel($"<align=\"center\">{Localizer.Format("#SpaceAge_UI_NoAchievements")}</align>", true);
                        break;
                    }
                    string body = null;
                    foreach (Achievement a in achList.GetRange(startingIndex, Math.Min(LinesPerPage, achievements.Count - startingIndex)))
                    {
                        // Achievement for a new body => display the body's name on a new line
                        if ((a.Body != body) && (a.Body.Length != 0))
                        {
                            body = a.Body;
                            grid.Add(new DialogGUILabel("", true));
                            grid.Add(new DialogGUILabel($"<align=\"center\"><color=\"white\"><b>{Localizer.Format("<<1>>", Core.GetBodyDisplayName(body))}</b></color></align>", true));
                            grid.Add(new DialogGUILabel("", true));
                        }
                        DisplayAchievement(a, grid);
                    }
                    windowContent = new DialogGUIGridLayout(
                        new RectOffset(5, 5, 0, 0),
                        new Vector2((windowWidth - 10) / 3 - 3, 20),
                        new Vector2(5, 5),
                        GridLayoutGroup.Corner.UpperLeft,
                        GridLayoutGroup.Axis.Horizontal,
                        TextAnchor.MiddleLeft,
                        GridLayoutGroup.Constraint.FixedColumnCount,
                        3,
                        grid.ToArray());
                    break;

                case Tab.Score:
                    Core.Log($"Displaying score bodies from {startingIndex} out of {scoreBodies.Count}...");
                    if (scoreAchievements.Count == 0)
                    {
                        windowContent = new DialogGUILabel($"<align=\"center\">{Localizer.Format("#SpaceAge_UI_NoScore")}</align>", true);
                        break;
                    }
                    grid = new List<DialogGUIBase>((1 + Math.Min(LinesPerPage, scoreBodies.Count)) * (1 + scoreRecordNames.Count));
                    grid.Add(new DialogGUILabel($"<color=\"white\">{Localizer.Format("#SpaceAge_UI_Body")}</color>"));
                    grid.AddRange(scoreRecordNames.Select(srn => new DialogGUILabel($"<color=\"white\">{srn}</color>")));
                    for (int i = startingIndex; i < Math.Min(startingIndex + LinesPerPage, scoreBodies.Count); i++)
                    {
                        grid.Add(new DialogGUILabel($"<color=\"white\">{Localizer.Format("<<1>>", Core.GetBodyDisplayName(scoreBodies[i]))}</color>"));
                        foreach (string srn in scoreRecordNames)
                        {
                            double s = 0;
                            bool manned = false;
                            foreach (Achievement a in scoreAchievements.Where(a
                                => a.Proto.ScoreName == srn
                                && (a.Body == scoreBodies[i] || (!a.Proto.IsBodySpecific && scoreBodies[i] == FlightGlobals.GetHomeBodyName()))))
                            {
                                s += a.Score;
                                if (a.Proto.CrewedOnly)
                                    manned = true;
                            }
                            string scoreIndicator;
                            if (s > 0)
                                if (manned)
                                    scoreIndicator = $"<color=\"green\">{Localizer.Format("#SpaceAge_UI_ScoreManned", s)}";
                                else scoreIndicator = $"<color=\"yellow\">{Localizer.Format("#SpaceAge_UI_ScoreUnmanned", s)}";
                            else scoreIndicator = $"<color=\"white\">{Localizer.Format("#SpaceAge_UI_ScoreNone")}";
                            grid.Add(new DialogGUILabel($"{scoreIndicator}</color>"));
                        }
                    }
                    windowContent = new DialogGUIVerticalLayout(true, true, 5, new RectOffset(5, 5, 0, 0), TextAnchor.MiddleLeft,
                        new DialogGUIGridLayout(
                            new RectOffset(0, 0, 0, 0),
                            new Vector2((windowWidth - 10) / (scoreRecordNames.Count + 1) - 5, 20),
                            new Vector2(5, 5),
                            GridLayoutGroup.Corner.UpperLeft,
                            GridLayoutGroup.Axis.Horizontal,
                            TextAnchor.MiddleLeft,
                            GridLayoutGroup.Constraint.FixedColumnCount,
                            scoreRecordNames.Count + 1,
                            grid.ToArray()),
                        new DialogGUILabel($"<color=\"white\"><b>{Localizer.Format("#SpaceAge_UI_TotalScore", score.ToString("N0"))}</b></color>"));
                    break;
            }

            window = PopupDialog.SpawnPopupDialog(
                new Vector2(1, 1),
                new Vector2(1, 1),
                new MultiOptionDialog(
                    "Space Age",
                    "",
                    "Space Age",
                    HighLogic.UISkin,
                    windowPosition,
                    new DialogGUIHorizontalLayout(
                        true,
                        false,
                        new DialogGUIButton<Tab>(Localizer.Format("#SpaceAge_UI_Chronicle"), SelectTab, Tab.Chronicle, () => currentTab != Tab.Chronicle, true),
                        new DialogGUIButton<Tab>(Localizer.Format("#SpaceAge_UI_Achievements"), SelectTab, Tab.Achievements, () => (currentTab != Tab.Achievements) && (achievements.Count > 0), true),
                        new DialogGUIButton<Tab>(Localizer.Format("#SpaceAge_UI_Score"), SelectTab, Tab.Score, () => currentTab != Tab.Score, true)),
                    PageCount > 1
                    ? new DialogGUIHorizontalLayout(
                        true,
                        false,
                        new DialogGUIButton("<<", FirstPage, () => Page > 1, false),
                        new DialogGUIButton("<", PageUp, () => Page > 1, false),
                        new DialogGUIHorizontalLayout(TextAnchor.LowerCenter, new DialogGUILabel($"{Page}/{PageCount}")),
                        new DialogGUIButton(">", PageDown, () => Page < PageCount, false),
                        new DialogGUIButton(">>", LastPage, () => Page < PageCount, false))
                    : new DialogGUIHorizontalLayout(),
                    windowContent),
                false,
                HighLogic.UISkin,
                false);
        }

        public void UndisplayData()
        {
            if (window != null)
            {
                Vector3 v = window.RTrf.position;
                windowPosition = new Rect(v.x / Screen.width + 0.5f, v.y / Screen.height + 0.5f, windowWidth, 50);
                window.Dismiss();
            }
        }

        public void Invalidate()
        {
            if (window != null)
            {
                UndisplayData();
                DisplayData();
            }
        }

        public void PageUp()
        {
            if (Page > 1)
                Page--;
            Invalidate();
        }

        public void FirstPage()
        {
            Page = 1;
            Invalidate();
        }

        public void PageDown()
        {
            if (Page < PageCount)
                Page++;
            Invalidate();
        }

        public void LastPage()
        {
            Page = PageCount;
            Invalidate();
        }

        public void SwitchTimeFormat()
        {
            if (logTimeFormat == TimeFormat.UT)
                logTimeFormat = TimeFormat.MET;
            else logTimeFormat = TimeFormat.UT;
            Invalidate();
        }

        public void ShowShipLog(ChronicleEvent ev)
        {
            if (ev == null)
            {
                Core.Log("ShowShipLog: ev is null.", LogLevel.Error);
                return;
            }
            Core.Log($"ShowShipLog('{ev.Type}')");
            string id = ev.VesselIds.FirstOrDefault(s => s != logVessel?.Id);
            if (id == null || !vessels.ContainsKey(id))
            {
                Core.Log($"Could not find a vessel record for event '{ev.Description}'. It has {ev.VesselIds.Count()} vessel ids.", LogLevel.Error);
                HideShipLog();
                return;
            }
            if (vessels.ContainsKey(id))
            {
                Core.Log($"Showing log for vessel id [{id}].");
                if (chroniclePage == 0)
                    chroniclePage = Page;
                logVessel = vessels[id];
                Invalidate();
            }
            else Core.Log($"No VesselRecord found for vessel [{id}].", LogLevel.Important);
        }

        public void HideShipLog()
        {
            logVessel = null;
            if (chroniclePage > 0)
            {
                Page = chroniclePage;
                chroniclePage = 0;
            }
            Invalidate();
        }

        public void DeleteChronicleItem(int i)
        {
            chronicle.Remove(displayChronicle[i]);
            if (displayChronicle != chronicle)
                displayChronicle.RemoveAt(i);
            DeleteUnusedVesselRecords();
            Invalidate();
        }

        void RegisterAppLauncherButton()
        {
            Core.Log("Registering AppLauncher button...");
            Texture2D icon = new Texture2D(38, 38);
            icon.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "icon.png")));
            appLauncherButton = ApplicationLauncher.Instance.AddModApplication(DisplayData, UndisplayData, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, icon);
        }

        void UnregisterAppLauncherButton()
        {
            if ((appLauncherButton != null) && (ApplicationLauncher.Instance != null))
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }

        int ChronicleIndex(int i) => SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1) : i;

        void DisplayAchievement(Achievement a, List<DialogGUIBase> grid)
        {
            if (a == null)
                return;
            grid.Add(new DialogGUILabel(a.Proto.Score > 0 ? Localizer.Format("#SpaceAge_UI_AchievementScore", a.Title, a.Score) : a.Title, true));
            grid.Add(new DialogGUILabel(a.FullDisplayValue, true));
            grid.Add(new DialogGUILabel(a.Proto.HasTime ? Core.PrintUT(a.Time) : "", true));
        }

        void UpdateScoreAchievements()
        {
            Core.Log("Updating score achievements...");
            scoreRecordNames.AddRange(protoAchievements
                .Where(pa => pa.Score > 0 && !scoreRecordNames.Contains(pa.ScoreName))
                .Select(pa => pa.ScoreName));

            scoreAchievements = new List<Achievement>(achievements.Values.Where(a => a.Proto.Score > 0));
            score = scoreAchievements.Sum(a => a.Score);
            scoreBodies = new List<string>(FlightGlobals.Bodies
                .Where(b => scoreAchievements.Exists(a => a.Body == b.name || (!a.Proto.IsBodySpecific && (b == FlightGlobals.GetHomeBody()))))
                .Select(b => b.name));

            Core.Log($"{scoreAchievements.Count} score achievements of {scoreRecordNames.Count} types for {scoreBodies.Count} bodies found. Total score: {score}");
            if (window != null && currentTab == Tab.Score)
                Invalidate();
        }

        void SelectTab(Tab t)
        {
            currentTab = t;
            Invalidate();
        }

        void Find()
        {
            Core.Log($"Find(textInput = '{textInput}')", LogLevel.Important);
            searchTerm = textInput.Trim(' ');
            Page = 1;
            Invalidate();
        }

        void AddCustomChronicleEvent()
        {
            Core.Log($"AddItem(textInput = '{textInput}')", LogLevel.Important);
            if (textInput.Trim(' ').Length != 0)
                AddChronicleEvent(new SpaceAge.ChronicleEvent("Custom", "description", textInput));
        }

        void ExportChronicle()
        {
            string filename = $"{KSPUtil.ApplicationRootPath}/saves/{HighLogic.SaveFolder}/{((textInput.Trim(' ') == "") ? "chronicle" : KSPUtil.SanitizeFilename(textInput))}.txt";
            Core.Log($"ExportChronicle to '{filename}'...", LogLevel.Important);
            TextWriter writer = File.CreateText(filename);
            for (int i = 0; i < displayChronicle.Count; i++)
                writer.WriteLine($"{KSPUtil.PrintDateCompact(displayChronicle[ChronicleIndex(i)].Time, true)}\t{displayChronicle[ChronicleIndex(i)].Description}");
            writer.Close();
            Core.Log("Done.");
            ScreenMessages.PostScreenMessage(Localizer.Format("#SpaceAge_UI_Exported", filename));
            Invalidate();
        }

        #endregion UI METHODS

        #region EVENT HANDLERS

        ChronicleEvent takeoff;
        double burnStarted = double.NaN;
        double deltaV;

        public void OnReachSpace(Vessel v)
        {
            Core.Log($"OnReachSpace({v.vesselName})");

            if (!v.IsTrackable(false))
                return;

            CheckTakeoff(true);
            if (SpaceAgeChronicleSettings.Instance.TrackReachSpace)
            {
                ChronicleEvent e = new ChronicleEvent("ReachSpace", v);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReachSpace", v);
        }

        public void onStageActivate(int stage)
        {
            Core.Log($"onStageActivate({stage})");
            if (FlightGlobals.ActiveVessel.IsTrackable(false))
            {
                AddChronicleEvent(new ChronicleEvent("Staging", FlightGlobals.ActiveVessel, "stage", stage)
                { LogOnly = true });
                CheckBurn();
            }
        }

        public void OnReturnFromOrbit(Vessel v, CelestialBody b)
        {
            Core.Log($"OnReturnFromOrbit({v.vesselName}, {b.bodyName})");

            if (!v.IsTrackable(true))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReturnFrom)
            {
                ChronicleEvent e = new ChronicleEvent("ReturnFromOrbit", v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReturnFromOrbit", b, v);
        }

        public void OnReturnFromSurface(Vessel v, CelestialBody b)
        {
            Core.Log($"OnReturnFromSurface({v.vesselName}, {b.bodyName})");

            if (!v.IsTrackable(true))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReturnFrom)
            {
                ChronicleEvent e = new ChronicleEvent("ReturnFromSurface", v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReturnFromSurface", b, v);
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log($"OnVesselRecovery('{v.vesselName}', {b})", LogLevel.Important);
            Core.Log($"missionTime = {v.missionTime}; launchTime = {v.launchTime}; autoClean = {v.autoClean}");

            if (!v.vesselRef.IsTrackable(false))
            {
                Core.Log($"{v.vesselName} is {v.vesselType}. NO adding to Chronicle.", LogLevel.Important);
                return;
            }

            if (v.missionTime <= 0)
            {
                Core.Log($"{v.vesselName} has not been launched. NO adding to Chronicle.", LogLevel.Important);
                return;
            }

            CheckAchievements("Recovery", v.vesselRef);

            if (!SpaceAgeChronicleSettings.Instance.TrackRecovery)
                return;

            ChronicleEvent e = new ChronicleEvent("Recovery", v);
            if (v.GetVesselCrew().Count > 0)
                e.AddData("crew", v.GetVesselCrew().Count);
            AddChronicleEvent(e);
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log($"OnVesselDestroy('{v.vesselName}')", LogLevel.Important);
            if (!v.IsTrackable(true))
            {
                Core.Log($"{v.name} is {v.vesselType}. NO adding to Chronicle.", LogLevel.Important);
                return;
            }

            CheckAchievements("Destroy", v);

            if (!SpaceAgeChronicleSettings.Instance.TrackDestroy)
                return;

            ChronicleEvent e = new ChronicleEvent("Destroy", v);
            if (v.terrainAltitude < 1000)
                e.AddData("body", v.mainBody.bodyName);
            AddChronicleEvent(e);
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log($"OnCrewKilled(<sender: '{report?.sender}'>)", LogLevel.Important);
            CheckAchievements("Death", report?.origin?.vessel?.mainBody, null, 0, report?.sender);
            if (!SpaceAgeChronicleSettings.Instance.TrackDeath)
                return;
            AddChronicleEvent(new ChronicleEvent("Death", "kerbal", report?.sender));
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log($"OnFlagPlanted('{v.vesselName}')", LogLevel.Important);
            CheckAchievements("FlagPlant", v);
            if (!SpaceAgeChronicleSettings.Instance.TrackFlagPlant)
                return;
            AddChronicleEvent(new ChronicleEvent("FlagPlant", "kerbal", v.GetVesselCrew()[0].nameWithGender, "body", v.mainBody.bodyName));
        }

        public void OnFacilityUpgraded(Upgradeables.UpgradeableFacility facility, int level)
        {
            Core.Log($"OnFacilityUpgraded('{facility.name}', {level})", LogLevel.Important);
            CheckAchievements("FacilityUpgraded", facility.name);
            if (!SpaceAgeChronicleSettings.Instance.TrackFacilityUpgraded)
                return;
            AddChronicleEvent(new ChronicleEvent("FacilityUpgraded", "facility", facility.name, "level", level + 1));
        }

        public void OnStructureCollapsed(DestructibleBuilding structure)
        {
            Core.Log($"OnStructureCollapsed('{structure.name}')", LogLevel.Important);
            CheckAchievements("StructureCollapsed", structure.name);
            if (!SpaceAgeChronicleSettings.Instance.TrackStructureCollapsed)
                return;
            AddChronicleEvent(new ChronicleEvent("StructureCollapsed", "facility", structure.name));
        }

        public void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a)
        {
            Core.Log($"OnTechnologyResearched(<'{a.host.title}', '{a.target}'>)", LogLevel.Important);
            CheckAchievements("TechnologyResearched", a.host.title);
            if (!SpaceAgeChronicleSettings.Instance.TrackTechnologyResearched)
                return;
            AddChronicleEvent(new ChronicleEvent("TechnologyResearched", "tech", a.host.title));
        }

        public void OnSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
        {
            Core.Log($"OnSOIChanged(<'{e.from.name}', '{e.to.name}', '{e.host.vesselName}'>)", LogLevel.Important);
            if (!e.host.IsTrackable(false))
                return;

            CheckTakeoff(true);

            if (SpaceAgeChronicleSettings.Instance.TrackSOIChange)
                AddChronicleEvent(new ChronicleEvent("SOIChange", e.host, "body", e.to.bodyName));

            if (e.from.HasParent(e.to))
            {
                Core.Log("This is a return from a child body to its parent's SOI, therefore no SOIChange achievement here.");
                return;
            }

            CheckAchievements("SOIChange", e.to, e.host);
        }

        public void OnSituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            Core.Log($"OnSituationChanged(<'{a.host.vesselName}', '{a.from}', '{a.to}'>)");

            if (!a.host.IsTrackable(true))
                return;

            ChronicleEvent e = new ChronicleEvent(null, a.host, "body", a.host.mainBody.bodyName);
            if (a.host.GetCrewCount() > 0)
                e.AddData("crew", a.host.GetCrewCount());

            switch (a.to)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    if ((takeoff != null && (takeoff.VesselIds.FirstOrDefault() != a.host.id.ToString() || Planetarium.GetUniversalTime() - takeoff.Time < SpaceAgeChronicleSettings.Instance.MinJumpDuration)) || (a.from == Vessel.Situations.PRELAUNCH))
                    {
                        Core.Log("Landing is not logged.");
                        return;
                    }
                    if (SpaceAgeChronicleSettings.Instance.TrackLanding)
                        e.Type = "Landing";
                    CheckAchievements("Landing", a.host);
                    break;

                case Vessel.Situations.ORBITING:
                    CheckTakeoff(true);
                    if (SpaceAgeChronicleSettings.Instance.TrackOrbit)
                        e.Type = "Orbit";
                    CheckAchievements("Orbit", a.host);
                    break;

                case Vessel.Situations.FLYING:
                    // It can be either a Launch, Reentry or Takeoff (e.g. hop)
                    if (a.from == Vessel.Situations.PRELAUNCH)
                    {
                        CheckAchievements("Launch", a.host);

                        if (!SpaceAgeChronicleSettings.Instance.TrackLaunch)
                            return;

                        e.Type = "Launch";
                        if (a.host.GetCrewCount() > 0)
                            e.AddData("crew", a.host.GetCrewCount());
                    }
                    else if ((a.from & (Vessel.Situations.SUB_ORBITAL | Vessel.Situations.ESCAPING | Vessel.Situations.ORBITING)) != 0)
                    {
                        if (SpaceAgeChronicleSettings.Instance.TrackReentry)
                            e.Type = "Reentry";
                        CheckAchievements("Reentry", a.host);
                    }
                    else if (a.from.IsLandedOrSplashed() && SpaceAgeChronicleSettings.Instance.TrackTakeoffs)
                    {
                        takeoff = new ChronicleEvent("Takeoff", a.host, "body", a.host.mainBody.bodyName);
                        takeoff.LogOnly = true;
                    }
                    break;

                case Vessel.Situations.SUB_ORBITAL:
                    if (a.from.IsLandedOrSplashed() && SpaceAgeChronicleSettings.Instance.TrackTakeoffs)
                    {
                        takeoff = new ChronicleEvent("Takeoff", a.host, "body", a.host.mainBody.bodyName);
                        takeoff.LogOnly = true;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(e.Type))
                AddChronicleEvent(e);
        }

        public void OnVesselDocking(uint a, uint b)
        {
            FlightGlobals.FindVessel(a, out Vessel v1);
            FlightGlobals.FindVessel(b, out Vessel v2);

            Core.Log($"OnVesselDocking('{v1?.vesselName}', '{v2?.vesselName}')");

            if (!v1.IsTrackable(false) || !v2.IsTrackable(false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackDocking)
                AddChronicleEvent(new ChronicleEvent("Docking", "vessel1", v1.vesselName, "vesselId1", v1.persistentId, "vessel2", v2.vesselName, "vesselId2", v2.persistentId));

            CheckAchievements("Docking", v1.mainBody, v1);
            CheckAchievements("Docking", v2.mainBody, v2);
        }

        public void OnVesselsUndocking(Vessel v1, Vessel v2)
        {
            Core.Log($"OnVesselsUndocking('{v1?.name}', '{v2?.name}')");

            if (!v1.IsTrackable(false) || !v2.IsTrackable(false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackDocking)
                AddChronicleEvent(new ChronicleEvent("Undocking", "vessel1", v1.vesselName, "vesselId1", v1.persistentId, "vessel2", v2.vesselName, "vesselId2", v2.persistentId));

            CheckAchievements("Undocking", v1.mainBody, v1);
            CheckAchievements("Undocking", v2.mainBody, v2);
        }

        public void OnFundsChanged(double v, TransactionReasons tr)
        {
            Core.Log($"OnFundsChanged({v}, {tr})");

            if (Funding.Instance == null)
            {
                Core.Log("Funding is not instantiated (perhaps because it is not a Career game). Terminating.");
                return;
            }

            Core.Log($"Current funds: {Funding.Instance.Funds}; last cached funds = {funds}");
            if (v > funds)
                CheckAchievements("Income", v - funds);
            else CheckAchievements("Expense", funds - v);
            funds = v;
        }

        public void OnProgressCompleted(ProgressNode n)
        {
            Core.Log($"OnProgressCompleted({n.Id})");

            if (n is KSPAchievements.PointOfInterest poi)
            {
                Core.Log($"Reached a point of interest: {poi.Id} on {poi.body}.");
                if (SpaceAgeChronicleSettings.Instance.TrackAnomalyDiscovery)
                    AddChronicleEvent(new ChronicleEvent("AnomalyDiscovery", FlightGlobals.ActiveVessel, "body", poi.body, "id", poi.Id));
                List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();
                Core.Log($"Active Vessel: {FlightGlobals.ActiveVessel.vesselName}; crew: {crew.Count}");
                CheckAchievements("AnomalyDiscovery", FlightGlobals.GetBodyByName(poi.body), null, 0, (crew.Count > 0) ? crew[0].name : null);
            }
        }

        /// <summary>
        /// Checks if a takeoff event should be recorded and adds the event
        /// </summary>
        /// <param name="ignoreTimer"></param>
        void CheckTakeoff(bool ignoreTimer)
        {
            if (takeoff != null && (ignoreTimer || (Planetarium.GetUniversalTime() - takeoff.Time) >= SpaceAgeChronicleSettings.Instance.MinJumpDuration))
            {
                Core.Log($"CheckTakeoff({ignoreTimer})");
                Vessel v = takeoff.Vessels.FirstOrDefault();
                if (v != null && !v.situation.IsLandedOrSplashed())
                {
                    Core.Log($"Registering takeoff event for {takeoff.Vessels.FirstOrDefault()}.");
                    AddChronicleEvent(takeoff);
                    CheckAchievements("Takeoff", takeoff.Vessels.FirstOrDefault());
                }
                else Core.Log($"Takeoff of {v?.vesselName} is not logged.");
                takeoff = null;
            }
        }

        void CheckBurn()
        {
            if (!SpaceAgeChronicleSettings.Instance.TrackBurns)
                return;
            Vessel v = FlightGlobals.ActiveVessel;
            if (v == null)
                return;
            bool isBurning = v.IsBurning();

            if (double.IsNaN(burnStarted) && isBurning)
            {
                burnStarted = Planetarium.GetUniversalTime();
                deltaV = v.VesselDeltaV.TotalDeltaVVac;
                Core.Log($"Burn started on {KSPUtil.PrintDateCompact(burnStarted, true, true)}. Vessel's deltaV = {deltaV:N0} m/s.");
                return;
            }

            if (!double.IsNaN(burnStarted) && !isBurning)
            {
                int burnDuration = (int)Math.Round(Planetarium.GetUniversalTime() - burnStarted);
                burnStarted = double.NaN;
                deltaV -= v.VesselDeltaV.TotalDeltaVVac;
                if (burnDuration >= SpaceAgeChronicleSettings.Instance.MinBurnDuration)
                {
                    Core.Log($"Finished burn that lasted {burnDuration} s, deltaV = {deltaV:N0} m/s.");
                    AddChronicleEvent(
                        new ChronicleEvent("Burn", v, "duration", burnDuration, "deltaV", (int)deltaV)
                        { LogOnly = true });
                }
            }
        }
    }
}

#endregion EVENT HANDLERS
