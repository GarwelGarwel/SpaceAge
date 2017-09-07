using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    public class Achievement
    {
        ProtoAchievement protoAchievement;
        public ProtoAchievement Proto
        {
            get { return protoAchievement; }
            set { protoAchievement = value; }
        }

        string body = null;
        public string Body
        {
            get { return Proto.IsBodySpecific ? body : null; }
            set { body = value; }
        }

        double time = Double.NaN;
        public double Time
        {
            get { return Proto.HasTime ? time : Double.NaN; }
            set { time = value; }
        }

        double value = 0;
        public double Value
        {
            get { return Proto.HasValue ? value : 0; }
            set { this.value = value; }
        }

        public string DisplayValue
        {
            get
            {
                if (!Proto.HasValue) return "";
                return ((Proto.ValueType == ProtoAchievement.ValueTypes.Mass) ? Value.ToString("F2") : Value.ToString("F0")) + " " + Proto.Unit;
            }
        }

        public string Title
        {
            get { return Proto.Title + (Proto.IsBodySpecific ? " " + Body : ""); }
        }

        public bool Register()
        {
            if (Proto.ExcludeHome && (Body == FlightGlobals.GetHomeBodyName())) return false;
            switch (Proto.Type)
            {
                case ProtoAchievement.Types.Total:
                    Value++;
                    return true;
                case ProtoAchievement.Types.Max:
                    return false;
                case ProtoAchievement.Types.First:
                    if (!Double.IsNaN(Time)) return false;
                    Time = Planetarium.GetUniversalTime();
                    return true;
            }
            return false;
        }

        public bool Register(double v)
        {
            switch (Proto.Type)
            {
                case ProtoAchievement.Types.Total:
                    Value += v;
                    return true;
                case ProtoAchievement.Types.Max:
                    if (v <= Value) return false;
                    Value = v;
                    Time = Planetarium.GetUniversalTime();
                    return true;
                case ProtoAchievement.Types.First:
                    return Register();
            }
            return false;
        }

        public bool Register(Vessel vessel = null, double value = 0)
        {
            if (Proto.CrewedOnly && (vessel.GetCrewCount() == 0)) return false;
            switch (Proto.ValueType)
            {
                case ProtoAchievement.ValueTypes.Cost: return Register(0); // NOT IMPLEMENTED
                case ProtoAchievement.ValueTypes.Mass: return Register(vessel.totalMass);
                case ProtoAchievement.ValueTypes.PartsNum: return Register(vessel.parts.Count);
                case ProtoAchievement.ValueTypes.CrewNum: return Register(vessel.GetCrewCount());
                case ProtoAchievement.ValueTypes.Scalar: return Register(value);
            }
            return Register(1);
        }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("ACHIEVEMENT");
                node.AddValue("name", Proto.Name);
                if (Proto.IsBodySpecific) node.AddValue("body", Body);
                if (Proto.HasTime) node.AddValue("time", Time);
                if (Proto.HasValue) node.AddValue("value", Value);
                return node;
            }
            set
            {
                if (value.name != "ACHIEVEMENT") return;
                Core.Log("Loading '" + value.GetValue("name") + "' achievement...");
                Proto = SpaceAgeScenario.FindProtoAchievement(value.GetValue("name"));
                if (Proto.IsBodySpecific && value.HasValue("body")) Body = value.GetValue("body");
                if (Proto.HasTime && value.HasValue("time")) Time = Double.Parse(value.GetValue("time"));
                if (Proto.HasValue && value.HasValue("value")) Value = Double.Parse(value.GetValue("value"));
            }
        }

        public Achievement(ProtoAchievement proto)
        { Proto = proto; }

        public Achievement(ConfigNode node)
        { ConfigNode = node; }
    }
}
