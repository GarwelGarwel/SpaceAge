using KSP.Localization;
using System;
using System.Collections.Generic;
using UniLinq;

namespace SpaceAge
{
    public class ChronicleEvent : IConfigNode
    {
        #region EVENT NAMES

        public const string Launch = "Launch";
        public const string ReachSpace = "ReachSpace";
        public const string Staging = "Staging";
        public const string Burn = "Burn";
        public const string Orbit = "Orbit";
        public const string SOIChange = "SOIChange";
        public const string Reentry = "Reentry";
        public const string Docking = "Docking";
        public const string Undocking = "Undocking";
        public const string Landing = "Landing";
        public const string Takeoff = "Takeoff";
        public const string Recovery = "Recovery";
        public const string ReturnFromOrbit = "ReturnFromOrbit";
        public const string ReturnFromSurface = "ReturnFromSurface";
        public const string Destroy = "Destroy";
        public const string Death = "Death";
        public const string FlagPlant = "FlagPlant";
        public const string FacilityUpgraded = "FacilityUpgraded";
        public const string StructureCollapsed = "StructureCollapsed";
        public const string TechnologyResearched = "TechnologyResearched";
        public const string AnomalyDiscovery = "AnomalyDiscovery";
        public const string Achievement = "Achievement";
        public const string Custom = "Custom";

        #endregion EVENT NAMES

        public long Time { get; set; }

        public string Type { get; set; }

        public bool LogOnly { get; set; } = false;

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case Launch:
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Launch_Crew", GetString("vessel"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Launch_NoCrew", GetString("vessel"));

                    case ReachSpace:
                        return Localizer.Format("#SpaceAge_CE_ReachSpace", GetString("vessel"));

                    case Staging:
                        return Localizer.Format("#SpaceAge_CE_Staging", GetString("vessel"), GetInt("stage"));

                    case Burn:
                        return Localizer.Format("#SpaceAge_CE_Burn", GetString("vessel"), Core.DateTimeFormatter.PrintTimeStampCompact(GetInt("duration")), GetInt("deltaV").ToString("N0"));

                    case Orbit:
                        return Localizer.Format("#SpaceAge_CE_Orbit", GetString("vessel"), GetBodyName("body"));

                    case SOIChange:
                        return Localizer.Format("#SpaceAge_CE_SOIChange", GetString("vessel"), GetBodyName("body"));

                    case Reentry:
                        return Localizer.Format("#SpaceAge_CE_Reentry", GetString("vessel"), GetBodyName("body"));

                    case Docking:
                        return Localizer.Format("#SpaceAge_CE_Docking", GetString("vessel1"), GetString("vessel2"));

                    case Undocking:
                        return Localizer.Format("#SpaceAge_CE_Undocking", GetString("vessel1"), GetString("vessel2"));

