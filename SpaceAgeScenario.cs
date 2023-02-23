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
        enum Tab
        {
            Chronicle,
            Achievements,
            Score
        };

        enum TimeFormat
        {
            UT,
            MET
        };

        static List<ProtoAchievement> protoAchievements;

        List<ChronicleEvent> chronicle = new List<ChronicleEvent>(), displayChronicle;
        Dictionary<string, VesselRecord> vessels = new Dictionary<string, VesselRecord>();
        Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();
        List<Achievement> scoreAchievements = new List<Achievement>();
        List<string> scoreRecordNames = new List<string>();
        List<string> scoreBodies = new List<string>();
        double score;
        double funds;
        float science;

        VesselRecord logVessel = null;
        TimeFormat logTimeFormat = TimeFormat.UT;

        public static SpaceAgeScenario Instance { get; private set; }

#if DEBUG
        IterationTimer saveTimer = new IterationTimer("SAVE");
        IterationTimer loadTimer = new IterationTimer("LOAD");
        int items, cycles;
#endif

#region LIFE CYCLE

        const string Node_Chronicle = "CHRONICLE";
        const string Node_Event = "EVENT";
        const string Node_Vessels = "VESSELS";
        const string Node_Vessel = "VESSEL";
        const string Node_Achievements = "ACHIEVEMENTS";
        const string Node_Achievement = "ACHIEVEMENT";
        const string Node_Protoachievement = "PROTOACHIEVEMENT";
        const string Node_Config = "SPACEAGE_CONFIG";

        double nextUpdate = 0;
        IButton toolbarButton;
        ApplicationLauncherButton appLauncherButton;

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start", LogLevel.Important);

            Instance = this;

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
            GameEvents.OnScienceChanged.Add(OnScienceChanged);
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
            science = (ResearchAndDevelopment.Instance != null) ? ResearchAndDevelopment.Instance.Science : float.NaN;

            InitializeDatabase();
            if (SpaceAgeChronicleSettings.Instance.ImportStockAchievements)
                ParseProgressTracking();
        }

        public void OnDisable()
        {
            Core.Log("SpaceAgeScenario.OnDisable", LogLevel.Important);
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
            GameEvents.OnScienceChanged.Remove(OnScienceChanged);
            GameEvents.OnProgressComplete.Remove(OnProgressCompleted);

            // Removing Toolbar & AppLauncher buttons
            toolbarButton?.Destroy();
            UnregisterAppLauncherButton();

            Instance = null;
        }

        public override void OnSave(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnSave", LogLevel.Important);
#if DEBUG
            saveTimer.Start();
#endif
            ConfigNode n1 = new ConfigNode(Node_Chronicle);

            n1.AddValue("logTimeFormat", logTimeFormat);
            ConfigNode n2;
            for (int i = 0; i < chronicle.Count; i++)
            {
                chronicle[i].Save(n2 = new ConfigNode(Node_Event));
                n1.AddNode(n2);
            }
            node.AddNode(n1);
            Core.Log($"{n1.CountNodes} events saved.");

            n1 = new ConfigNode(Node_Vessels);
            foreach (VesselRecord vessel in vessels.Values)
            {
                vessel.Save(n2 = new ConfigNode(Node_Vessel));
                n1.AddNode(n2);
            }
            node.AddNode(n1);
            Core.Log($"{n1.CountNodes} vessels saved.");

            n1 = new ConfigNode(Node_Achievements);
            foreach (Achievement a in achievements.Values)
            {
                a.Save(n2 = new ConfigNode(Node_Achievement));
                n1.AddNode(n2);
            }
            node.AddNode(n1);
            Core.Log($"{n1.CountNodes} achievements saved.");

#if DEBUG
            saveTimer.Stop();
            items += chronicle.Count;
            cycles++;
            Core.Log($"Average {(float)items / cycles:N0} Chronicle items.");
#endif
        }

        public override void OnLoad(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnLoad", LogLevel.Important);
#if DEBUG
            loadTimer.Start();
#endif
            InitializeDatabase();

            if (node.HasValue("logTimeFormat"))
                Enum.TryParse<TimeFormat>(node.GetValue("logTimeFormat"), out logTimeFormat);

            chronicle.Clear();
            if (node.HasNode(Node_Chronicle))
            {
                ConfigNode[] chronicleNodes = node.GetNode(Node_Chronicle).GetNodes(Node_Event);
                Core.Log($"{chronicleNodes.Length} nodes found in {Node_Chronicle}.");
                for (int i = 0; i < chronicleNodes.Length; i++)
                {
                    ChronicleEvent e = new ChronicleEvent(chronicleNodes[i]);
                    if (e.Valid)
                        chronicle.Add(e);
                }    
            }
            displayChronicle = chronicle;

            vessels.Clear();
            if (node.HasNode(Node_Vessels))
            {
                ConfigNode[] vesselsNodes = node.GetNode(Node_Vessels).GetNodes(Node_Vessel);
                Core.Log($"{vesselsNodes.Length} nodes found in {Node_Vessels}.");
                foreach (VesselRecord vr in vesselsNodes.Select(n => new VesselRecord(n)).Where(vr => vr.Valid))
                    AddVesselRecord(vr);
            }

            achievements.Clear();
            if (node.HasNode(Node_Achievements))
            {
                ConfigNode[] achievmentsNodes = node.GetNode(Node_Achievements).GetNodes(Node_Achievement);
                Core.Log($"{achievmentsNodes.Length} nodes found in {Node_Achievements}.");
                double score = 0;
                foreach (Achievement a in achievmentsNodes.Select(n => new Achievement(n)).Where(a => a.Valid))
                    try
                    {
                        achievements.Add(a.FullName, a);
                        if (a.Proto.Score != 0)
                            Core.Log($"{a.Title}: {a.Score} points ({a.Hero}).");
                        score += a.Score;
                    }
                    catch (ArgumentException e)
                    {
                        Core.Log(e.Message, LogLevel.Error);
                    }
                Core.Log($"Total score: {score} points.");
            }

            UpdateScoreAchievements();
#if DEBUG
            loadTimer.Stop();
            items += chronicle.Count;
            cycles++;
            Core.Log($"Average {(float)items / cycles:N0} Chronicle items.");
#endif
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
            protoAchievements = new List<ProtoAchievement>(GameDatabase.Instance.GetConfigNodes(Node_Protoachievement)
                .Select(n => new ProtoAchievement(n))
                .Where(pa => pa.Valid));
            Core.Log($"{protoAchievements.Count} ProtoAchievements loaded.");
        }

        /// <summary>
        /// Applying default settings, using config files if exist
        /// </summary>
        void ResetSettings()
        {
            Core.Log("ResetSettings", LogLevel.Important);
            SpaceAgeChronicleSettings.Instance.Reset();
            if (GameDatabase.Instance.ExistsConfigNode(Node_Config))
            {
                ConfigNode config = GameDatabase.Instance.GetConfigNode(Node_Config);
                Core.Log($"{Node_Config}: {config}", LogLevel.Important);
                SpaceAgeChronicleSettings.Instance.UseStockDateTimeFormat = config.GetBool("stockDateTimeFormat", SpaceAgeChronicleSettings.Instance.UseStockDateTimeFormat);
                SpaceAgeChronicleSettings.Instance.FundsPerScore = (float)config.GetDouble("fundsPerScore", SpaceAgeChronicleSettings.Instance.FundsPerScore);
                SpaceAgeChronicleSettings.Instance.SciencePerScore = (float)config.GetDouble("sciencePerScore", SpaceAgeChronicleSettings.Instance.SciencePerScore);
                SpaceAgeChronicleSettings.Instance.RepPerScore = (float)config.GetDouble("repPerScore", SpaceAgeChronicleSettings.Instance.RepPerScore);
            }
            else Core.Log($"{Node_Config} node not found.", LogLevel.Important);
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

#region CHRONICLE

        /// <summary>
        /// Adds a new event to the Chronicle, including all relevant vessel records, and displays a notification, if necessary
        /// </summary>
        /// <param name="e"></param>
        public void AddChronicleEvent(ChronicleEvent e)
        {
            chronicle.Add(e);
            foreach (string id in e.VesselIds)
                AddVesselRecord(new VesselRecord(id));
            if (SpaceAgeChronicleSettings.Instance.ShowNotifications && !e.LogOnly)
            {
                TimeWarp.SetRate(0, true, false);
                ScreenMessages.PostScreenMessage(e.Description);
            }
            Invalidate();
        }

        /// <summary>
        /// Removes i-th event in the display chronicle
        /// </summary>
        /// <param name="i">0-based index in the display chronicle</param>
        public void RemoveChronicleItem(int i)
        {
            ChronicleEvent ce = displayChronicle[i];
            chronicle.Remove(ce);
            if (displayChronicle != chronicle)
                displayChronicle.RemoveAt(i);
            foreach (string vesselId in ce.VesselIds.Where(vesselId => !chronicle.Exists(ce2 => ce2.HasVesselId(vesselId))))
                vessels.Remove(vesselId);
            Invalidate();
        }

        /// <summary>
        /// Adds a new VesselRecord if it is unique
        /// </summary>
        public void AddVesselRecord(VesselRecord vesselRecord)
        {
            if (vesselRecord.Valid && !vessels.ContainsKey(vesselRecord.Id))
                vessels.Add(vesselRecord.Id, vesselRecord);
        }

        public void DeleteUnusedVesselRecords()
        {
            foreach (string id in vessels.Keys.Where(id => !chronicle.Exists(ev => ev.HasVesselId(id))).ToList())
                vessels.Remove(id);
        }

#endregion CHRONICLE

#region ACHIEVEMENTS

        int achievementsImported = 0;

        public static ProtoAchievement FindProtoAchievement(string name) => protoAchievements.Find(pa => pa.Name == name);

        public Achievement FindAchievement(string fullname) => achievements.ContainsKey(fullname) ? achievements[fullname] : null;

        public void SetAchievement(string fullname, Achievement value) => achievements[fullname] = value;

        void ParseProgressNodes(ConfigNode node, CelestialBody body)
        {
            Core.Log($"{node.name} config node contains {node.CountNodes} sub-nodes.");
            Achievement a;
            foreach (ProtoAchievement pa in protoAchievements
                .Where(pa => pa.StockSynonym != null && pa.StockSynonym.Length != 0 && node.HasNode(pa.StockSynonym)))
            {
                ConfigNode n = node.GetNode(pa.StockSynonym);
                Core.Log($"{pa.StockSynonym} node found for {pa.Name}:\n{n}");
                a = new Achievement(pa, body);
                a.Time = Math.Min(n.GetLongOrDouble("completed", long.MaxValue), n.GetLongOrDouble("completedManned", long.MaxValue));
                if (!pa.CrewedOnly)
                    a.Time = Math.Min(a.Time, n.GetLongOrDouble("completedUnmanned", long.MaxValue));
                if (a.Time == long.MaxValue)
                {
                    Core.Log("Time value not found, achievement has not been completed.");
                    continue;
                }
                Core.Log($"Found candidate achievement: {Core.DateTimeFormatter.PrintDateCompact(a.Time, true)} {a.Title}");
                if (a.Register())
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

            SpaceAgeChronicleSettings.Instance.ImportStockAchievements = false;
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
                if (ach.Register())
                {
                    if (ach.Proto.Score != 0)
                    {
                        scored = true;
                        double score = ach.Score;
                        msg = $"\r\n{Localizer.Format("#SpaceAge_PS_PointsAdded", score)}";
                        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                        {
                            double funds = score * SpaceAgeChronicleSettings.Instance.FundsPerScore;
                            if (funds != 0)
                            {
                                Core.Log($"Adding {funds} funds.");
                                Funding.Instance.AddFunds(funds, TransactionReasons.Progression);
                                msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Funds", funds.ToString("N0"))}";
                            }
                        }
                        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                        {
                            float science = (float)score * SpaceAgeChronicleSettings.Instance.SciencePerScore;
                            if (science != 0)
                            {
                                Core.Log($"Adding {science} science.");
                                ResearchAndDevelopment.Instance.AddScience(science, TransactionReasons.Progression);
                                msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Science", science.ToString("N1"))}";
                            }
                        }
                        float reputation = (float)score * SpaceAgeChronicleSettings.Instance.RepPerScore;
                        if (reputation != 0)
                        {
                            Core.Log($"Adding {reputation} rep.");
                            Reputation.Instance.AddReputation(reputation, TransactionReasons.Progression);
                            msg += $"\r\n{Localizer.Format("#SpaceAge_PS_Reputation", reputation.ToString("N0"))}";
                        }
                    }

                    if (pa.HasTime)
                    {
                        Core.Log($"{ach.FullName} completed.");
                        if (SpaceAgeChronicleSettings.Instance.TrackAchievements)
                            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Achievement, "title", ach.Title, "value", ach.ShortDisplayValue, vessel));
                        MessageSystem.Instance.AddMessage(new MessageSystem.Message(
                            Localizer.Format("#SpaceAge_PS_Title"),
                            Localizer.Format("#SpaceAge_PS_AchievementCompleted", ach.Title) + msg,
                            MessageSystemButton.MessageButtonColor.YELLOW,
                            MessageSystemButton.ButtonIcons.ACHIEVE));
                    }

                    if (pa.HasTime || currentTab == Tab.Achievements)
                        Invalidate();
                }
            }
            if (scored)
                UpdateScoreAchievements();
        }

        void CheckAchievements(string ev, Vessel v) => CheckAchievements(ev, v.mainBody, v);

        void CheckAchievements(string ev, double v) => CheckAchievements(ev, null, null, v);

        void CheckAchievements(string ev, string hero) => CheckAchievements(ev, null, null, 0, hero);

        void UpdateScoreAchievements()
        {
            Core.Log("Updating score achievements...");
            scoreRecordNames.AddRange(protoAchievements
                .Where(pa => pa.Score != 0 && !scoreRecordNames.Contains(pa.ScoreName))
                .Select(pa => pa.ScoreName));
            scoreAchievements = new List<Achievement>(achievements.Values.Where(a => a.Proto.Score != 0));
            score = scoreAchievements.Sum(a => a.Score);
            scoreBodies = new List<string>(FlightGlobals.Bodies
                .Where(b => scoreAchievements.Exists(a => a.Body == b.name || (!a.Proto.IsBodySpecific && b == FlightGlobals.GetHomeBody())))
                .Select(b => b.name));
            Core.Log($"{scoreAchievements.Count} score achievements of {scoreRecordNames.Count} types for {scoreBodies.Count} bodies found. Total score: {score}");
            if (currentTab == Tab.Score)
                Invalidate();
        }

