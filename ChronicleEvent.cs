using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class ChronicleEvent
    {
        public enum EventType { Launch, Recovery, Destroy, Death, FlagPlant };

        public double Time
        {
            get { return time; }
            set { time = value; }
        }


        public EventType Type
        {
            get { return type; }
            set { type = value; }
        }

        public Dictionary<string, string> Data
        {
            get { return data; }
            set { data = value; }
        }

        public bool HasData(string key)
        {
            return Data.ContainsKey(key);
        }

        public string GetString(string key)
        {
            string r;
            Data.TryGetValue(key, out r);
            return r as string;
        }

        public int GetInt(string key)
        {
            string r;
            Data.TryGetValue(key, out r);
            return int.Parse(r);
        }

        public string Name
        { get { return Type.ToString(); } }

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case EventType.Launch: return GetString("vesselName") + " launched" + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case EventType.Recovery: return GetString("vesselName") + " was recovered" + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case EventType.Destroy: return GetString("vesselName") + " was destroyed.";
                    case EventType.Death: return GetString("kerbalName") + " was killed.";
                    case EventType.FlagPlant: return "A flag was planted on " + GetString("body") + ".";
                }
                return null;
            }
        }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("EVENT");
                node.AddValue("time", Time);
                node.AddValue("type", Type.ToString());
                foreach (KeyValuePair<string, string> kvp in Data)
                {
                    ConfigNode subnode = new ConfigNode("DATA");
                    subnode.AddValue("key", kvp.Key);
                    subnode.AddValue("value", kvp.Value);
                    node.AddNode(subnode);
                }
                return node;
            }
            set
            {
                Time = Double.Parse(value.GetValue("time"));
                Type = (EventType)Enum.Parse(typeof(EventType), value.GetValue("type"));
                Core.Log("Loading data for ConfigNode (" + value.CountNodes + " subnodes)...");
                foreach (ConfigNode node in value.GetNodes("DATA"))
                {
                    Core.Log("Loading node with key '" + node.GetValue("key") + "', value " + node.GetValue("value") + "'...");
                    Data.Add(node.GetValue("key"), node.GetValue("value"));
                }
            }
        }

        public ChronicleEvent()
        { Time = Planetarium.GetUniversalTime(); }

        public ChronicleEvent(EventType type)
            : this()
        { Type = type; }

        public ChronicleEvent(EventType type, params string[] data)
            : this(type)
        {
            Core.Log("Constructing " + type + " event with " + data.Length + " params.");
            for (int i = 0; i < data.Length; i += 2)
                Data.Add(data[i], data[i + 1]);
        }

        public ChronicleEvent(ConfigNode node)
        { ConfigNode = node; }

        //public ChronicleEvent(double time)
        //{ Time = time; }

        double time;
        EventType type;
        Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
}
