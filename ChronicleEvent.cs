using KSP.Localization;
using System;
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
            catch (FormatException)
            { return 0; }
        }

        public List<string> GetVesselIds()
        {
            List<string> ids = new List<string>();
            foreach (KeyValuePair<string, string> kvp in Data)
                if (kvp.Key.Contains("vesselId"))
                    ids.Add(kvp.Value);
            return ids;
        }

        public bool HasVesselId(string vesselId) => GetVesselIds().Contains(vesselId);

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case "Launch":
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Launch_Crew", GetString("vessel"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Launch_NoCrew", GetString("vessel"));
                    case "ReachSpace":
                        return Localizer.Format("#SpaceAge_CE_ReachSpace", GetString("vessel"));
                    case "SOIChange":
                        return Localizer.Format("#SpaceAge_CE_SOIChange", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "Orbit":
                        return Localizer.Format("#SpaceAge_CE_Orbit", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "Reentry":
                        return Localizer.Format("#SpaceAge_CE_Reentry", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "Docking":
                        return Localizer.Format("#SpaceAge_CE_Docking", GetString("vessel1"), GetString("vessel2"));
                    case "Undocking":
                        return Localizer.Format("#SpaceAge_CE_Undocking", GetString("vessel1"), GetString("vessel2"));
                    case "Landing":
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Landing_Crew", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Landing_NoCrew", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "Recovery":
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Recovery_Crew", GetString("vessel"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Recovery_NoCrew", GetString("vessel"));
                    case "ReturnFromOrbit":
                        return Localizer.Format("#SpaceAge_CE_ReturnFromOrbit", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "ReturnFromSurface":
                        return Localizer.Format("#SpaceAge_CE_ReturnFromSurface", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));
                    case "Destroy":
                        return HasData("body")
                            ? Localizer.Format("#SpaceAge_CE_Destroy_Body", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")))
                            : Localizer.Format("#SpaceAge_CE_Destroy_NoBody", GetString("vessel"));
                    case "Death":
                        return Localizer.Format("#SpaceAge_CE_Death", GetString("kerbal"));
                    case "FlagPlant":
                        return Localizer.Format("#SpaceAge_CE_FlagPlant", Core.GetBodyDisplayName(GetString("body")));
                    case "FacilityUpgraded":
                        return Localizer.Format("#SpaceAge_CE_FacilityUpgraded", GetString("facility"), GetString("level"));
                    case "StructureCollapsed":
                        return Localizer.Format("#SpaceAge_CE_StructureCollapsed", GetString("facility"));
                    case "TechnologyResearched":
                        return Localizer.Format("#SpaceAge_CE_TechnologyResearched", GetString("tech"));
                    case "AnomalyDiscovery":
                        return Localizer.Format("#SpaceAge_CE_AnomalyDiscovery", GetString("id"), Core.GetBodyDisplayName(GetString("body")));
                    case "Achievement":
                        return (HasData("value") && GetString("value").Length != 0)
                            ? Localizer.Format("#SpaceAge_CE_Achievement_Value", GetString("title"), GetString("value"))
                            : Localizer.Format("#SpaceAge_CE_Achievement_NoValue", GetString("title"));
                    case "Custom":
                        return GetString("description");
                }
                return Localizer.Format("#SpaceAge_CE_Unknown", Type);
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

        public ChronicleEvent(string type, params object[] data)
            : this()
        {
            Core.Log("Constructing " + type + " event with " + data.Length + " params.");
            Type = type;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] is Vessel v)
                {
                    Data.Add("vessel", v.vesselName);
                    Data.Add("vesselId", v.id.ToString());
                }
                if (data[i] is ProtoVessel pv)
                {
                    Data.Add("vessel", pv.vesselName);
                    Data.Add("vesselId", pv.vesselID.ToString());
                }
                if (data[i] is string s)
                {
                    Data.Add(s, data[i + 1] as string ?? data[i + 1].ToString());
                    i++;
                }
            }
        }

        public ChronicleEvent(ConfigNode node) => ConfigNode = node;
    }
}