                    case Landing:
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Landing_Crew", GetString("vessel"), GetBodyName("body"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Landing_NoCrew", GetString("vessel"), GetBodyName("body"));

                    case Takeoff:
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Takeoff_Crew", GetString("vessel"), GetBodyName("body"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Takeoff_NoCrew", GetString("vessel"), GetBodyName("body"));

                    case Recovery:
                        return HasData("crew")
                            ? Localizer.Format("#SpaceAge_CE_Recovery_Crew", GetString("vessel"), GetInt("crew"))
                            : Localizer.Format("#SpaceAge_CE_Recovery_NoCrew", GetString("vessel"));

                    case ReturnFromOrbit:
                        return Localizer.Format("#SpaceAge_CE_ReturnFromOrbit", GetString("vessel"), GetBodyName("body"));

                    case ReturnFromSurface:
                        return Localizer.Format("#SpaceAge_CE_ReturnFromSurface", GetString("vessel"), GetBodyName("body"));

                    case Destroy:
                        return HasData("body")
                            ? Localizer.Format("#SpaceAge_CE_Destroy_Body", GetString("vessel"), GetBodyName("body"))
                            : Localizer.Format("#SpaceAge_CE_Destroy_NoBody", GetString("vessel"));

                    case Death:
                        return Localizer.Format("#SpaceAge_CE_Death", GetString("kerbal"));

                    case FlagPlant:
                        return Localizer.Format("#SpaceAge_CE_FlagPlant", GetString("kerbal") ?? Localizer.Format("#SpaceAge_Kerbal"), GetBodyName("body"));

                    case FacilityUpgraded:
                        return Localizer.Format("#SpaceAge_CE_FacilityUpgraded", GetString("facility"), GetString("level"));

                    case StructureCollapsed:
                        return Localizer.Format("#SpaceAge_CE_StructureCollapsed", GetString("facility"));

                    case TechnologyResearched:
                        return Localizer.Format("#SpaceAge_CE_TechnologyResearched", GetString("tech"));

                    case AnomalyDiscovery:
                        return Localizer.Format("#SpaceAge_CE_AnomalyDiscovery", GetString("id"), GetBodyName("body"));

                    case Achievement:
                        return (HasData("value") && Data["value"].Length != 0)
                            ? (HasData("vessel") ? Localizer.Format("#SpaceAge_CE_Achievement_Vessel_Value", Data["vessel"], GetString("title"), Data["value"]) : Localizer.Format("#SpaceAge_CE_Achievement_Value", GetString("title"), Data["value"]))
                            : (HasData("vessel") ? Localizer.Format("#SpaceAge_CE_Achievement_Vessel_NoValue", Data["vessel"], GetString("title")) : Localizer.Format("#SpaceAge_CE_Achievement_NoValue", GetString("title")));

                    case Custom:
                        return GetString("description");
                }
                return Localizer.Format("#SpaceAge_CE_Unknown", Type);
            }
        }

        public IEnumerable<string> VesselIds => Data.Where(kvp => kvp.Key.Contains("vesselId")).Select(kvp => kvp.Value);

        public IEnumerable<Vessel> Vessels => VesselIds.Select(id => FlightGlobals.FindVessel(new Guid(id)));

        public Vessel Vessel => Vessels.FirstOrDefault();

        public bool Valid => !string.IsNullOrEmpty(Type) && Time >= 0;

        protected IDictionary<string, string> Data { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public ChronicleEvent() => Time = (long)Planetarium.GetUniversalTime();

        public ChronicleEvent(string type, params object[] data)
            : this()
        {
            Core.Log($"Constructing {type} event with {data.Length} params.");
            Type = type;
            for (int i = 0; i < data.Length; i++)
                switch (data[i])
                {
                    case null:
                        Core.Log($"Parameter #{i + 1} for chronicle event {Type} is unexpectedly null.", LogLevel.Important);
                        continue;

                    case string s:
                        if (data.Length > ++i)
                            AddData(s, data[i]);
                        else Core.Log($"Unexpected last parameter '{s}' for chronicle event {Type}.", LogLevel.Error);
                        break;

                    case Vessel v:
                        AddData("vessel", v.vesselName);
                        AddData("vesselId", v.id.ToString());
                        break;

                    case ProtoVessel pv:
                        AddData("vessel", pv.vesselName);
                        AddData("vesselId", pv.vesselID.ToString());
                        break;

                    default:
                        Core.Log($"Unrecognized parameter #{i + 1} for chronicle event {Type}: {data} (type: {data.GetType()})", LogLevel.Error);
                        break;
                }
        }

        public ChronicleEvent(ConfigNode node) => Load(node);

        public void Save(ConfigNode node)
        {
            node.AddValue("time", Time);
            node.AddValue("type", Type);
            if (LogOnly)
                node.AddValue("logOnly", true);
            foreach (KeyValuePair<string, string> kvp in Data)
                node.AddValue(kvp.Key, kvp.Value);
        }

        public void Load(ConfigNode node)
        {
            Time = node.GetLongOrDouble("time", -1);
            Type = node.GetValue("type");
            LogOnly = node.GetBool("logOnly");
            for (int i = 0; i < node.CountValues; i++)
                if (node.values[i].name != "time" && node.values[i].name != "type" && node.values[i].name != "logOnly" && node.values[i].value.Length != 0)
                    AddData(node.values[i].name, node.values[i].value);
        }

        public void AddData(string key, object value)
        {
            if (Data.ContainsKey(key))
                Core.Log($"Key {key} already exists in ChronicleEvent {Type}.", LogLevel.Error);
            else Data.Add(key, value.ToString());
        }

        public bool HasData(string key) => Data.ContainsKey(key);

        public string GetString(string key) => HasData(key) ? Data[key] : null;

        public string GetBodyName(string key) => Core.GetBodyDisplayName(GetString(key)) ?? Localizer.Format("#SpaceAge_Invalid");

        public int GetInt(string key) => HasData(key) ? (int.TryParse(Data[key], out int res) ? res : 0) : 0;

        public bool HasVesselId() => Data.Any(kvp => kvp.Key.Contains("vesselId"));

        public bool HasVesselId(string vesselId) => VesselIds.Contains(vesselId);
    }
}
