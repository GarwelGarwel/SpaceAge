using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class VesselEvent : ChronicleEvent
    {
        public enum EventType { Launch, Destroy };
        EventType type;
        string vesselName;

        public EventType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string VesselName
        {
            get { return vesselName; }
            set { vesselName = value; }
        }

        public override string Name
        {
            get
            {
                switch (Type)
                {
                    case EventType.Launch: return "Launch";
                    case EventType.Destroy: return "Destroy";
                }
                return "N/A";
            }
        }

        public override string Description
        {
            get
            {
                switch (Type)
                {
                    case EventType.Launch: return VesselName + " launched.";
                    case EventType.Destroy: return VesselName + " destroyed.";
                }
                return null;
            }
        }

        public override ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("VesselEvent");
                node.AddValue("time", Time);
                node.AddValue("type", type.ToString());
                node.AddValue("vesselName", VesselName);
                return node;
            }
            set
            {
                if (value.name != "VesselEvent")
                {
                    Core.Log("Error loading VesselEvent config node! Node is " + value.name + "!");
                    return;
                }
                Time = Double.Parse(value.GetValue("time"));
                Type = (EventType)Enum.Parse(typeof(EventType), value.GetValue("type"));
                VesselName = value.GetValue("vesselName");
            }
        }

        public VesselEvent(EventType type, string vesselName) : base()
        {
            Type = type;
            VesselName = vesselName;
        }

        public VesselEvent(ConfigNode node)
        { ConfigNode = node; }

        public VesselEvent() : base()
        { }
    }
}