#endregion ACHIEVEMENTS

#region UI METHODS

        const float windowWidth = 600;
        static readonly DialogGUILabel emptyLabel = new DialogGUILabel("", true);

        Tab currentTab = Tab.Chronicle;
        int[] pages = new int[3] { 1, 1, 1 };
        int chroniclePage = 0;
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;
        string textInput = "";
        string searchQuery = "";

        List<Achievement> SortedAchievements
        {
            get
            {
                List<Achievement> res = new List<Achievement>(protoAchievements
                    .Where(pa => !pa.IsBodySpecific && achievements.ContainsKey(pa.Name))
                    .Select(pa => FindAchievement(pa.Name)));
                res.AddRange(FlightGlobals.Bodies
                    .SelectMany(b => protoAchievements
                    .Where(pa => pa.IsBodySpecific && achievements.ContainsKey(Achievement.GetFullName(pa.Name, b.name)))
                    .Select(pa => FindAchievement(Achievement.GetFullName(pa.Name, b.name)))));
                return res;
            }
        }

        int CurrentPage
        {
            get => pages[(int)currentTab];
            set => pages[(int)currentTab] = value;
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

        public void Invalidate()
        {
            if (window != null)
            {
                UndisplayData();
                DisplayData();
            }
        }

        void DisplayData()
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

                if (searchQuery.Length != 0)
                {
                    // Filtering search results
                    List<string> searchTerms = searchQuery.SplitIntoTerms().ToList();
                    displayChronicle = displayChronicle.FindAll(ev => searchTerms.All(term => ev.Description.ContainsTerm(term)));
                    Core.Log($"Filtered {displayChronicle.Count} search results for '{searchQuery}' ({searchTerms.Count} search terms).");
                }
            }

            if (CurrentPage > PageCount)
                CurrentPage = PageCount;
            if (PageCount == 0)
                CurrentPage = 1;
            int startingIndex = (CurrentPage - 1) * LinesPerPage;
            List<DialogGUIBase> grid;
            DialogGUIBase windowContent = null;

            switch (currentTab)
            {
                case Tab.Chronicle:
                    grid = new List<DialogGUIBase>(LinesPerPage);
                    Core.Log($"Displaying events {startingIndex + 1} to {Math.Min(startingIndex + LinesPerPage, displayChronicle.Count)}...");
                    for (int i = startingIndex; i < startingIndex + LinesPerPage && i < displayChronicle.Count; i++)
                    {
                        ChronicleEvent ce = displayChronicle[ChronicleIndex(i)];
                        grid.Add(
                            new DialogGUIHorizontalLayout(
                                new DialogGUILabel($"<color=\"white\">{((logTimeFormat == TimeFormat.MET && logVessel != null) ? Core.DateTimeFormatter.PrintTimeCompact(ce.Time - logVessel.LaunchTime, true) : Core.DateTimeFormatter.PrintDateCompact(ce.Time, true))}</color>", 90),
                                new DialogGUILabel(ce.Description, true),
                                ce.HasVesselId() && (logVessel == null || ce.VesselIds.Count() > 1)
                                    ? new DialogGUIButton<ChronicleEvent>(Localizer.Format("#SpaceAge_UI_LogBtn"), ShowShipLog, ce, false)
                                    : new DialogGUIBase(),
                                new DialogGUIButton<int>("x", RemoveChronicleItem, ChronicleIndex(i))));
                    }
                    windowContent = new DialogGUIVerticalLayout(
                        logVessel != null
                        ? new DialogGUIHorizontalLayout(
                            TextAnchor.MiddleCenter,
                            new DialogGUIButton(logTimeFormat == TimeFormat.UT ? Localizer.Format("#SpaceAge_UI_UT") : Localizer.Format("#SpaceAge_UI_MET"), SwitchTimeFormat, false),
                            new DialogGUILabel($"<align=\"center\"><b>{Localizer.Format("#SpaceAge_UI_LogTitle", logVessel.Name)}</b>\n{Localizer.Format("#SpaceAge_UI_ShipInfo", Core.DateTimeFormatter.PrintDateCompact(logVessel.LaunchTime, true, true))}</align>", true),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Back"), HideShipLog, false))
                        : new DialogGUIBase(),
                        new DialogGUIVerticalLayout(windowWidth - 10, 0, 5, new RectOffset(5, 5, 0, 0), TextAnchor.UpperLeft, grid.ToArray()),
                        HighLogic.LoadedSceneIsFlight
                        ? new DialogGUIHorizontalLayout(
                            true,
                            true,
                            new DialogGUIButton(
                                Localizer.Format("#SpaceAge_UI_ActiveVesselLog"),
                                () => ShowShipLog(new VesselRecord(FlightGlobals.ActiveVessel)),
                                () => FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id != logVessel?.Guid,
                                false))
                        : new DialogGUIHorizontalLayout(
                            windowWidth - 20,
                            10,
                            new DialogGUITextInput(textInput, false, 100, s => textInput = s),
                            (searchQuery.Length != 0) ? new DialogGUIButton(Localizer.Format("#SpaceAge_UI_ClearBtn"), ClearInput) : new DialogGUIBase(),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Find"), Find),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Add"), AddCustomChronicleEvent),
                            new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Export"), ExportChronicle)));
                    break;

                case Tab.Achievements:
                    grid = new List<DialogGUIBase>(LinesPerPage * 3);
                    Core.Log($"Displaying achievements starting from {startingIndex} out of {achievements.Count}...");
                    string body = null;
                    List<Achievement> sortedAchievements = SortedAchievements;
                    for (int i = startingIndex; i < startingIndex + LinesPerPage && i < sortedAchievements.Count; i++)
                    {
                        Achievement a = sortedAchievements[i];
                        // Achievement for a new body => display the body's name on a new line
                        if (a.Body != body && a.Body.Length != 0)
                        {
                            body = a.Body;
                            grid.Add(emptyLabel);
                            grid.Add(new DialogGUILabel($"<align=\"center\"><color=\"white\"><b>{Localizer.Format("<<1>>", Core.GetBodyDisplayName(body))}</b></color></align>", true));
                            grid.Add(emptyLabel);
                        }
                        grid.Add(new DialogGUILabel(a.Proto.Score != 0 ? Localizer.Format("#SpaceAge_UI_AchievementScore", a.Title, a.Score) : a.Title, true));
                        grid.Add(new DialogGUILabel(a.FullDisplayValue, true));
                        if (a.Proto.HasTime)
                            grid.Add(new DialogGUILabel(Core.DateTimeFormatter.PrintDateCompact(a.Time, false), true));
                        else grid.Add(emptyLabel);
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
                    grid = new List<DialogGUIBase>((Math.Min(LinesPerPage, scoreBodies.Count) + 1) * (scoreRecordNames.Count + 1));
                    grid.Add(new DialogGUILabel($"<color=\"white\">{Localizer.Format("#SpaceAge_UI_Body")}</color>"));
                    grid.AddRange(scoreRecordNames.Select(srn => new DialogGUILabel($"<color=\"white\">{srn}</color>")));
                    for (int i = startingIndex; i < startingIndex + LinesPerPage && i < scoreBodies.Count; i++)
                    {
                        grid.Add(new DialogGUILabel($"<color=\"white\">{Localizer.Format("<<1>>", Core.GetBodyDisplayName(scoreBodies[i]))}</color>"));
                        foreach (string srn in scoreRecordNames)
                        {
                            double s = 0;
                            bool manned = false;
                            foreach (Achievement a in scoreAchievements.Where(a =>
                                a.Proto.ScoreName == srn
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
                        new DialogGUIButton<Tab>(Localizer.Format("#SpaceAge_UI_Achievements"), SelectTab, Tab.Achievements, () => currentTab != Tab.Achievements && achievements.Count > 0, true),
                        new DialogGUIButton<Tab>(Localizer.Format("#SpaceAge_UI_Score"), SelectTab, Tab.Score, () => currentTab != Tab.Score && score != 0, true)),
                    PageCount > 1
                    ? new DialogGUIHorizontalLayout(
                        true,
                        false,
                        new DialogGUIButton("<<", FirstPage, () => CurrentPage > 1, false),
                        new DialogGUIButton("<", PageUp, () => CurrentPage > 1, false),
                        new DialogGUIHorizontalLayout(TextAnchor.LowerCenter, new DialogGUILabel($"{CurrentPage}/{PageCount}")),
                        new DialogGUIButton(">", PageDown, () => CurrentPage < PageCount, false),
                        new DialogGUIButton(">>", LastPage, () => CurrentPage < PageCount, false))
                    : new DialogGUIHorizontalLayout(),
                    windowContent),
                false,
                HighLogic.UISkin,
                false);
        }

        void UndisplayData()
        {
            if (window != null)
            {
                Vector3 v = window.RTrf.position;
                windowPosition = new Rect(v.x / Screen.width + 0.5f, v.y / Screen.height + 0.5f, windowWidth, 50);
                window.Dismiss();
            }
        }

        void PageUp()
        {
            if (CurrentPage > 1)
                CurrentPage--;
            Invalidate();
        }

        void FirstPage()
        {
            CurrentPage = 1;
            Invalidate();
        }

        void PageDown()
        {
            if (CurrentPage < PageCount)
                CurrentPage++;
            Invalidate();
        }

        void LastPage()
        {
            CurrentPage = PageCount;
            Invalidate();
        }

        void SwitchTimeFormat()
        {
            if (logTimeFormat == TimeFormat.UT)
                logTimeFormat = TimeFormat.MET;
            else logTimeFormat = TimeFormat.UT;
            Invalidate();
        }

        void ShowShipLog(VesselRecord vesselRecord)
        {
            if (vesselRecord?.Id == null)
                return;
            Core.Log($"Showing log for vessel {vesselRecord.Name} [{vesselRecord.Id}].");
            if (chroniclePage == 0)
                chroniclePage = CurrentPage;
            logVessel = vesselRecord;
            CurrentPage = 1;
            Invalidate();
        }

        void ShowShipLog(ChronicleEvent ev)
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
            ShowShipLog(vessels[id]);
        }

        void HideShipLog()
        {
            logVessel = null;
            if (chroniclePage > 0)
            {
                CurrentPage = chroniclePage;
                chroniclePage = 0;
            }
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
            if (appLauncherButton != null)
                ApplicationLauncher.Instance?.RemoveModApplication(appLauncherButton);
        }

        int ChronicleIndex(int i) => SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1) : i;

        void SelectTab(Tab t)
        {
            currentTab = t;
            Invalidate();
        }

        void ClearInput()
        {
            searchQuery = textInput = "";
            CurrentPage = 1;
            Invalidate();
        }

        void Find()
        {
            Core.Log($"Find, textInput = '{textInput}'", LogLevel.Important);
            searchQuery = textInput.Trim();
            CurrentPage = 1;
            Invalidate();
        }

        void AddCustomChronicleEvent()
        {
            Core.Log($"AddCustomChronicleEvent, textInput = '{textInput}'", LogLevel.Important);
            if (textInput.Trim().Length != 0)
                AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Custom, "description", textInput));
            textInput = "";
            Invalidate();
        }

        void ExportChronicle()
        {
            textInput = textInput.Trim();
            string filename = $"{KSPUtil.ApplicationRootPath}/saves/{HighLogic.SaveFolder}/{((textInput.Length == 0) ? "chronicle" : KSPUtil.SanitizeFilename(textInput))}.txt";
            Core.Log($"ExportChronicle to '{filename}'...", LogLevel.Important);
            TextWriter writer = File.CreateText(filename);
            for (int i = 0; i < displayChronicle.Count; i++)
                writer.WriteLine($"{Core.DateTimeFormatter.PrintDateCompact(displayChronicle[ChronicleIndex(i)].Time, true)}\t{displayChronicle[ChronicleIndex(i)].Description}");
            writer.Close();
            Core.Log("Done.");
            ScreenMessages.PostScreenMessage(Localizer.Format("#SpaceAge_UI_Exported", filename));
            textInput = "";
            Invalidate();
        }

#endregion UI METHODS

#region EVENT HANDLERS

        ChronicleEvent takeoff;
        double burnStarted = double.NaN;
        double deltaV;

        public void OnReachSpace(Vessel v)
        {
            Core.Log($"OnReachSpace('{v.vesselName}')");

            if (!v.IsTrackable(false))
                return;

            CheckTakeoff(true);
            if (SpaceAgeChronicleSettings.Instance.TrackReachSpace)
            {
                ChronicleEvent e = new ChronicleEvent(ChronicleEvent.ReachSpace, v);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements(ChronicleEvent.ReachSpace, v);
        }

        public void onStageActivate(int stage)
        {
            Core.Log($"onStageActivate({stage})");
            if (FlightGlobals.ActiveVessel.IsTrackable(false))
            {
                AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Staging, FlightGlobals.ActiveVessel, "stage", stage)
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
                ChronicleEvent e = new ChronicleEvent(ChronicleEvent.ReturnFromOrbit, v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements(ChronicleEvent.ReturnFromOrbit, b, v);
        }

        public void OnReturnFromSurface(Vessel v, CelestialBody b)
        {
            Core.Log($"OnReturnFromSurface('{v.vesselName}', {b.bodyName})");

            if (!v.IsTrackable(true))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReturnFrom)
            {
                ChronicleEvent e = new ChronicleEvent(ChronicleEvent.ReturnFromSurface, v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.AddData("crew", v.GetCrewCount());
                AddChronicleEvent(e);
            }

            CheckAchievements(ChronicleEvent.ReturnFromSurface, b, v);
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log($"OnVesselRecovery('{v.vesselName}', {b})", LogLevel.Important);

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

            CheckAchievements(ChronicleEvent.Recovery, v.vesselRef);

            if (!SpaceAgeChronicleSettings.Instance.TrackRecovery)
                return;

            ChronicleEvent e = new ChronicleEvent(ChronicleEvent.Recovery, v);
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

            CheckAchievements(ChronicleEvent.Destroy, v);

            if (!SpaceAgeChronicleSettings.Instance.TrackDestroy)
                return;

            ChronicleEvent e = new ChronicleEvent(ChronicleEvent.Destroy, v);
            if (v.terrainAltitude < 1000 || (v.situation & Vessel.Situations.FLYING) != 0)
                e.AddData("body", v.mainBody.bodyName);
            AddChronicleEvent(e);
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log($"OnCrewKilled(<sender: '{report?.sender}'>)", LogLevel.Important);
            CheckAchievements(ChronicleEvent.Death, report?.origin?.vessel?.mainBody, null, 0, report?.sender);
            if (!SpaceAgeChronicleSettings.Instance.TrackDeath)
                return;
            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Death, "kerbal", report?.sender));
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log($"OnFlagPlanted(<mainBody: {v.mainBody}>)", LogLevel.Important);
            string kerbal = FlightGlobals.ActiveVessel.GetVesselCrew().FirstOrDefault()?.nameWithGender;
            CheckAchievements(ChronicleEvent.FlagPlant, body: v.mainBody, hero: kerbal);
            if (!SpaceAgeChronicleSettings.Instance.TrackFlagPlant)
                return;
            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.FlagPlant, "kerbal", kerbal, "body", v.mainBody.bodyName));
        }

        public void OnFacilityUpgraded(Upgradeables.UpgradeableFacility facility, int level)
        {
            Core.Log($"OnFacilityUpgraded('{facility.name}', {level})", LogLevel.Important);
            CheckAchievements(ChronicleEvent.FacilityUpgraded, facility.name);
            if (!SpaceAgeChronicleSettings.Instance.TrackFacilityUpgraded)
                return;
            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.FacilityUpgraded, "facility", facility.name, "level", level + 1));
        }

        public void OnStructureCollapsed(DestructibleBuilding structure)
        {
            Core.Log($"OnStructureCollapsed('{structure.name}')", LogLevel.Important);
            CheckAchievements(ChronicleEvent.StructureCollapsed, structure.name);
            if (!SpaceAgeChronicleSettings.Instance.TrackStructureCollapsed)
                return;
            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.StructureCollapsed, "facility", structure.name));
        }

        public void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a)
        {
            Core.Log($"OnTechnologyResearched(<'{a.host.title}', '{a.target}'>)", LogLevel.Important);
            CheckAchievements(ChronicleEvent.TechnologyResearched, a.host.title);
            if (!SpaceAgeChronicleSettings.Instance.TrackTechnologyResearched)
                return;
            AddChronicleEvent(new ChronicleEvent(ChronicleEvent.TechnologyResearched, "tech", a.host.title));
        }

        public void OnSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
        {
            Core.Log($"OnSOIChanged(<'{e.from.name}', '{e.to.name}', '{e.host.vesselName}'>)", LogLevel.Important);
            if (!e.host.IsTrackable(false))
                return;

            CheckTakeoff(true);

            if (SpaceAgeChronicleSettings.Instance.TrackSOIChange)
                AddChronicleEvent(new ChronicleEvent(ChronicleEvent.SOIChange, e.host, "body", e.to.bodyName));

            if (e.from.HasParent(e.to))
            {
                Core.Log("This is a return from a child body to its parent's SOI, therefore no SOIChange achievement here.");
                return;
            }

            CheckAchievements(ChronicleEvent.SOIChange, e.to, e.host);
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
                        e.Type = ChronicleEvent.Landing;
                    CheckAchievements(ChronicleEvent.Landing, a.host);
                    break;

                case Vessel.Situations.ORBITING:
                    CheckTakeoff(true);
                    if (SpaceAgeChronicleSettings.Instance.TrackOrbit)
                        e.Type = ChronicleEvent.Orbit;
                    CheckAchievements(ChronicleEvent.Orbit, a.host);
                    break;

                case Vessel.Situations.FLYING:
                    // It can be either a Launch, Reentry or Takeoff (e.g. hop)
                    if (a.from == Vessel.Situations.PRELAUNCH)
                    {
                        CheckAchievements(ChronicleEvent.Launch, a.host);

                        if (!SpaceAgeChronicleSettings.Instance.TrackLaunch)
                            return;

                        e.Type = ChronicleEvent.Launch;
                        if (a.host.GetCrewCount() > 0)
                            e.AddData("crew", a.host.GetCrewCount());
                    }
                    else if ((a.from & (Vessel.Situations.SUB_ORBITAL | Vessel.Situations.ESCAPING | Vessel.Situations.ORBITING)) != 0)
                    {
                        if (SpaceAgeChronicleSettings.Instance.TrackReentry)
                            e.Type = ChronicleEvent.Reentry;
                        CheckAchievements(ChronicleEvent.Reentry, a.host);
                    }
                    else if (a.from.IsLandedOrSplashed() && SpaceAgeChronicleSettings.Instance.TrackTakeoffs)
                    {
                        takeoff = new ChronicleEvent(ChronicleEvent.Takeoff, a.host, "body", a.host.mainBody.bodyName)
                        { LogOnly = true };
                    }
                    break;

                case Vessel.Situations.SUB_ORBITAL:
                    if (a.from.IsLandedOrSplashed() && SpaceAgeChronicleSettings.Instance.TrackTakeoffs)
                    {
                        takeoff = new ChronicleEvent(ChronicleEvent.Takeoff, a.host, "body", a.host.mainBody.bodyName)
                        { LogOnly = true };
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
                AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Docking, "vessel1", v1.vesselName, "vesselId1", v1.id, "vessel2", v2.vesselName, "vesselId2", v2.id));

            CheckAchievements(ChronicleEvent.Docking, v1.mainBody, v1);
            CheckAchievements(ChronicleEvent.Docking, v2.mainBody, v2);
        }

        public void OnVesselsUndocking(Vessel v1, Vessel v2)
        {
            Core.Log($"OnVesselsUndocking('{v1?.name}', '{v2?.name}')");

            if (!v1.IsTrackable(false) || !v2.IsTrackable(false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackDocking)
                AddChronicleEvent(new ChronicleEvent(ChronicleEvent.Undocking, "vessel1", v1.vesselName, "vesselId1", v1.id, "vessel2", v2.vesselName, "vesselId2", v2.id));

            CheckAchievements(ChronicleEvent.Undocking, v1.mainBody, v1);
            CheckAchievements(ChronicleEvent.Undocking, v2.mainBody, v2);
        }

        public void OnFundsChanged(double v, TransactionReasons tr)
        {
            Core.Log($"OnFundsChanged({v}, {tr})");

            if (double.IsNaN(funds) || Funding.Instance == null)
            {
                Core.Log("Funding is not instantiated (perhaps because it is not a Career game). Terminating.", LogLevel.Error);
                return;
            }

            Core.Log($"Current funds: {Funding.Instance.Funds}; last cached funds = {funds}");
            if (v > funds)
                CheckAchievements("Income", v - funds);
            else CheckAchievements("Expense", funds - v);
            funds = v;
        }

        public void OnScienceChanged(float v, TransactionReasons tr)
        {
            Core.Log($"OnScienceChanged({v}, {tr})");

            if (float.IsNaN(science) || ResearchAndDevelopment.Instance == null || !ResearchAndDevelopment.Instance.enabled)
            {
                Core.Log("R&D is not active (perhaps because it is not a Career or Science game). Terminating.", LogLevel.Error);
                return;
            }

            Core.Log($"Current science: {ResearchAndDevelopment.Instance.Science}; last cached science = {science}");
            if (v > science)
                CheckAchievements("ScienceAdded", v - science);
            science = v;
        }

        public void OnProgressCompleted(ProgressNode n)
        {
            Core.Log($"OnProgressCompleted({n.Id})");

            if (n is KSPAchievements.PointOfInterest poi)
            {
                Core.Log($"Reached a point of interest: {poi.Id} on {poi.body}.");
                if (SpaceAgeChronicleSettings.Instance.TrackAnomalyDiscovery)
                    AddChronicleEvent(new ChronicleEvent(ChronicleEvent.AnomalyDiscovery, FlightGlobals.ActiveVessel, "body", poi.body, "id", poi.Id));
                List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();
                Core.Log($"Active Vessel: {FlightGlobals.ActiveVessel.vesselName}; crew: {crew.Count}");
                CheckAchievements(ChronicleEvent.AnomalyDiscovery, FlightGlobals.GetBodyByName(poi.body), null, 0, (crew.Count > 0) ? crew[0].name : null);
            }
        }

        /// <summary>
        /// Checks if a takeoff event should be recorded and adds the event
        /// </summary>
        /// <param name="ignoreTimer"></param>
        void CheckTakeoff(bool ignoreTimer)
        {
            if (takeoff != null && (ignoreTimer || Planetarium.GetUniversalTime() - takeoff.Time >= SpaceAgeChronicleSettings.Instance.MinJumpDuration))
            {
                Core.Log($"CheckTakeoff({ignoreTimer})");
                Vessel v = takeoff.Vessel;
                if (v != null && !v.situation.IsLandedOrSplashed())
                {
                    Core.Log($"Registering takeoff event for {takeoff.Vessel}.");
                    AddChronicleEvent(takeoff);
                    CheckAchievements(ChronicleEvent.Takeoff, takeoff.Vessel);
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
                Core.Log($"Burn started on {Core.DateTimeFormatter.PrintDateCompact(burnStarted, true, true)}. Vessel's deltaV = {deltaV:N0} m/s.");
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
                        new ChronicleEvent(ChronicleEvent.Burn, v, "duration", burnDuration, "deltaV", Math.Round(deltaV))
                        { LogOnly = true });
                }
            }
        }
    }
}

#endregion EVENT HANDLERS
