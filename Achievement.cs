using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string DisplayValue
        {
            get
            {
                if (invalid) return "N/A";
                if (!Proto.HasValue) return "";
                return ((Proto.ValueType == ProtoAchievement.ValueTypes.Mass) ? Value.ToString("F2") : Value.ToString("N0")) + " " + Proto.Unit;
            }
        }

        public string Title => invalid ? "N/A" : Proto.Title + (Proto.IsBodySpecific ? " " + Body : "");

        public static string GetFullName(string name, string body = null) => name + (body != null ? "@" + body : "");

        public string FullName => invalid ? "N/A" : GetFullName(Proto.Name, Body);

        public override string ToString() => (Time != Double.NaN ? KSPUtil.PrintDateCompact(Time, true) : "") + "\t" + Title + ((Value != 0 ? (" (" + Value + ")") : ""));

        public bool Register(Achievement old)
        {
            Core.Log("Registering candidate achievement: " + this + ".");
            if (old != null) Core.Log("Old achievement: " + old + ".");
            else Core.Log("Old achievement of this type does not exist.");
            if (invalid)
            {
                Core.Log("This candidate achievement is invalid. Terminating.");
                return false;
            }
            bool res = false;
            if ((old != null) && ((old.Proto != Proto) || (old.Body != Body))) return false;
            switch (Proto.Type)
            {
                case ProtoAchievement.Types.Total:
                    if (old != null) Value += old.Value;
                    res = true;
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
                    if (Proto.HasTime && value.HasValue("time")) Time = Double.Parse(value.GetValue("time"));
                    if (Proto.HasValue && value.HasValue("value")) Value = Double.Parse(value.GetValue("value"));
                }
                catch (Exception) { throw new ArgumentException("Achievement config node is incorrect: " + value); }
            }
        }

        public Achievement(ProtoAchievement proto) => Proto = proto;

        public Achievement(ConfigNode node) => ConfigNode = node;

        public Achievement(ProtoAchievement proto, CelestialBody body = null, Vessel vessel = null, double value = 0)
        {
            Proto = proto;
            if (invalid) return;
            if (body != null) Body = body.name;
            if (Proto.HasTime) Time = Planetarium.GetUniversalTime();
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
            if (proto.CrewedOnly && ((vessel == null) || (vessel.GetCrewCount() == 0))) invalid = true;
        }

        bool invalid = false;
    }
}
