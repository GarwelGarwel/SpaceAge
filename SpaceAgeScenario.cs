using System;
using System.Collections.Generic;
using System.IO;
using KSP.Localization;
using KSP.UI.Screens;
using UnityEngine;

namespace SpaceAge
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    class SpaceAgeScenario : ScenarioModule
    {
        List<ChronicleEvent> chronicle = new List<ChronicleEvent>(), displayChronicle;
        static List<ProtoAchievement> protoAchievements;
        Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        List<Achievement> scoreAchievements = new List<Achievement>();
        List<string> scoreRecordNames = new List<string>();
        List<string> scoreBodies = new List<string>();
        double score;
        double funds;

        IButton toolbarButton;
        ApplicationLauncherButton appLauncherButton;

        enum Tab { Chronicle, Achievements, Score };
        Tab currentTab = Tab.Chronicle;

        const float windowWidth = 600;
        int[] page = new int[3] { 1, 1, 1 };
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;
        string textInput = "";

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start", Core.LogLevel.Important);

            displayChronicle = chronicle;

            // Adding event handlers
            GameEvents.onGameNewStart.Add(ResetSettings);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.VesselSituation.onLaunch.Add(OnLaunch);
            GameEvents.VesselSituation.onReachSpace.Add(OnReachSpace);
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
                toolbarButton.OnClick += (e) =>
                {
                    if (window == null)
                        DisplayData();
                    else UndisplayData();
                };
            }

            funds = (Funding.Instance != null) ? Funding.Instance.Funds : Double.NaN;
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
            GameEvents.VesselSituation.onLaunch.Remove(OnLaunch);
            GameEvents.VesselSituation.onReachSpace.Remove(OnReachSpace);
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
            ConfigNode chronicleNode = new ConfigNode("CHRONICLE");
            foreach (ChronicleEvent e in chronicle)
                chronicleNode.AddNode(e.ConfigNode);
            Core.Log(chronicleNode.CountNodes + " nodes saved.");
            node.AddNode(chronicleNode);
            ConfigNode achievementsNode = new ConfigNode("ACHIEVEMENTS");
            foreach (Achievement a in achievements.Values)
                achievementsNode.AddNode(a.ConfigNode);
            Core.Log(achievementsNode.CountNodes + " achievements saved.");
            node.AddNode(achievementsNode);
        }

        public override void OnLoad(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnLoad");
            InitializeDatabase();

            chronicle.Clear();
            if (node.HasNode("CHRONICLE"))
            {
                Core.Log(node.GetNode("CHRONICLE").CountNodes + " nodes found in Chronicle.");
                foreach (ConfigNode n in node.GetNode("CHRONICLE").GetNodes())
                    if (n.name == "EVENT")
                        chronicle.Add(new ChronicleEvent(n));
            }
            displayChronicle = chronicle;

            if (node.HasNode("ACHIEVEMENTS"))
            {
                Core.Log(node.GetNode("ACHIEVEMENTS").CountNodes + " nodes found in ACHIEVEMENTS.");
                double score = 0;
                foreach (ConfigNode n in node.GetNode("ACHIEVEMENTS").GetNodes())
                    if (n.name == "ACHIEVEMENT")
                        try
                        {
                            Achievement a = new Achievement(n);
                            achievements.Add(a.FullName, a);
                            if (a.Proto.Score > 0) Core.Log(a.FullDisplayValue + ": " + a.Score + " points");
                            score += a.Score;
                        }
                        catch (ArgumentException e) { Core.Log(e.Message); }
                Core.Log("Total score: " + score);
            }

            UpdateScoreAchievements();
        }

        void InitializeDatabase()
        {
            if (protoAchievements != null) return;
            Core.Log("Initializing proto records...");
            protoAchievements = new List<ProtoAchievement>();
            foreach (ConfigNode n in GameDatabase.Instance.GetConfigNodes("PROTOACHIEVEMENT"))
                protoAchievements.Add(new ProtoAchievement(n));
            Core.Log("protoAchievements contains " + protoAchievements.Count + " records.");
        }

        void RegisterAppLauncherButton()
        {
            Core.Log("Registering AppLauncher button...");
            Texture2D icon = new Texture2D(38, 38);
            icon.LoadImage(File.ReadAllBytes(System.IO.Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "icon.png")));
            appLauncherButton = ApplicationLauncher.Instance.AddModApplication(DisplayData, UndisplayData, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, icon);
        }

        void UnregisterAppLauncherButton()
        {
            if ((appLauncherButton != null) && (ApplicationLauncher.Instance != null))
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }

        /// <summary>
        /// Applying default settings, using config files if exist
        /// </summary>
        void ResetSettings()
        {
            Core.Log("ResetSettings", Core.LogLevel.Important);
            SpaceAgeChronicleSettings.Instance.Reset();
            if (GameDatabase.Instance.ExistsConfigNode("SPACEAGE_CONFIG"))
            {
                ConfigNode config = GameDatabase.Instance.GetConfigNode("SPACEAGE_CONFIG");
                Core.Log("SPACEAGE_CONFIG: " + config, Core.LogLevel.Important);
                SpaceAgeChronicleSettings.Instance.FundsPerScore = (float)Core.GetDouble(config, "fundsPerScore", SpaceAgeChronicleSettings.Instance.FundsPerScore);
                SpaceAgeChronicleSettings.Instance.SciencePerScore = (float)Core.GetDouble(config, "sciencePerScore", SpaceAgeChronicleSettings.Instance.SciencePerScore);
                SpaceAgeChronicleSettings.Instance.RepPerScore = (float)Core.GetDouble(config, "repPerScore", SpaceAgeChronicleSettings.Instance.RepPerScore);
            }
            else Core.Log("SPACEAGE_CONFIG node not found.", Core.LogLevel.Important);
        }

        /// <summary>
        /// Check if settings need to be reset; show or hide AppLauncher button if the settings have changed
        /// </summary>
        void OnGameSettingsApplied()
        {
            Core.Log("OnGameSettingsApplied", Core.LogLevel.Important);
            if (SpaceAgeChronicleSettings.Instance.ResetSettings)
            {
                ResetSettings();
                ScreenMessages.PostScreenMessage(Localizer.Format("#SpaceAge_UI_SettingsReset"));
            }
            if (SpaceAgeChronicleSettings.Instance.ShowAppLauncherButton && appLauncherButton == null)
                RegisterAppLauncherButton();
            if (!SpaceAgeChronicleSettings.Instance.ShowAppLauncherButton)
                UnregisterAppLauncherButton();
        }

        public void AddChronicleEvent(ChronicleEvent e)
        {
            Core.ShowNotification(Localizer.Format("#SpaceAge_UI_EventDetected", e.Type));
            if (SpaceAgeChronicleSettings.Instance.UnwarpOnEvents && (TimeWarp.CurrentRateIndex != 0))
                TimeWarp.SetRate(0, true, !SpaceAgeChronicleSettings.Instance.ShowNotifications);
            chronicle.Add(e);
            Invalidate();
        }

        #region ACHIEVEMENTS METHODS

        int achievementsImported = 0;
        void ParseProgressNodes(ConfigNode node, CelestialBody body)
        {
            Core.Log(node.name + " config node contains " + node.CountNodes + " sub-nodes.");
            Achievement a;
            foreach (ProtoAchievement pa in protoAchievements)
                if ((pa.StockSynonym != null) && (pa.StockSynonym.Length != 0) && (node.HasNode(pa.StockSynonym)))
                {
                    ConfigNode n = node.GetNode(pa.StockSynonym);
                    Core.Log(pa.StockSynonym + " node found for " + pa.Name + ".");
                    Core.Log(n.ToString());
                    a = new SpaceAge.Achievement(pa, body);
                    if (n.HasValue("completed"))
                        a.Time = Double.Parse(n.GetValue("completed"));
                    else if (n.HasValue("completedManned"))
                        a.Time = Double.Parse(n.GetValue("completedManned"));
                    else if (!pa.CrewedOnly && n.HasValue("completedUnmanned"))
                        a.Time = Double.Parse(n.GetValue("completedUnmanned"));
                    else
                    {
                        Core.Log("Time value not found, achievement has not been completed.");
                        continue;
                    }
                    Core.Log("Found candidate achievement: " + KSPUtil.PrintDateCompact(a.Time, true) + " " + a.Title);
                    if (CheckAchievement(a))
                        achievementsImported++;
                }
        }

        void ParseProgressTracking()
        {
            ConfigNode trackingNode = null;
            foreach (ProtoScenarioModule psm in HighLogic.CurrentGame.scenarios)
                if (psm.moduleName == "ProgressTracking")
                {
                    trackingNode = psm.GetData();
                    break;
                }

            if (trackingNode == null)
            {
                Core.Log("ProgressTracking scenario not found!", Core.LogLevel.Important);
                return;
            }

            if (trackingNode.HasNode("Progress"))
                trackingNode = trackingNode.GetNode("Progress");
            else
            {
                Core.Log("ProgressTracking scenario does not contain Progress node!", Core.LogLevel.Important);
                Core.Log(trackingNode.ToString());
                return;
            }

            achievementsImported = 0;
            ParseProgressNodes(trackingNode, null);
            foreach (CelestialBody b in FlightGlobals.Bodies)
                if (trackingNode.HasNode(b.name))
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

        public static ProtoAchievement FindProtoAchievement(string name)
        {
            Core.Log("Searching among " + protoAchievements.Count + " ProtoAchievements.");
            foreach (ProtoAchievement pa in protoAchievements)
                if (pa.Name == name)
                    return pa;
            Core.Log("ProtoAchievement '" + name + "' not found!", Core.LogLevel.Error);
            return null;
        }

        public Achievement FindAchievement(string fullname) => achievements.ContainsKey(fullname) ? achievements[fullname] : null;

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

            Core.Log("CheckAchievements('" + ev + "', body = '" + body?.name + "', vessel = '" + vessel?.vesselName + "', value = " + value + ", hero = '" + (hero ?? "null") + "')");

            bool scored = false;
            foreach (ProtoAchievement pa in protoAchievements)
                if (pa.OnEvents.Contains(ev))
                {
                    string msg = "";
                    Achievement ach = new Achievement(pa, body, vessel, value, hero);
                    if (CheckAchievement(ach))
                    {
                        if (ach.Proto.Score > 0)
                        {
                            scored = true;
                            double score = ach.Score;
                            msg = "\r\n" + Localizer.Format("#SpaceAge_PS_PointsAdded", score);
                            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                            {
                                double f = score * SpaceAgeChronicleSettings.Instance.FundsPerScore;
                                if (f != 0)
                                {
                                    Core.Log("Adding " + f + " funds.");
                                    Funding.Instance.AddFunds(f, TransactionReasons.Progression);
                                    msg += "\r\n" + Localizer.Format("#SpaceAge_PS_Funds", f.ToString("N0"));
                                }
                            }
                            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER) || (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
                            {
                                float s = (float)score * SpaceAgeChronicleSettings.Instance.SciencePerScore;
                                if (s != 0)
                                {
                                    Core.Log("Adding " + s + " science.");
                                    ResearchAndDevelopment.Instance.AddScience(s, TransactionReasons.Progression);
                                    msg += "\r\n" + Localizer.Format("#SpaceAge_PS_Science", s.ToString("N1"));
                                }
                            }
                            float r = (float)score * SpaceAgeChronicleSettings.Instance.RepPerScore;
                            if (r != 0)
                            {
                                Core.Log("Adding " + r + " rep.");
                                Reputation.Instance.AddReputation(r, TransactionReasons.Progression);
                                msg += "\r\n" + Localizer.Format("#SpaceAge_PS_Reputation", r.ToString("N0"));
                            }
                        }
                        if (pa.Type != AchievementType.Total)
                        {
                            Core.Log(ach.FullName + " completed.");
                            if (SpaceAgeChronicleSettings.Instance.TrackAchievements)
                                AddChronicleEvent(new ChronicleEvent("Achievement", "title", ach.Title, "value", ach.ShortDisplayValue));
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

        #endregion
        #region UI METHODS

        void DisplayAchievement(Achievement a, List<DialogGUIBase> grid)
        {
            if (a == null) return;
            grid.Add(new DialogGUILabel(a.Proto.Score > 0 ? Localizer.Format("#SpaceAge_UI_AchievementScore", a.Title, a.Score) : a.Title, true));
            grid.Add(new DialogGUILabel(a.FullDisplayValue, true));
            grid.Add(new DialogGUILabel(a.Proto.HasTime ? Core.ParseUT(a.Time) : "", true));
        }

        List<Achievement> SortedAchievements
        {
            get
            {
                List<Achievement> res = new List<Achievement>();
                foreach (ProtoAchievement pa in protoAchievements)
                    if (!pa.IsBodySpecific && achievements.ContainsKey(pa.Name))
                        res.Add(FindAchievement(pa.Name));
                foreach (CelestialBody b in FlightGlobals.Bodies)
                    foreach (ProtoAchievement pa in protoAchievements)
                        if (pa.IsBodySpecific && achievements.ContainsKey(Achievement.GetFullName(pa.Name, b.name)))
                            res.Add(FindAchievement(Achievement.GetFullName(pa.Name, b.name)));
                return res;
            }
        }

        void UpdateScoreAchievements()
        {
            Core.Log("Updating score achievements...");
            foreach (ProtoAchievement pa in protoAchievements)
                if ((pa.Score > 0) && !scoreRecordNames.Contains(pa.ScoreName))
                    scoreRecordNames.Add(pa.ScoreName);

            scoreAchievements.Clear();
            score = 0;
            foreach (Achievement a in achievements.Values)
                if (a.Proto.Score > 0)
                {
                    Core.Log(a.FullName + " gives " + a.Score + " score.");
                    scoreAchievements.Add(a);
                    score += a.Score;
                }
            
            scoreBodies.Clear();
            foreach (CelestialBody b in FlightGlobals.Bodies)
                foreach (Achievement a in scoreAchievements)
                    if ((a.Body == b.name) || (!a.Proto.IsBodySpecific && (b == FlightGlobals.GetHomeBody())))
                    {
                        Core.Log("There is at least one " + b.name + " achievement.");
                        scoreBodies.Add(b.name);
                        break;
                    }

            Core.Log(scoreAchievements.Count + " score achievements of " + scoreRecordNames.Count + " types for " + scoreBodies.Count + " bodies found. Total score: " + score);
            if ((window != null) && (currentTab == Tab.Score))
                Invalidate();
        }

        public DialogGUIBase WindowContents
        {
            get
            {
                List<DialogGUIBase> gridContents;

                if (Page > PageCount)
                    Page = PageCount;
                if (PageCount == 0)
                    Page = 1;
                int startingIndex = (Page - 1) * LinesPerPage;

                switch (currentTab)
                {
                    case Tab.Chronicle:
                        gridContents = new List<DialogGUIBase>(LinesPerPage);
                        Core.Log("Displaying events " + ((Page - 1) * LinesPerPage + 1) + "-" + Math.Min(Page * LinesPerPage, displayChronicle.Count) + "...");
                        for (int i = startingIndex; i < Math.Min(startingIndex + LinesPerPage, displayChronicle.Count); i++)
                            gridContents.Add(
                                new DialogGUIHorizontalLayout(
                                    new DialogGUILabel(
                                        "<color=\"white\">" + Core.ParseUT(displayChronicle[SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1) : i].Time) + "</color>\t" + displayChronicle[SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1) : i].Description,
                                        true),
                                    new DialogGUIButton<int>("X", DeleteChronicleItem, SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1) : i)));
                        return new DialogGUIVerticalLayout(
                            new DialogGUIVerticalLayout(windowWidth - 10, 0, 5, new RectOffset(5, 5, 0, 0), TextAnchor.UpperLeft, gridContents.ToArray()),
                            (HighLogic.LoadedSceneIsFlight ? new DialogGUIHorizontalLayout() :
                            new DialogGUIHorizontalLayout(
                                windowWidth - 20,
                                10,
                                new DialogGUITextInput(textInput, false, 100, (s) => textInput = s),
                                new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Find"), Find, false),
                                new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Add"), AddItem, false),
                                new DialogGUIButton(Localizer.Format("#SpaceAge_UI_Export"), ExportChronicle))));

                    case Tab.Achievements:
                        gridContents = new List<DialogGUIBase>(LinesPerPage * 3);
                        Core.Log("Displaying achievements starting from " + startingIndex + " out of " + achievements.Count + "...");
                        List<Achievement> achList = SortedAchievements;
                        if ((achievements.Count == 0) || (achList.Count == 0))
                        {
                            Core.Log("Can't display Achievement tabs. There are " + achievements.Count + " achievements and " + achList.Count + " protoachievements.", Core.LogLevel.Error);
                            currentTab = Tab.Chronicle;
                            return WindowContents;
                        }
                        string body = null;
                        foreach (Achievement a in achList.GetRange(startingIndex, Math.Min(LinesPerPage, achievements.Count - startingIndex)))
                        {
                            // Achievement for a new body => display the body's name on a new line
                            if ((a.Body != body) && (a.Body.Length != 0))
                            {
                                body = a.Body;
                                gridContents.Add(new DialogGUILabel("", true));
                                gridContents.Add(new DialogGUILabel("<align=\"center\"><color=\"white\"><b>" + Localizer.Format("<<1>>", Core.GetBodyDisplayName(body)) + "</b></color></align>", true));
                                gridContents.Add(new DialogGUILabel("", true));
                            }
                            DisplayAchievement(a, gridContents);
                        }
                        return new DialogGUIGridLayout(
                            new RectOffset(5, 5, 0, 0),
                            new Vector2((windowWidth - 10) / 3 - 3, 20),
                            new Vector2(5, 5),
                            UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft,
                            UnityEngine.UI.GridLayoutGroup.Axis.Horizontal,
                            TextAnchor.MiddleLeft,
                            UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount,
                            3,
                            gridContents.ToArray());

                    case Tab.Score:
                        Core.Log("Displaying score bodies from " + startingIndex + " out of " + scoreBodies.Count + "...");
                        if (scoreAchievements.Count == 0)
                            return new DialogGUILabel("<align=\"center\">" + Localizer.Format("#SpaceAge_UI_NoScore") + "</align>", true);
                        gridContents = new List<DialogGUIBase>((1 + Math.Min(LinesPerPage, scoreBodies.Count)) * (1 + scoreRecordNames.Count));
                        gridContents.Add(new DialogGUILabel("<color=\"white\">" + Localizer.Format("#SpaceAge_UI_Body") + "</color>"));
                        foreach (string srn in scoreRecordNames)
                            gridContents.Add(new DialogGUILabel("<color=\"white\">" + srn + "</color>"));
                        for (int i = startingIndex; i < Math.Min(startingIndex + LinesPerPage, scoreBodies.Count); i++)
                        {
                            gridContents.Add(new DialogGUILabel("<color=\"white\">" + Localizer.Format("<<1>>", Core.GetBodyDisplayName(scoreBodies[i])) + "</color>"));
                            foreach (string srn in scoreRecordNames)
                            {
                                double s = 0;
                                bool crewed = false;
                                foreach (Achievement a in scoreAchievements)
                                    if ((a.Proto.ScoreName == srn) && ((a.Body == scoreBodies[i]) || (!a.Proto.IsBodySpecific && (scoreBodies[i] == FlightGlobals.GetHomeBodyName()))))
                                    {
                                        s += a.Score;
                                        if (a.Proto.CrewedOnly)
                                            crewed = true;
                                    }
                                gridContents.Add(new DialogGUILabel(((s > 0) ? (crewed ? "<color=\"green\">[M]  " : "<color=\"yellow\">[U]  ") + s + "</color>" : "<color=\"white\">—</color>")));
                            }
                        }
                        return new DialogGUIVerticalLayout(true, true, 5, new RectOffset(5, 5, 0, 0), TextAnchor.MiddleLeft,
                            new DialogGUIGridLayout(
                                new RectOffset(0, 0, 0, 0),
                                new Vector2((windowWidth - 10) / (scoreRecordNames.Count + 1) - 5, 20),
                                new Vector2(5, 5),
                                UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft,
                                UnityEngine.UI.GridLayoutGroup.Axis.Horizontal,
                                TextAnchor.MiddleLeft,
                                UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount,
                                scoreRecordNames.Count + 1,
                                gridContents.ToArray()),
                            new DialogGUILabel("<color=\"white\"><b>" + Localizer.Format("#SpaceAge_UI_TotalScore", score.ToString("N0")) + "</b></color>"));
                }
                return null;
            }
        }

        public void DisplayData()
        {
            Core.Log("DisplayData", Core.LogLevel.Important);

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
                    PageCount > 1 ?
                    new DialogGUIHorizontalLayout(
                        true,
                        false,
                        new DialogGUIButton("<<", FirstPage, () => (Page > 1), false),
                        new DialogGUIButton("<", PageUp, () => (Page > 1), false),
                        new DialogGUIHorizontalLayout(TextAnchor.LowerCenter, new DialogGUILabel(Page + "/" + PageCount)),
                        new DialogGUIButton(">", PageDown, () => (Page < PageCount), false),
                        new DialogGUIButton(">>", LastPage, () => (Page < PageCount), false)) :
                        new DialogGUIHorizontalLayout(),
                    WindowContents),
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

        void SelectTab(Tab t)
        {
            currentTab = t;
            Invalidate();
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
                return (int)System.Math.Ceiling((double)itemsNum / LinesPerPage);
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

        public void DeleteChronicleItem(int i)
        {
            chronicle.Remove(displayChronicle[i]);
            if (displayChronicle != chronicle)
                displayChronicle.RemoveAt(i);
            Invalidate();
        }

        void Find()
        {
            Core.Log("Find(textInput = '" + textInput + "')", Core.LogLevel.Important);
            if (textInput.Trim(' ').Length != 0) 
            {
                displayChronicle = new List<ChronicleEvent>();
                foreach (ChronicleEvent e in chronicle)
                    if (e.Description.ToLower().Contains(textInput.ToLower()))
                        displayChronicle.Add(e);
            }
            else displayChronicle = chronicle;
            Page = 1;
            Invalidate();
        }

        void AddItem()
        {
            Core.Log("AddItem(textInput = '" + textInput + "')", Core.LogLevel.Important);
            if (textInput.Trim(' ').Length == 0)
                return;
            AddChronicleEvent(new SpaceAge.ChronicleEvent("Custom", "description", textInput));
        }

        void ExportChronicle()
        {
            string filename = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/" + ((textInput.Trim(' ') == "") ? "chronicle" : KSPUtil.SanitizeFilename(textInput)) + ".txt";
            Core.Log("ExportChronicle to '" + filename + "'...", Core.LogLevel.Important);
            TextWriter writer = File.CreateText(filename);
            for (int i = 0; i < displayChronicle.Count; i++)
                writer.WriteLine(KSPUtil.PrintDateCompact(displayChronicle[
                    SpaceAgeChronicleSettings.Instance.NewestFirst
                    ? (displayChronicle.Count - i - 1) : i].Time, true) + "\t" + displayChronicle[SpaceAgeChronicleSettings.Instance.NewestFirst ? (displayChronicle.Count - i - 1)
                    : i].Description);
            writer.Close();
            Core.Log("Done.");
            ScreenMessages.PostScreenMessage(Localizer.Format("#SpaceAge_UI_Exported", filename));
            Invalidate();
        }

        #endregion
        #region EVENT HANDLERS

        bool IsVesselEligible(Vessel v, bool mustBeActive)
            => (v != null)
            && (v.vesselType != VesselType.Debris)
            && (v.vesselType != VesselType.EVA)
            && (v.vesselType != VesselType.Flag)
            && (v.vesselType != VesselType.SpaceObject)
            && (v.vesselType != VesselType.Unknown)
            && (!mustBeActive || (v == FlightGlobals.ActiveVessel));

        double lastLaunch = -1;
        public void OnLaunch(Vessel v)
        {
            Core.Log("OnLaunch(" + v?.vesselName + ")", Core.LogLevel.Important);
            
            if (!IsVesselEligible(v, true))
            {
                Core.Log("Vessel is ineligible due to being " + v?.vesselType);
                return;
            }

            double timeSinceLastLaunch = Planetarium.GetUniversalTime() - lastLaunch;
            if (timeSinceLastLaunch < 1)
            {
                Core.Log("Duplicate OnLaunch call. Previous launch happened " + timeSinceLastLaunch + " s ago.");
                return;
            }

            if ((v.mainBody != FlightGlobals.GetHomeBody()) || (v.missionTime >= 1))
            {
                Core.Log("Fake launch due to main body: " + v.mainBody.name + ", mission time: " + v.missionTime);
                return;
            }

            lastLaunch = Planetarium.GetUniversalTime();
            CheckAchievements("Launch", v);
            
            if (!SpaceAgeChronicleSettings.Instance.TrackLaunch)
                return;

            ChronicleEvent e = new ChronicleEvent("Launch", v);
            if (FlightGlobals.ActiveVessel.GetCrewCount() > 0)
                e.Data.Add("crew", v.GetCrewCount().ToString());

            AddChronicleEvent(e);
        }

        public void OnReachSpace(Vessel v)
        {
            Core.Log("OnReachSpace(" + v.vesselName + ")");

            if (!IsVesselEligible(v, false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReachSpace)
            {
                ChronicleEvent e = new ChronicleEvent("ReachSpace", v);
                if (v.GetCrewCount() > 0)
                    e.Data.Add("crew", v.GetCrewCount().ToString());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReachSpace", v);
        }

        public void OnReturnFromOrbit(Vessel v, CelestialBody b)
        {
            Core.Log("OnReturnFromOrbit(" + v.vesselName + ", " + b.bodyName + ")");

            if (!IsVesselEligible(v, true))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReturnFrom)
            {
                ChronicleEvent e = new ChronicleEvent("ReturnFromOrbit", v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.Data.Add("crew", v.GetCrewCount().ToString());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReturnFromOrbit", b, v);
        }

        public void OnReturnFromSurface(Vessel v, CelestialBody b)
        {
            Core.Log("OnReturnFromSurface(" + v.vesselName + ", " + b.bodyName + ")");

            if (!IsVesselEligible(v, true))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackReturnFrom)
            {
                ChronicleEvent e = new ChronicleEvent("ReturnFromSurface", v, "body", b.bodyName);
                if (v.GetCrewCount() > 0)
                    e.Data.Add("crew", v.GetCrewCount().ToString());
                AddChronicleEvent(e);
            }

            CheckAchievements("ReturnFromSurface", b, v);
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log("OnVesselRecovery('" + v.vesselName + "', " + b + ")", Core.LogLevel.Important);
            Core.Log("missionTime = " + v.missionTime + "; launchTime = " + v.launchTime + "; autoClean = " + v.autoClean);

            if (!IsVesselEligible(v.vesselRef, false))
            {
                Core.Log(v.vesselName + " is " + v.vesselType + ". NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }

            if (v.missionTime <= 0)
            {
                Core.Log(v.vesselName + " has not been launched. NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }

            CheckAchievements("Recovery", v.vesselRef);

            if (!SpaceAgeChronicleSettings.Instance.TrackRecovery)
                return;

            ChronicleEvent e = new ChronicleEvent("Recovery", v);
            if (v.GetVesselCrew().Count > 0)
                e.Data.Add("crew", v.GetVesselCrew().Count.ToString());

            AddChronicleEvent(e);
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log("OnVesselDestroy('" + v.vesselName + "')", Core.LogLevel.Important);

            if (!IsVesselEligible(v, true))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }

            CheckAchievements("Destroy", v);

            if (!SpaceAgeChronicleSettings.Instance.TrackDestroy)
                return;

            ChronicleEvent e = new ChronicleEvent("Destroy", v);
            if (v.terrainAltitude < 1000)
                e.Data.Add("body", v.mainBody.bodyName);

            AddChronicleEvent(e);
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log("OnCrewKilled(<sender: '" + report?.sender + "'>)", Core.LogLevel.Important);

            CheckAchievements("Death", report?.origin?.vessel?.mainBody, null, 0, report?.sender);

            if (!SpaceAgeChronicleSettings.Instance.TrackDeath)
                return;

            AddChronicleEvent(new ChronicleEvent("Death", "kerbal", report?.sender));
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log("OnFlagPlanted('" + v.vesselName + "')", Core.LogLevel.Important);

            CheckAchievements("FlagPlant", v);

            if (!SpaceAgeChronicleSettings.Instance.TrackFlagPlant)
                return;

            AddChronicleEvent(new ChronicleEvent("FlagPlant", "body", v.mainBody.bodyName));
        }

        public void OnFacilityUpgraded(Upgradeables.UpgradeableFacility facility, int level)
        {
            Core.Log("OnFacilityUpgraded('" + facility.name + "', " + level + ")", Core.LogLevel.Important);

            CheckAchievements("FacilityUpgraded", facility.name);

            if (!SpaceAgeChronicleSettings.Instance.TrackFacilityUpgraded)
                return;

            AddChronicleEvent(new ChronicleEvent("FacilityUpgraded", "facility", facility.name, "level", (level + 1).ToString()));
        }

        public void OnStructureCollapsed(DestructibleBuilding structure)
        {
            Core.Log("OnStructureCollapsed('" + structure.name + "')", Core.LogLevel.Important);

            CheckAchievements("StructureCollapsed", structure.name);

            if (!SpaceAgeChronicleSettings.Instance.TrackStructureCollapsed)
                return;

            AddChronicleEvent(new ChronicleEvent("StructureCollapsed", "facility", structure.name));
        }

        public void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a)
        {
            Core.Log("OnTechnologyResearched(<'" + a.host.title + "', '" + a.target.ToString() + "'>)", Core.LogLevel.Important);

            CheckAchievements("TechnologyResearched", a.host.title);

            if (!SpaceAgeChronicleSettings.Instance.TrackTechnologyResearched)
                return;

            AddChronicleEvent(new ChronicleEvent("TechnologyResearched", "tech", a.host.title));
        }

        public void OnSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
        {
            Core.Log("OnSOIChanged(<'" + e.from.name + "', '" + e.to.name + "', '" + e.host.vesselName + "'>)", Core.LogLevel.Important);

            if (!IsVesselEligible(e.host, false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackSOIChange)
                AddChronicleEvent(new SpaceAge.ChronicleEvent("SOIChange", e.host, "body", e.to.bodyName));

            if (e.from.HasParent(e.to))
            {
                Core.Log("This is a return from a child body to its parent's SOI, therefore no SOIChange achievement here.");
                return;
            }

            CheckAchievements("SOIChange", e.to, e.host);
        }

        double lastTakeoff = 0;
        public void OnSituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            Core.Log("OnSituationChanged(<'" + a.host.vesselName + "', '" + a.from + "', '" + a.to + "'>)");

            if (!IsVesselEligible(a.host, true))
                return;

            ChronicleEvent e = new ChronicleEvent(null, a.host);
            e.Data.Add("body", a.host.mainBody.bodyName);
            if (a.host.GetCrewCount() > 0)
                e.Data.Add("crew", a.host.GetCrewCount().ToString());

            switch (a.to)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    if ((Planetarium.GetUniversalTime() < lastTakeoff + SpaceAgeChronicleSettings.Instance.MinJumpDuration) || (a.from == Vessel.Situations.PRELAUNCH))
                    {
                        Core.Log("Landing is not logged (last takeoff: " + lastTakeoff + "; current UT:" + Planetarium.GetUniversalTime() + ").");
                        return;
                    }
                    if (SpaceAgeChronicleSettings.Instance.TrackLanding)
                        e.Type = "Landing";
                    CheckAchievements("Landing", a.host);
                    break;

                case Vessel.Situations.ORBITING:
                    if (SpaceAgeChronicleSettings.Instance.TrackOrbit)
                        e.Type = "Orbit";
                    CheckAchievements("Orbit", a.host);
                    break;

                case Vessel.Situations.FLYING:
                    // Fix for some launches not calling OnLaunch event
                    if (a.from == Vessel.Situations.PRELAUNCH)
                        OnLaunch(a.host);
                    else if ((a.from & (Vessel.Situations.SUB_ORBITAL | Vessel.Situations.ESCAPING | Vessel.Situations.ORBITING)) != 0)
                    {
                        if (SpaceAgeChronicleSettings.Instance.TrackReentry)
                            e.Type = "Reentry";
                        CheckAchievements("Reentry", a.host);
                    }
                    break;
            }

            if (((a.from == Vessel.Situations.LANDED) || (a.from == Vessel.Situations.SPLASHED)) && ((a.to == Vessel.Situations.FLYING) || (a.to == Vessel.Situations.SUB_ORBITAL)))
                lastTakeoff = Planetarium.GetUniversalTime();

            if ((e.Type != null) && (e.Type.Length != 0))
                AddChronicleEvent(e);
        }

        public void OnVesselDocking(uint a, uint b)
        {
            FlightGlobals.FindVessel(a, out Vessel v1);
            FlightGlobals.FindVessel(b, out Vessel v2);

            Core.Log("OnVesselDocking('" + v1?.vesselName + "', '" + v2?.vesselName + "')");

            if (!IsVesselEligible(v1, false) || !IsVesselEligible(v2, false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackDocking)
                AddChronicleEvent(new ChronicleEvent("Docking", "vessel1", v1.vesselName, "vesselId1", v1.persistentId, "vessel2", v2.vesselName, "vesselId2", v2.persistentId));

            CheckAchievements("Docking", v1.mainBody, v1);
            CheckAchievements("Docking", v2.mainBody, v2);
        }

        public void OnVesselsUndocking(Vessel v1, Vessel v2)
        {
            Core.Log("OnVesselsUndocking('" + v1?.name + "', '" + v2?.name + "')");

            if (!IsVesselEligible(v1, false) || !IsVesselEligible(v2, false))
                return;

            if (SpaceAgeChronicleSettings.Instance.TrackDocking)
                AddChronicleEvent(new ChronicleEvent("Undocking", "vessel1", v1.vesselName, "vesselId1", v1.persistentId, "vessel2", v2.vesselName, "vesselId2", v2.persistentId));

            CheckAchievements("Undocking", v1.mainBody, v1);
            CheckAchievements("Undocking", v2.mainBody, v2);
        }

        public void OnFundsChanged(double v, TransactionReasons tr)
        {
            Core.Log("OnFundsChanged(" + v + ", " + tr + ")");

            if (Funding.Instance == null)
            {
                Core.Log("Funding is not instantiated (perhaps because it is not a Career game). Terminating.");
                return;
            }

            Core.Log("Current funds: " + Funding.Instance.Funds + "; last cached funds = " + funds);
            if (v > funds)
                CheckAchievements("Income", v - funds);
            else CheckAchievements("Expense", funds - v);
            funds = v;
        }

        public void OnProgressCompleted(ProgressNode n)
        {
            Core.Log("OnProgressCompleted(" + n.Id + ")");

            if (n is KSPAchievements.PointOfInterest poi)
            {
                Core.Log("Reached a point of interest: " + poi.Id + " on " + poi.body);
                if (SpaceAgeChronicleSettings.Instance.TrackAnomalyDiscovery)
                    AddChronicleEvent(new ChronicleEvent("AnomalyDiscovery", FlightGlobals.ActiveVessel, "body", poi.body, "id", poi.Id));
                List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();
                Core.Log("Active Vessel: " + FlightGlobals.ActiveVessel.vesselName + "; crew: " + crew.Count);
                CheckAchievements("AnomalyDiscovery", FlightGlobals.GetBodyByName(poi.body), null, 0, (crew.Count > 0) ? crew[0].name : null);
            }
        }
    }
}
#endregion
