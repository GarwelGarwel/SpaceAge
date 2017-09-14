using System;
using System.Collections.Generic;
using System.IO;
using KSP.UI.Screens;
using UnityEngine;

namespace SpaceAge
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    class SpaceAgeScenario : ScenarioModule
    {
        List<ChronicleEvent> chronicle = new List<ChronicleEvent>();
        static List<ProtoAchievement> protoAchievements;
        Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        IButton toolbarButton;
        ApplicationLauncherButton appLauncherButton;
        enum Tabs { Chronicle, Achievements };
        Tabs currentTab = Tabs.Chronicle;
        const float windowWidth = 500;
        int page = 1;
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;

        double funds;

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start", Core.LogLevel.Important);

            // Adding event handlers
            GameEvents.onLaunch.Add(OnLaunch);
            GameEvents.onVesselRecovered.Add(OnVesselRecovery);
            GameEvents.onVesselWillDestroy.Add(OnVesselDestroy);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
            GameEvents.onFlagPlant.Add(OnFlagPlanted);
            GameEvents.OnKSCFacilityUpgraded.Add(OnFacilityUpgraded);
            GameEvents.OnKSCStructureCollapsed.Add(OnStructureCollapsed);
            GameEvents.OnTechnologyResearched.Add(OnTechnologyResearched);
            GameEvents.onVesselSOIChanged.Add(OnSOIChanged);
            GameEvents.onVesselSituationChange.Add(OnSituationChanged);
            GameEvents.OnFundsChanged.Add(OnFundsChanged);

            // Adding buttons to Toolbar or AppLauncher
            if (ToolbarManager.ToolbarAvailable && Core.UseBlizzysToolbar)
            {
                Core.Log("Registering Blizzy's Toolbar button...", Core.LogLevel.Important);
                toolbarButton = ToolbarManager.Instance.add("SpaceAge", "SpaceAge");
                toolbarButton.Text = "Space Age";
                toolbarButton.TexturePath = "SpaceAge/icon24";
                toolbarButton.ToolTip = "Space Age";
                toolbarButton.OnClick += (e) => { if (window == null) DisplayData(); else UndisplayData(); };
            }
            else
            {
                Core.Log("Registering AppLauncher button...", Core.LogLevel.Important);
                Texture2D icon = new Texture2D(38, 38);
                icon.LoadImage(File.ReadAllBytes(System.IO.Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "icon38.png")));
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(DisplayData, UndisplayData, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, icon);
            }

            funds = Funding.Instance.Funds;
            InitializeProtoAchievements();
        }

        void InitializeProtoAchievements()
        {
            if (protoAchievements != null) return;
            Core.Log("Initializing ProtoAchievements...");
            ConfigNode config = ConfigNode.Load(KSPUtil.ApplicationRootPath + "/GameData/SpaceAge/achievements.cfg");
            protoAchievements = new List<ProtoAchievement>();
            foreach (ConfigNode n in config.GetNodes())
                protoAchievements.Add(new ProtoAchievement(n));
            Core.Log("protoAchievements contains " + protoAchievements.Count + " records.");
        }

        void ParseProgressTracking()
        {
            Core.Log(HighLogic.CurrentGame.scenarios.Count + " scenarios found.");
            ConfigNode trackingNode = null;
            foreach (ProtoScenarioModule psm in HighLogic.CurrentGame.scenarios)
                if (psm.moduleName == "ProgressTracking") trackingNode = psm.GetData();
            if (trackingNode == null)
            {
                Core.Log("ProgressTracking scenario not found!", Core.LogLevel.Important);
                return;
            }
            if (trackingNode.HasNode("Progress"))
                trackingNode = trackingNode.GetNode("Progress");
            else
            {
                Core.Log("ProgressTracking scenario does not contain Progress node!", Core.LogLevel.Error);
                return;
            }
            Core.Log("ProgressTracking config node contains " + trackingNode.CountNodes + " sub-nodes.");
            Achievement a = null;
            foreach (ProtoAchievement pa in protoAchievements)
                if ((pa.StockSynonym != null) && (pa.StockSynonym != "") && (trackingNode.HasNode(pa.StockSynonym)))
                {
                    Core.Log(pa.StockSynonym + " progress node found.");
                    a = new SpaceAge.Achievement(pa);
                    a.Time = Double.Parse(trackingNode.GetValue(pa.StockCompletedString));
                }
        }

        public void OnDisable()
        {
            Core.Log("SpaceAgeScenario.OnDisable");
            UndisplayData();

            // Removing event handlers
            GameEvents.onLaunch.Remove(OnLaunch);
            GameEvents.onVesselRecovered.Remove(OnVesselRecovery);
            GameEvents.onVesselWillDestroy.Remove(OnVesselDestroy);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);
            GameEvents.onFlagPlant.Remove(OnFlagPlanted);
            GameEvents.OnKSCFacilityUpgraded.Remove(OnFacilityUpgraded);
            GameEvents.OnKSCStructureCollapsed.Remove(OnStructureCollapsed);
            GameEvents.OnTechnologyResearched.Remove(OnTechnologyResearched);
            GameEvents.onVesselSOIChanged.Remove(OnSOIChanged);
            GameEvents.onVesselSituationChange.Remove(OnSituationChanged);
            GameEvents.OnFundsChanged.Remove(OnFundsChanged);

            // Removing Toolbar & AppLauncher buttons
            if (toolbarButton != null) toolbarButton.Destroy();
            if ((appLauncherButton != null) && (ApplicationLauncher.Instance != null))
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
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
            chronicle.Clear();
            InitializeProtoAchievements();
            if (node.HasNode("CHRONICLE"))
            {
                Core.Log(node.GetNode("CHRONICLE").CountNodes + " nodes found in Chronicle.");
                int i = 0;
                foreach (ConfigNode n in node.GetNode("CHRONICLE").GetNodes())
                {
                    Core.Log("Processing chronicle node #" + ++i + "...");
                    if (n.name == "EVENT") chronicle.Add(new ChronicleEvent(n));
                }
            }
            if (node.HasNode("ACHIEVEMENTS"))
            {
                Core.Log(node.GetNode("ACHIEVEMENTS").CountNodes + " nodes found in ACHIEVEMENTS.");
                int i = 0;
                foreach (ConfigNode n in node.GetNode("ACHIEVEMENTS").GetNodes())
                    if (n.name == "ACHIEVEMENT")
                    {
                        Core.Log("Processing Achievement node #" + ++i + "...");
                        Achievement a = new Achievement(n);
                        achievements.Add(a.FullName, a);
                    }
            }
        }

        public void AddChronicleEvent(ChronicleEvent e)
        {
            Core.ShowNotification(e.Type + " event detected.");
            chronicle.Add(e);
            if (window != null) Invalidate();
        }

        public static ProtoAchievement FindProtoAchievement(string name)
        {
            Core.Log("Searching among " + protoAchievements.Count + " ProtoAchievements.");
            foreach (ProtoAchievement pa in protoAchievements)
                if (pa.Name == name) return pa;
            Core.Log("ProtoAchievement '" + name + "' not found!", Core.LogLevel.Error);
            return null;
        }

        public Achievement FindAchievement(string name, CelestialBody body = null)
        {
            try { return achievements[Achievement.GetFullName(name, body?.name)]; }
            catch (KeyNotFoundException) { return null; }
            //foreach (Achievement a in achievements)
            //    if ((a.Proto.Name == name) && (!a.Proto.IsBodySpecific || (a.Body == body?.name))) return a;
            //return null;
        }

        public void RegisterAchievement(Achievement ach, Vessel vessel = null, double value = Double.NaN, bool useCurrentTime = true)
        {
            Achievement a = achievements[ach.FullName] ?? ach;
            if (a.Register(vessel, value))
            {
                Core.Log("Achievement registered.");
                achievements[a.FullName] = a;
                if (a.Proto.Type != ProtoAchievement.Types.Total)
                    MessageSystem.Instance.AddMessage(new MessageSystem.Message("Achievement", a.Title + " achievement completed!", MessageSystemButton.MessageButtonColor.YELLOW, MessageSystemButton.ButtonIcons.ACHIEVE));
            }
        }

        void CheckAchievements(string ev, CelestialBody body = null, Vessel vessel = null, double value = 0)
        {
            Core.Log("CheckAchievements('" + ev + "', body = '" + body?.name + "', vessel = '" + vessel?.vesselName + "', " + value + ")");
            foreach (ProtoAchievement pa in protoAchievements)
                if (pa.OnEvent == ev)
                {
                    Core.Log("Checking ProtoAchievement '" + pa.Name + "'...");
                    Achievement ach = new Achievement(pa);
                    ach.Body = body?.name;
                    RegisterAchievement(ach, vessel, value);
                }
        }

        void CheckAchievements(string ev, Vessel v)
        { CheckAchievements(ev, v.mainBody, v); }

        void CheckAchievements(string ev, double v)
        { CheckAchievements(ev, null, null, v); }

        // UI METHODS BELOW


        void DisplayAchievement(Achievement a, List<DialogGUIBase> grid)
        {
            if (a == null) return;
            grid.Add(new DialogGUILabel(a.Title, true));
            grid.Add(new DialogGUILabel(a.DisplayValue, true));
            grid.Add(new DialogGUILabel(a.Proto.HasTime ? KSPUtil.PrintDate(a.Time, true) : "", true));
        }

        public DialogGUIBase windowContents
        {
            get
            {
                List<DialogGUIBase> gridContents;
                switch (currentTab)
                {
                    case Tabs.Chronicle:
                        if (page > PageCount) page = PageCount;
                        gridContents = new List<DialogGUIBase>(LinesPerPage);
                        Core.Log("Displaying events " + ((page - 1) * LinesPerPage + 1) + "-" + Math.Min(page * LinesPerPage, chronicle.Count) + "...");
                        for (int i = (page - 1) * LinesPerPage; i < Math.Min(page * LinesPerPage, chronicle.Count); i++)
                        {
                            Core.Log("chronicle[" + (Core.NewestFirst ? (chronicle.Count - i - 1) : i) + "]: " + chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Description);
                            gridContents.Add(
                                new DialogGUIHorizontalLayout(
                                    new DialogGUILabel(KSPUtil.PrintDateCompact(chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Time, true) + "\t" + chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Description, true),
                                    new DialogGUIButton<int>("X", DeleteItem, Core.NewestFirst ? (chronicle.Count - i - 1) : i)));
                        }
                        return new DialogGUIVerticalLayout(
                            new DialogGUIHorizontalLayout(
                                true,
                                false,
                                new DialogGUIButton("<<", FirstPage, () => (page > 1), false),
                                new DialogGUIButton("<", PageUp, () => (page > 1), false),
                                new DialogGUIHorizontalLayout(TextAnchor.LowerCenter, new DialogGUILabel(page + "/" + PageCount)),
                                new DialogGUIButton(">", PageDown, () => (page < PageCount), false),
                                new DialogGUIButton(">>", LastPage, () => (page < PageCount), false)),
                            new DialogGUIVerticalLayout(windowWidth - 10, 0f, 5f, new RectOffset(5, 5, 0, 0), TextAnchor.UpperLeft, gridContents.ToArray()),
                            (HighLogic.LoadedSceneIsFlight ? new DialogGUIHorizontalLayout() :
                            new DialogGUIHorizontalLayout(
                                windowWidth - 20,
                                10,
                                new DialogGUITextInput("", false, 100, TextInputChanged),
                                new DialogGUIButton("Add", AddItem, false),
                                new DialogGUIButton("Export", ExportChronicle))));

                    case Tabs.Achievements:
                        gridContents = new List<DialogGUIBase>(achievements.Count * 3);
                        Core.Log("Displaying " + achievements.Count + " achievements...");
                        foreach (ProtoAchievement pa in protoAchievements)
                            if (!pa.IsBodySpecific) DisplayAchievement(FindAchievement(pa.Name), gridContents);
                        Core.Log("Displaying body achievements...");
                        foreach (CelestialBody b in FlightGlobals.Bodies)
                            foreach (ProtoAchievement pa in protoAchievements)
                                if (pa.IsBodySpecific) DisplayAchievement(FindAchievement(pa.Name, b), gridContents);
                        return new DialogGUIGridLayout(new RectOffset(5, 5, 0, 0), new Vector2((windowWidth - 10) / 3 - 3, 20), new Vector2(5, 5), UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft, UnityEngine.UI.GridLayoutGroup.Axis.Horizontal, TextAnchor.MiddleLeft, UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount, 3, gridContents.ToArray());
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
                    "Space Age Chronicle", 
                    "", 
                    "Space Age Chronicle", 
                    HighLogic.UISkin,
                    windowPosition,
                    new DialogGUIHorizontalLayout(
                        true, 
                        false,
                        new DialogGUIButton<Tabs>("Chronicle", SelectTab, Tabs.Chronicle, () => (currentTab != Tabs.Chronicle), true),
                        new DialogGUIButton<Tabs>("Achievements", SelectTab, Tabs.Achievements, () => (currentTab != Tabs.Achievements), true)),
                    windowContents),
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
            UndisplayData();
            DisplayData();
        }

        void SelectTab(Tabs t)
        {
            currentTab = t;
            Invalidate();
        }

        int LinesPerPage
        { get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().linesPerPage; } }

        int PageCount
        { get { return (int)System.Math.Ceiling((double)chronicle.Count / LinesPerPage); } }

        public void PageUp()
        {
            if (page > 1) page--;
            Invalidate();
        }

        public void FirstPage()
        {
            page = 1;
            Invalidate();
        }

        public void PageDown()
        {
            if (page < PageCount) page++;
            Invalidate();
        }

        public void LastPage()
        {
            page = PageCount;
            Invalidate();
        }

        public void DeleteItem(int i)
        {
            chronicle.RemoveAt(i);
            Invalidate();
        }

        string textInput = "";
        public string TextInputChanged(string s)
        {
            Core.Log("TextInputChanged('" + s + "')");
            textInput = s;
            return s;
        }

        void ExportChronicle()
        {
            string filename = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/" + ((textInput.Trim(' ') == "") ? "chronicle" : KSPUtil.SanitizeFilename(textInput)) + ".txt";
            Core.Log("ExportChronicle to '" + filename + "'...", Core.LogLevel.Important);
            TextWriter writer = File.CreateText(filename);
            for (int i = 0; i < chronicle.Count; i++)
                writer.WriteLine(KSPUtil.PrintDateCompact(chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Time, true) + "\t" + chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Description);
            writer.Close();
            Core.Log("Done.");
            ScreenMessages.PostScreenMessage("The Chronicle has been exported to GameData\\SpaceAge\\PluginData\\SpaceAge\\" + filename + ".");
            Invalidate();
        }

        void AddItem()
        {
            Core.Log("AddItem (textInput = '" + textInput + "')", Core.LogLevel.Important);
            if (textInput.Trim(' ') == "") return;
            AddChronicleEvent(new SpaceAge.ChronicleEvent("Custom", "description", textInput));
        }

        // EVENT HANDLERS BELOW--USED TO TRACK AND RECORD EVENTS

        public void OnLaunch(EventReport report)
        {
            Core.Log("OnLaunch(<" + report.eventType + ", " + report.origin + ", " + report.sender + ">)", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackLaunch) return;
            if (report.eventType != FlightEvents.LAUNCH)
            {
                Core.Log("Not an actual launch. NO processing.");
                return;
            }
            ChronicleEvent e = new ChronicleEvent("Launch", "vessel", FlightGlobals.ActiveVessel.vesselName);
            if (FlightGlobals.ActiveVessel.GetCrewCount() > 0) e.Data.Add("crew", FlightGlobals.ActiveVessel.GetCrewCount().ToString());
            AddChronicleEvent(e);
            CheckAchievements("Launch", FlightGlobals.ActiveVessel);
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log("OnVesselRecovery('" + v.vesselName + "', " + b + ")", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackRecovery) return;
            Core.Log("missionTime = " + v.missionTime + "; launchTime = " + v.launchTime + "; autoClean = " + v.autoClean);
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.EVA) || (v.vesselType == VesselType.Flag))
            {
                Core.Log(v.vesselName + " is " + v.vesselType + ". NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }
            if (v.missionTime <= 0)
            {
                Core.Log(v.vesselName + " has not been launched. NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }
            ChronicleEvent e = new ChronicleEvent("Recovery", "vessel", v.vesselName);
            if (v.GetVesselCrew().Count > 0) e.Data.Add("crew", v.GetVesselCrew().Count.ToString());
            AddChronicleEvent(e);
            CheckAchievements("Recovery", v.vesselRef);
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log("OnVesselDestroy('" + v.vesselName + "')", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackDestroy) return;
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.Flag) || (v.vesselType == VesselType.EVA) || (v.vesselType == VesselType.SpaceObject))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }
            AddChronicleEvent(new ChronicleEvent("Destroy", "vessel", v.vesselName));
            CheckAchievements("Destroy", v);
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log("OnCrewKilled", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackDeath) return;
            AddChronicleEvent(new ChronicleEvent("Death", "kerbal", report?.sender));
            CheckAchievements("Death", report?.origin?.vessel?.mainBody);
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log("OnFlagPlanted('" + v.vesselName + "')", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackFlagPlant) return;
            AddChronicleEvent(new ChronicleEvent("FlagPlant", "body", v.mainBody.name));
            CheckAchievements("FlagPlant", v);
        }

        public void OnFacilityUpgraded(Upgradeables.UpgradeableFacility facility, int level)
        {
            Core.Log("OnFacilityUpgraded('" + facility.name + "', " + level + ")", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackFacilityUpgraded) return;
            AddChronicleEvent(new ChronicleEvent("FacilityUpgraded", "facility", facility.name, "level", (level + 1).ToString()));
            CheckAchievements("FacilityUpgraded");
        }

        public void OnStructureCollapsed(DestructibleBuilding structure)
        {
            Core.Log("OnStructureCollapsed('" + structure.name + "')", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackStructureCollapsed) return;
            AddChronicleEvent(new ChronicleEvent("StructureCollapsed", "facility", structure.name));
            CheckAchievements("StructureCollapsed");
        }

        public void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a)
        {
            Core.Log("OnTechnologyResearched(<'" + a.host.name + "', '" + a.target.ToString() + "'>)", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackTechnologyResearched) return;
            AddChronicleEvent(new ChronicleEvent("TechnologyResearched", "tech", a.host.title));
            CheckAchievements("TechnologyResearched");
        }

        public void OnSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
        {
            Core.Log("OnSOIChanged(<'" + e.from.name + "', '" + e.to.name + "', '" + e.host.vesselName + "'>)", Core.LogLevel.Important);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().trackSOIChange) return;
            AddChronicleEvent(new SpaceAge.ChronicleEvent("SOIChange", "vessel", e.host.vesselName, "body", e.to.name));
            CheckAchievements("SOIChange", e.to, e.host);
        }

        public void OnSituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            Core.Log("OnSituationChanged(<'" + a.host.vesselName + "', '" + a.to + "'>)");
            switch (a.to)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    CheckAchievements("Landed", a.host);
                    break;
                case Vessel.Situations.ESCAPING:
                case Vessel.Situations.SUB_ORBITAL:
                    CheckAchievements("Flyby", a.host);
                    break;
                case Vessel.Situations.ORBITING:
                    CheckAchievements("Orbit", a.host);
                    break;
            }
        }

        public void OnFundsChanged(double v, TransactionReasons tr)
        {
            Core.Log("OnFundsChanged(" + v + ", " + tr + ")");
            Core.Log("Current funds: " + Funding.Instance.Funds + "; SpaceAgeScenario.funds = " + funds);
            if (v > funds) CheckAchievements("Income", v - funds);
            else CheckAchievements("Expense", funds - v);
            funds = v;
        }
    }
}
