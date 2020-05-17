﻿using System;
using System.Collections.Generic;

namespace SpaceAge
{
    class ChronicleEvent
    {
        public double Time { get; set; }

        public string Type { get; set; }

        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public bool HasData(string key) => Data.ContainsKey(key);

        public string GetString(string key) => HasData(key) ? Data[key] : null;

        public int GetInt(string key)
        {
            try { return HasData(key) ? int.Parse(Data[key]) : 0; }
            catch (FormatException) { return 0; }
        }

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case "Launch":
                        return GetString("vessel") + " was launched" + (HasData("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case "ReachSpace":
                        return GetString("vessel") + " reached space.";
                    case "SOIChange":
                        return GetString("vessel") + " reached " + GetString("body") + "'s sphere of influence.";
                    case "Orbit":
                        return GetString("vessel") + " entered orbit around " + GetString("body") + ".";
                    case "Reentry":
                        return GetString("vessel") + " entered atmosphere of " + GetString("body") + ".";
                    case "Docking":
                        return GetString("vessel1") + " docked with " + GetString("vessel2") + ".";
                    case "Undocking":
                        return GetString("vessel1") + " undocked from " + GetString("vessel2") + ".";
                    case "Landing":
                        return GetString("vessel") + " landed on " + GetString("body") + (HasData("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case "Recovery":
                        return GetString("vessel") + " was recovered" + (HasData("crew") ? " with a crew of " + GetInt("crew") : "") + ".";
                    case "ReturnFromOrbit":
                        return GetString("vessel") + " returned from a " + GetString("body") + "'s orbit.";
                    case "ReturnFromSurface":
                        return GetString("vessel") + " returned from a " + GetString("body") + "'s surface.";
                    case "Destroy":
                        return GetString("vessel") + " was destroyed" + (HasData("body") ? " at " + GetString("body") : "") + ".";
                    case "Death":
                        return GetString("kerbal") + " died.";
                    case "FlagPlant":
                        return "A flag was planted on " + GetString("body") + ".";
                    case "FacilityUpgraded":
                        return GetString("facility") + " was upgraded to level " + GetString("level") + ".";
                    case "StructureCollapsed":
                        return GetString("facility") + " collapsed.";
                    case "TechnologyResearched":
                        return GetString("tech") + " was researched.";
                    case "AnomalyDiscovery":
                        return GetString("id") + " anomaly was discovered on " + GetString("body") + ".";
                    case "Achievement":
                        return GetString("title") + ((HasData("value") && GetString("value").Length != 0) ? " (" + GetString("value") + ")" : "") + " achievement completed!";
                    case "Custom":
                        return GetString("description");
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
                foreach (ConfigNode.Value v in value.values)
                    if ((v.name != "time") && (v.name != "type"))
                        Data.Add(v.name, v.value);
            }
        }

        public ChronicleEvent() => Time = Planetarium.GetUniversalTime();

        public ChronicleEvent(string type, params string[] data)
            : this()
        {
            Core.Log("Constructing " + type + " event with " + data.Length + " params.");
            Type = type;
            for (int i = 0; i < data.Length; i += 2)
                Data.Add(data[i], data[i + 1]);
        }

        public ChronicleEvent(ConfigNode node) => ConfigNode = node;
    }
}
