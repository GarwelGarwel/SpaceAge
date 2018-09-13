using System;

namespace SpaceAge
{
    public class Achievement
    {
        ProtoAchievement proto;
        public ProtoAchievement Proto
        {
            get => proto;
            set
            {
                proto = value;
                if (value == null) invalid = true;
            }
        }

        string body = null;
        public string Body
        {
            get => invalid ? "N/A" : Proto.IsBodySpecific ? body : null;
            set => body = value;
        }

        double time = Double.NaN;
        public double Time
        {
            get => (!invalid && Proto.HasTime) ? time : Double.NaN;
            set => time = value;
        }

        double value = 0;
        public double Value
        {
            get => (!invalid && Proto.HasValue) ? value : 0;
            set => this.value = value;
        }

        string hero;
        public string Hero
        {
            get => !invalid ? hero : null;
            set => hero = value;
        }

        public string Ids { get; set; } = "";
        void AddId(string id) => Ids += "[" + id + "]";
        bool ContainsId(string id) => Ids.Contains("[" + id + "]");

        public string ShortDisplayValue => invalid ? "N/A" : Proto.HasValue ? ((Proto.ValueType == ProtoAchievement.ValueTypes.Mass) ? Value.ToString("N2") : Value.ToString("N0")) + " " + Proto.Unit : "";
        public string FullDisplayValue => invalid ? "N/A" : Proto.HasValue ? ((Proto.ValueType == ProtoAchievement.ValueTypes.Mass) ? Value.ToString("N2") : Value.ToString("N0")) + " " + Proto.Unit + (Hero != null ? " (" + Hero + ")" : "") : (Hero ?? "");

        public string Title => invalid ? "N/A" : Proto.Title + (Proto.IsBodySpecific ? " " + Body : "");

        public static string GetFullName(string name, string body = null) => name + (body != null ? "@" + body : "");

        public string FullName => invalid ? "N/A" : GetFullName(Proto.Name, Body);

        public override string ToString() => (Time != Double.NaN ? KSPUtil.PrintDateCompact(Time, true) : "") + "\t" + Title + ((Value != 0 ? (" (" + Value + ")") : ""));

        public bool Register(Achievement old)
        {
            Core.Log("Registering candidate achievement: " + this + ".");
            if (invalid)
            {
                Core.Log("This candidate achievement is invalid. Terminating.");
                return false;
            }
            if (old != null) Core.Log("Old achievement: " + old + ".");
            else Core.Log("Old achievement of this type does not exist.");
            if ((old != null) && ((old.Proto != Proto) || (old.Body != Body))) return false;
            bool res = false;
            switch (Proto.Type)
            {
                case ProtoAchievement.Types.Total:
                    Core.Log("Unique: " + Proto.Unique + ". Id: " + Ids + ". Old achievement's ids: " + (old?.Ids ?? "N/A"));
                    if ((old == null) || !Proto.Unique || !old.ContainsId(Ids))
                    {
                        if (old != null)
                        {
                            Value += old.Value;
                            if (Proto.Unique) Ids += old.Ids;
                        }
                        res = true;
                    }
                    else res = false;
                    break;
                case ProtoAchievement.Types.Max:
                    if ((old == null) || (Value > old.Value)) res = true;
                    break;
                case ProtoAchievement.Types.First:
                    if ((old == null) || (Time < old.Time)) res = true;
                    break;
            }
            if (res) Core.Log("Registration successful: achievement completed!");
            else Core.Log("Registration failed: this doesn't qualify as an achievement.");
            return res;
        }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("ACHIEVEMENT");
                if (invalid) return node;
                node.AddValue("name", Proto.Name);
                if (Proto.IsBodySpecific) node.AddValue("body", Body);
                if (Proto.HasTime) node.AddValue("time", Time);
                if (Proto.HasValue) node.AddValue("value", Value);
                if (Hero != null) node.AddValue("hero", Hero);
                if (Proto.Unique) node.AddValue("ids", Ids);
                return node;
            }
            set
            {
                try
                {
                    if (value.name != "ACHIEVEMENT") throw new Exception();
                    Core.Log("Loading '" + value.GetValue("name") + "' achievement...");
                    Proto = SpaceAgeScenario.FindProtoAchievement(value.GetValue("name"));
                    if (invalid) return;
                    if (Proto.IsBodySpecific && value.HasValue("body")) Body = value.GetValue("body");
                    if (Proto.HasTime) Time = Core.GetDouble(value, "time");
                    if (Proto.HasValue) Value = Core.GetDouble(value, "value");
                    Hero = Core.GetString(value, "hero");
                    if (Proto.Unique) Ids = Core.GetString(value, "ids", "");
                }
                catch (Exception) { throw new ArgumentException("Achievement config node is incorrect: " + value); }
            }
        }

        public Achievement(ConfigNode node) => ConfigNode = node;

        public Achievement(ProtoAchievement proto, CelestialBody body = null, Vessel vessel = null, double value = 0, string hero = null)
        {
            Proto = proto;
            if (invalid) return;
            if (body != null) Body = body.name;
            switch (Proto.Home)
            {
                case ProtoAchievement.HomeCountTypes.Default: break;
                case ProtoAchievement.HomeCountTypes.Only: invalid = FlightGlobals.GetHomeBody() != body; break;
                case ProtoAchievement.HomeCountTypes.Exclude: invalid = FlightGlobals.GetHomeBody() == body; break;
            }
            if (Proto.HasTime)
            {
                Time = Planetarium.GetUniversalTime();
                Hero = hero ?? vessel?.vesselName;
            }
            if (hero != null) AddId(hero);
            else if (vessel != null) AddId(vessel.id.ToString());
            if (Proto.HasValue)
                switch (Proto.ValueType)
                {
                    case ProtoAchievement.ValueTypes.Cost: Value = Core.VesselCost(vessel); break;
                    case ProtoAchievement.ValueTypes.Mass: Value = vessel.totalMass; break;
                    case ProtoAchievement.ValueTypes.PartsCount: Value = vessel.parts.Count; break;
                    case ProtoAchievement.ValueTypes.CrewCount: Value = vessel.GetCrewCount(); break;
                    case ProtoAchievement.ValueTypes.Funds: Value = value; break;
                    default: Value = 1; break;
                }
            if (Proto.CrewedOnly && ((vessel == null) || (vessel.GetCrewCount() == 0))) invalid = true;
        }

        bool invalid = false;
    }
}
