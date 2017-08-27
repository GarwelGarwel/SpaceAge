using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    class SpaceAgeScenario : ScenarioModule
    {
        List<ChronicleEvent> chronicle = new List<ChronicleEvent>();

        public void Start()
        {
            Core.Log("SpaceAgeScenario.Start");
            GameEvents.onLaunch.Add(OnLaunch);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
        }

        public void OnDisable()
        {
            Core.Log("SpaceAgeScenario.OnDisable");
            GameEvents.onLaunch.Remove(OnLaunch);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
        }

        public void OnLaunch(EventReport data)
        {
            ScreenMessages.PostScreenMessage("Launch detected!");
            Core.Log("OnLaunch(EventReport): data.eventType = " + data.eventType + "; data.msg = " + data.msg + "; data.origin = " + data.origin + "; data.other = " + data.other + "; param = " + data.param + "; data.sender = " + data.sender + "; data.stage = " + data.stage);
            chronicle.Add(new VesselEvent(VesselEvent.EventType.Launch, FlightGlobals.ActiveVessel.name));
        }

        public void OnVesselDestroy(Vessel v)
        {
            ScreenMessages.PostScreenMessage("Vessel destruction detected!");
            chronicle.Add(new VesselEvent(VesselEvent.EventType.Destroy, v.name));
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
