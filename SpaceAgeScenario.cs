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

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start");

            // Adding event listeners
            GameEvents.onLaunch.Add(OnLaunch);
            GameEvents.onVesselRecovered.Add(OnVesselRecovery);
            GameEvents.onVesselWillDestroy.Add(OnVesselDestroy);

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
            if ((appLauncherButton != null) && (ApplicationLauncher.Instance != null))
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }

        public void DisplayData()
        {
            string s = "";
            int i = 0;
            foreach (ChronicleEvent e in chronicle)
            {
                s += ++i + "\t";
                s += KSPUtil.PrintDateCompact(e.Time, true) + "\t";
                s += e.Description + "\r\n";
            }
            MessageSystem.Instance.AddMessage(new MessageSystem.Message("Space Age Chronicle", s, MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.ACHIEVE));
        }

        public void UndisplayData()
        { }

        public void OnLaunch(EventReport data)
        {
            Core.Log("OnLaunch");
            ScreenMessages.PostScreenMessage("Launch detected!");
            chronicle.Add(new VesselEvent(VesselEvent.EventType.Launch, FlightGlobals.ActiveVessel.vesselName));
        }

        public void OnVesselRecovery(ProtoVessel v, bool b)
        {
            Core.Log("OnVesselRecovery('" + v.vesselName + "', " + b + ")");
            Core.Log("messionTime = " + v.missionTime + "; launchTime = " + v.launchTime + "; autoClean = " + v.autoClean);
            ScreenMessages.PostScreenMessage("Vessel recovery detected!");
            chronicle.Add(new VesselEvent(VesselEvent.EventType.Recovery, v.vesselName));
        }

        public void OnVesselDestroy(Vessel v)
        {
            Core.Log("OnVesselDestroy('" + v.vesselName + "')");
            if ((v.vesselType == VesselType.Debris) || (v.vesselType == VesselType.Flag))
            {
                Core.Log(v.name + " is " + v.vesselType + ". NO adding to Chronicle.");
                return;
            }
            ScreenMessages.PostScreenMessage("Vessel destruction detected!");
            chronicle.Add(new VesselEvent(VesselEvent.EventType.Destroy, v.vesselName));
        }

        public override void OnSave(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnSave");
            ConfigNode chronicleNode = new ConfigNode("Chronicle");
            foreach (ChronicleEvent e in chronicle)
                chronicleNode.AddNode(e.ConfigNode);
            Core.Log(chronicleNode.CountNodes + " nodes saved in the chronicle.");
            node.AddNode(chronicleNode);
        }

        public override void OnLoad(ConfigNode node)
        {
            Core.Log("SpaceAgeScenario.OnLoad");
            chronicle.Clear();
            if (!node.HasNode("Chronicle"))
            {
                Core.Log("'Chronicle' node not found. Aborting OnLoad.", Core.LogLevel.Error);
                return;
            }
            Core.Log(node.GetNode("Chronicle").CountNodes + " nodes found in Chronicle.");
            int i = 0;
            foreach (ConfigNode n in node.GetNode("Chronicle").GetNodes())
            {
                Core.Log("Processing chronicle node #" + ++i + "...");
                switch (n.name)
                {
                    case "VesselEvent":
                        chronicle.Add(new VesselEvent(n));
                        break;
                }
            }
        }
    }
}
