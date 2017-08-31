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

        IButton toolbarButton;
        ApplicationLauncherButton appLauncherButton;
        const float windowWidth = 500;
        int page = 1;
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;

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
                icon.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "icon38.png")));
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(DisplayData, UndisplayData, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, icon);
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
            Core.Log(chronicleNode.CountNodes + " nodes saved in the chronicle.");
            node.AddNode(chronicleNode);
        }

        public override void OnLoad(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnLoad");
            chronicle.Clear();
            if (!node.HasNode("CHRONICLE"))
            {
                Core.Log("'CHRONICLE' node not found. Aborting OnLoad.", Core.LogLevel.Error);
                return;
            }
            Core.Log(node.GetNode("CHRONICLE").CountNodes + " nodes found in Chronicle.");
            int i = 0;
            foreach (ConfigNode n in node.GetNode("CHRONICLE").GetNodes())
            {
                Core.Log("Processing chronicle node #" + ++i + "...");
                if (n.name == "EVENT") chronicle.Add(new ChronicleEvent(n));
            }
        }

        public void AddChronicleEvent(ChronicleEvent e)
        {
            Core.ShowNotification(e.Type + " event detected.");
            chronicle.Add(e);
            if (window != null) Invalidate();
        }

        // UI METHODS BELOW

        public void DisplayData()
        {
            Core.Log("DisplayData", Core.LogLevel.Important);
            if (chronicle.Count == 0)
            {
                Core.Log("Chronicle is empty. Aborting.", Core.LogLevel.Important);
                ScreenMessages.PostScreenMessage("You don't have any entries in the Chronicle yet. Do something first!");
                return;
            }
            if (page > PageCount) page = PageCount;
            List<DialogGUIBase> gridContents = new List<DialogGUIBase>(LinesPerPage);
            Core.Log("Displaying lines " + ((page - 1) * LinesPerPage + 1) + "-" + Math.Min(page * LinesPerPage, chronicle.Count) + "...");
            for (int i = (page - 1) * LinesPerPage; i < Math.Min(page * LinesPerPage, chronicle.Count); i++)
            {
                Core.Log("chronicle[" + (Core.NewestFirst ? (chronicle.Count - i - 1) : i) + "]: " + chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Description);
                gridContents.Add(
                    new DialogGUIHorizontalLayout(
                        new DialogGUILabel(KSPUtil.PrintDateCompact(chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Time, true) + "\t" + chronicle[Core.NewestFirst ? (chronicle.Count - i - 1) : i].Description, true),
                        new DialogGUIButton<int>("X", DeleteItem, Core.NewestFirst ? (chronicle.Count - i - 1) : i)));
            }
            Core.Log("Now displaying the window...");

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
                        new DialogGUIButton("<<", FirstPage, PageUpEnabled, false), 
                        new DialogGUIButton("<", PageUp, PageUpEnabled, false), 
                        new DialogGUIHorizontalLayout(TextAnchor.LowerCenter, new DialogGUILabel(page + "/" + PageCount)), 
                        new DialogGUIButton(">", PageDown, PageDownEnabled, false), 
                        new DialogGUIButton(">>", LastPage, PageDownEnabled, false)),
                    new DialogGUIVerticalLayout(windowWidth - 10, 0f, 5f, new RectOffset(5, 5, 5, 5), TextAnchor.UpperLeft, gridContents.ToArray())),
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

        int LinesPerPage
        {
            get { return HighLogic.CurrentGame.Parameters.CustomParams<SpaceAgeChronicleSettings>().linesPerPage; }
        }

        int PageCount
        { get { return (int)System.Math.Ceiling((double)chronicle.Count / LinesPerPage); } }

        public bool PageUpEnabled()
        { return page > 1; }

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

        public bool PageDownEnabled()
        { return page < PageCount; }

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

        // EVENT HANDLERS BELOW--USED TO TRACK AND RECORD EVENTS

        public void OnLaunch(EventReport report)
        {
            Core.Log("OnLaunch", Core.LogLevel.Important);
            ChronicleEvent e = new ChronicleEvent("Launch", "vessel", FlightGlobals.ActiveVessel.vesselName);
            if (FlightGlobals.ActiveVessel.GetCrewCount() > 0) e.Data.Add("crew", FlightGlobals.ActiveVessel.GetCrewCount().ToString());
            AddChronicleEvent(e);
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log("OnVesselRecovery('" + v.vesselName + "', " + b + ")", Core.LogLevel.Important);
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
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log("OnVesselDestroy('" + v.vesselName + "')", Core.LogLevel.Important);
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.Flag) || (v.vesselType == VesselType.EVA) || (v.vesselType == VesselType.SpaceObject))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.", Core.LogLevel.Important);
                return;
            }
            AddChronicleEvent(new ChronicleEvent("Destroy", "vessel", v.vesselName));
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log("OnCrewKilled", Core.LogLevel.Important);
            AddChronicleEvent(new ChronicleEvent("Death", "kerbal", report.sender));
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log("OnFlagPlanted('" + v.vesselName + "')", Core.LogLevel.Important);
            AddChronicleEvent(new ChronicleEvent("FlagPlant", "body", v.mainBody.name));
        }

        public void OnFacilityUpgraded(Upgradeables.UpgradeableFacility facility, int level)
        {
            Core.Log("OnFacilityUpgraded('" + facility.name + "', " + level + ")", Core.LogLevel.Important);
            AddChronicleEvent(new ChronicleEvent("FacilityUpgraded", "facility", facility.name, "level", (level + 1).ToString()));
        }

        public void OnStructureCollapsed(DestructibleBuilding structure)
        {
            Core.Log("OnStructureCollapsed('" + structure.name + "')", Core.LogLevel.Important);
            AddChronicleEvent(new ChronicleEvent("StructureCollapsed", "facility", structure.name));
        }

        public void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> a)
        {
            Core.Log("OnTechnologyResearched(<'" + a.host.name + "', '" + a.target.ToString() + "'>)", Core.LogLevel.Important);
            AddChronicleEvent(new ChronicleEvent("TechnologyResearched", "tech", a.host.title));
        }

        public void OnSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> a)
        {
            Core.Log("OnSOIChanged(<'" + a.from.name + "', '" + a.to.name + "', '" + a.host.vesselName + "'>)", Core.LogLevel.Important);
            AddChronicleEvent(new SpaceAge.ChronicleEvent("SOIChange", "vessel", a.host.vesselName, "body", a.to.name));
        }
    }
}
