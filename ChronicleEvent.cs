using KSP.Localization;
using System;
using System.Collections.Generic;
using UniLinq;

namespace SpaceAge
{
    public class ChronicleEvent
    {
        public long Time { get; set; }

        public string Type { get; set; }

        public bool LogOnly { get; set; } = false;

        protected Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

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

                    case "Takeoff":
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Takeoff_Crew", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Takeoff_NoCrew", GetString("vessel"), Core.GetBodyDisplayName(GetString("body")));

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
                            ? (HasData("vessel") ? Localizer.Format("#SpaceAge_CE_Achievement_Vessel_Value", GetString("vessel"), GetString("title"), GetString("value")) : Localizer.Format("#SpaceAge_CE_Achievement_Value", GetString("title"), GetString("value")))
                            : (HasData("vessel") ? Localizer.Format("#SpaceAge_CE_Achievement_Vessel_NoValue", GetString("vessel"), GetString("title")) : Localizer.Format("#SpaceAge_CE_Achievement_NoValue", GetString("title")));

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
                if (LogOnly)
                    node.AddValue("logOnly", true);
                foreach (KeyValuePair<string, string> kvp in Data)
                    node.AddValue(kvp.Key, kvp.Value);
                return node;
            }
            set
            {
                Time = value.GetLongOrDouble("time", -1);
                Type = value.GetValue("type");
                LogOnly = value.GetBool("logOnly");
                foreach (ConfigNode.Value v in value.values)
                    if ((v.name != "time") && (v.name != "type") && (v.name != "logOnly") && (v.value.Length != 0))
                        AddData(v.name, v.value);
            }
        }

        public ChronicleEvent() => Time = (long)Planetarium.GetUniversalTime();

        public ChronicleEvent(string type, params object[] data)
            : this()
        {
            Core.Log($"Constructing {type} event with {data.Length} params.");
            Type = type;
            for (int i = 0; i < data.Length; i++)
            {
                AddData(data[i]);
                if (data[i] is string s)
                {
                    AddData(s, data[i + 1]);
                    i++;
                }
            }
        }

        public ChronicleEvent(ConfigNode node) => ConfigNode = node;

        public void AddData(object data)
        {
            if (data == null)
                return;
            if (data is Vessel v)
            {
                AddData("vessel", v.vesselName);
                AddData("vesselId", v.id.ToString());
            }
            if (data is ProtoVessel pv)
            {
                AddData("vessel", pv.vesselName);
                AddData("vesselId", pv.vesselID.ToString());
            }
        }

        public void AddData(string key, object value)
        {
            if (Data.ContainsKey(key))
                Core.Log($"Key {key} already exists in ChronicleEvent {Type}.", LogLevel.Error);
            else Data.Add(key, value.ToString());
        }

        public bool HasData(string key) => Data.ContainsKey(key);

        public string GetString(string key) => HasData(key) ? Data[key] : null;

        public int GetInt(string key) => HasData(key) ? (int.TryParse(Data[key], out int res) ? res : 0) : 0;

        public List<string> GetVesselIds()
            => new List<string>(Data
                .Where(kvp => kvp.Key.Contains("vesselId"))
                .Select(kvp => kvp.Value));

        public bool HasVesselId() => GetVesselIds().Count > 0;

        public bool HasVesselId(string vesselId) => GetVesselIds().Contains(vesselId);
    }
}
