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

        ApplicationLauncherButton appLauncherButton;
        const float windowWidth = 500;
        int linesPerPage = 10;
        int page = 1;
        Rect windowPosition = new Rect(0.5f, 0.5f, windowWidth, 50);
        PopupDialog window;

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start");

            // Adding event listeners
            GameEvents.onLaunch.Add(OnLaunch);
            GameEvents.onVesselRecovered.Add(OnVesselRecovery);
            GameEvents.onVesselWillDestroy.Add(OnVesselDestroy);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
            GameEvents.onFlagPlant.Add(OnFlagPlanted);
            //GameEvents.onCrash.Add(OnVesselCrash);

            Core.Log("Registering AppLauncher button...", Core.LogLevel.Important);
            Texture2D icon = new Texture2D(38, 38);
            icon.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "icon38.png")));
            appLauncherButton = ApplicationLauncher.Instance.AddModApplication(DisplayData, UndisplayData, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, icon);
        }

        public void OnDisable()
        {
            Core.Log("SpaceAgeScenario.OnDisable");
            GameEvents.onLaunch.Remove(OnLaunch);
            GameEvents.onVesselRecovered.Remove(OnVesselRecovery);
            GameEvents.onVesselWillDestroy.Remove(OnVesselDestroy);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);
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
                switch (n.name)
                {
                    case "EVENT":
                        chronicle.Add(new ChronicleEvent(n));
                        break;
                }
            }
        }

        // UI METHODS BELOW

        public void DisplayData()
        {
            Core.Log("DisplayData", Core.LogLevel.Important);
            List<DialogGUIBase> gridContents = new List<DialogGUIBase>(linesPerPage + 1);
            // Creating column titles
            //gridContents.Add(new DialogGUILabel("#", true));
            //gridContents.Add(new DialogGUILabel("Date", true));
            //gridContents.Add(new DialogGUILabel("Event", true));
            Core.Log("Displaying lines " + ((page - 1) * linesPerPage + 1) + "-" + Math.Min(page * linesPerPage, chronicle.Count) + "...");
            for (int i = (page - 1) * linesPerPage; i < Math.Min(page * linesPerPage, chronicle.Count); i++)
            {
                Core.Log("chronicle[" + i + "]: " + chronicle[i].Description);
                gridContents.Add(new DialogGUILabel(KSPUtil.PrintDateCompact(chronicle[i].Time, true) + "\t" + chronicle[i].Description, windowWidth));
                //gridContents.Add(new DialogGUILabel((i + 1).ToString(), true));
                //gridContents.Add(new DialogGUILabel(KSPUtil.PrintDateCompact(chronicle[i].Time, true), true));
                //gridContents.Add(new DialogGUILabel(chronicle[i].Description, true));
            }
            Core.Log("Now displaying the window...");

            window = PopupDialog.SpawnPopupDialog(
                new Vector2(0.5f, 0.5f), 
                new Vector2(0.5f, 0.5f), 
                new MultiOptionDialog(
                    "Space Age Chronicle", 
                    "", 
                    "Space Age Chronicle", 
                    HighLogic.UISkin, 
                    windowPosition, 
                    new DialogGUIGridLayout(new RectOffset(0, 0, 0, 0), new Vector2(100, 30), new Vector2(20, 0), UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft, UnityEngine.UI.GridLayoutGroup.Axis.Vertical, TextAnchor.MiddleCenter, UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount, 1, gridContents.ToArray()),
                    new DialogGUIHorizontalLayout(new DialogGUIButton("<", PageDown), new DialogGUILabel(page + "/" + PageCount), new DialogGUIButton(">", PageUp))),
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

        int PageCount
        { get { return (int)System.Math.Ceiling((double)chronicle.Count / linesPerPage); } }

        public void PageUp()
        {
            if (page > 1) page--;
            UndisplayData();
            DisplayData();
        }

        public void PageDown()
        {
            if (page < PageCount) page++;
            UndisplayData();
            DisplayData();
        }

        // EVENT HANDLERS BELOW--USED TO TRACK AND RECORD EVENTS

        public void OnLaunch(EventReport report)
        {
            Core.Log("OnLaunch");
            ScreenMessages.PostScreenMessage("Launch detected!");
            ChronicleEvent e = new ChronicleEvent(ChronicleEvent.EventType.Launch, "vesselName", FlightGlobals.ActiveVessel.vesselName);
            if (FlightGlobals.ActiveVessel.GetCrewCount() > 0) e.Data.Add("crew", FlightGlobals.ActiveVessel.GetCrewCount().ToString());
            chronicle.Add(e);

        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log("OnVesselRecovery('" + v.vesselName + "', " + b + ")");
            Core.Log("missionTime = " + v.missionTime + "; launchTime = " + v.launchTime + "; autoClean = " + v.autoClean);
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.EVA) || (v.vesselType == VesselType.Flag))
            {
                Core.Log(v.vesselName + " is " + v.vesselType + ". NO adding to Chronicle.");
                return;
            }
            if (v.missionTime <= 0)
            {
                Core.Log(v.vesselName + " has not been launched. NO adding to Chronicle.");
                return;
            }
            ScreenMessages.PostScreenMessage("Vessel recovery detected!");
            ChronicleEvent e = new ChronicleEvent(ChronicleEvent.EventType.Recovery, "vesselName", v.vesselName);
            if (v.GetVesselCrew().Count > 0) e.Data.Add("crew", v.GetVesselCrew().Count.ToString());
            chronicle.Add(e);
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log("OnVesselDestroy('" + v.vesselName + "')");
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.Flag) || (v.vesselType == VesselType.EVA))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.");
                return;
            }
            ScreenMessages.PostScreenMessage("Vessel destruction detected!");
            chronicle.Add(new ChronicleEvent(ChronicleEvent.EventType.Destroy, "vesselName", v.vesselName));
        }

        public void OnVesselCrash(EventReport report)
        {
            Vessel v = report.origin.vessel;
            Core.Log("OnVesselCrash('" + v.vesselName + "')");
            Core.Log("EventReport: " + report.eventType + "; " + report.msg + "; " + report.origin + "; " + report.other + "; " + report.param + "; " + report.sender + "; " + report.stage);

            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.Flag) || (v.vesselType == VesselType.EVA))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.");
                return;
            }
            ScreenMessages.PostScreenMessage("Vessel crash detected!");
            chronicle.Add(new ChronicleEvent(ChronicleEvent.EventType.Destroy, "vesselName", v.vesselName));
        }

        public void OnCrewKilled(EventReport report)
        {
            Core.Log("OnCrewKilled");
            ScreenMessages.PostScreenMessage("Crew kill detected!");
            chronicle.Add(new ChronicleEvent(ChronicleEvent.EventType.Death, "kerbalName", report.sender));
        }

        public void OnFlagPlanted(Vessel v)
        {
            Core.Log("OnFlagPlanted('" + v.vesselName + "')");
            ScreenMessages.PostScreenMessage("Flag planting detected!");
            chronicle.Add(new SpaceAge.ChronicleEvent(ChronicleEvent.EventType.FlagPlant, "body", v.mainBody.name));
        }
    }
}
