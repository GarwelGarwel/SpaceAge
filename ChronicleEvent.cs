using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class ChronicleEvent
    {
        public double Time
        {
            get { return time; }
            set { time = value; }
        }

        public string Type
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
            return r;
        }

        public int GetInt(string key)
        {
            string r;
            Data.TryGetValue(key, out r);
            try { return int.Parse(r); }
            catch (FormatException e) { return 0; }
        }

        public string Name
        { get { return Type.ToString(); } }

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case "Launch": return GetString("vessel") + " launched" + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case "Landing": return GetString("vessel") + " landed on " + GetString("body") + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "");
                    case "Orbit": return GetString("vessel") + " entered orbit around " + GetString("body") + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "");
                    case "Recovery": return GetString("vessel") + " was recovered" + (Data.ContainsKey("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case "Destroy": return GetString("vessel") + " was destroyed.";
                    case "Death": return GetString("kerbal") + " died.";
                    case "FlagPlant": return "A flag was planted on " + GetString("body") + ".";
                    case "FacilityUpgraded": return GetString("facility") + " was upgraded to level " + GetString("level") + ".";
                    case "StructureCollapsed": return GetString("facility") + " collapsed.";
                    case "TechnologyResearched": return GetString("tech") + " was researched.";
                    case "SOIChange": return GetString("vessel") + " reached " + GetString("body") + "'s sphere of influencce.";
                    case "Custom": return GetString("description");
                }
                return "Something happened.";
            }
        }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("EVENT");
                node.AddValue("time", Time);
                node.AddValue("type", Type);
                foreach (KeyValuePair<string, string> kvp in Data)
                    node.AddValue(kvp.Key, kvp.Value);
                return node;
            }
            set
            {
                Time = Double.Parse(value.GetValue("time"));
                Type = value.GetValue("type");
                Core.Log("Loading data for ConfigNode (" + value.CountValues + " values)...");
                foreach (ConfigNode.Value v in value.values)
                    if ((v.name != "time") && (v.name != "type"))
                        Data.Add(v.name, v.value);
            }
        }

        public ChronicleEvent()
        { Time = Planetarium.GetUniversalTime(); }

        public ChronicleEvent(string type)
            : this()
        { Type = type; }

        public ChronicleEvent(string type, params string[] data)
            : this(type)
        {
            Core.Log("Constructing " + type + " event with " + data.Length + " params.");
            for (int i = 0; i < data.Length; i += 2)
                Data.Add(data[i], data[i + 1]);
        }

        public ChronicleEvent(ConfigNode node)
        { ConfigNode = node; }

        double time;
        string type;
        Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
}
