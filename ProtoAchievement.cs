using System;
using System.Collections.Generic;

namespace SpaceAge
{
    public class ProtoAchievement
    {
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string title = null;
        public string Title
        {
            get { return title ?? (Name + (IsBodySpecific ? " @ " : "")); }
            set { title = value; }
        }

        bool bodySpecific = false;
        public bool IsBodySpecific
        {
            get { return bodySpecific; }
            set { bodySpecific = value; }
        }

        bool excludeHome = false;
        public bool ExcludeHome
        {
            get { return excludeHome; }
            set { excludeHome = value; }
        }

        public enum Types { Max, Total, First };
        Types type;
        public Types Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool HasValue
        { get { return (Type == Types.Max) || (Type == Types.Total); } }

        public enum ValueTypes { None, Cost, Mass, PartsCount, CrewCount, Funds };
        ValueTypes valueType = ValueTypes.None;
        public ValueTypes ValueType
        {
            get { return valueType; }
            set { valueType = value; }
        }

        public string Unit
        {
            get
            {
                switch (ValueType)
                {
                    case ValueTypes.Funds:
                    case ValueTypes.Cost: return "£";
                    case ValueTypes.Mass: return "t";
                    case ValueTypes.PartsCount: return "parts";
                }
                return "";
            }
        }

        public bool HasTime
        { get { return (Type == Types.First) || (Type == Types.Max); } }

        string onEvent = "";
        public string OnEvent
        {
            get { return onEvent; }
            set { onEvent = value; }
        }

        bool crewedOnly = false;
        public bool CrewedOnly
        {
            get { return crewedOnly; }
            set { crewedOnly = value; }
        }

        string stockSynonym = null;
        public string StockSynonym
        {
            get { return stockSynonym; }
            set { stockSynonym = value; }
        }

        public Achievement GetAchievement()
        { return new SpaceAge.Achievement(this); }

        public ConfigNode ConfigNode
        {
            set
            {
                try
                {
                    Core.Log("Loading protoachievement " + value.GetValue("name") + "...");
                    Name = value.GetValue("name");
                    if (value.HasValue("title")) Title = value.GetValue("title");
                    if (value.HasValue("onEvent")) OnEvent = value.GetValue("onEvent");
                    Type = (Types)Enum.Parse(typeof(Types), value.GetValue("type"), true);
                    if (value.HasValue("valueType")) ValueType = (ValueTypes)Enum.Parse(typeof(ValueTypes), value.GetValue("valueType"), true);
                    if (value.HasValue("bodySpecific")) IsBodySpecific = Boolean.Parse(value.GetValue("bodySpecific"));
                    if (value.HasValue("excludeHome")) ExcludeHome = Boolean.Parse(value.GetValue("excludeHome"));
                    if (value.HasValue("crewedOnly")) CrewedOnly = Boolean.Parse(value.GetValue("crewedOnly"));
                    if (value.HasValue("stockSynonym")) StockSynonym = value.GetValue("stockSynonym");
                }
                catch (Exception) { Core.Log("Error parsing a ProtoAchievement node: " + value, Core.LogLevel.Error); }
            }
        }

        public ProtoAchievement() { }
        public ProtoAchievement(string name) { Name = name; }
        public ProtoAchievement(ConfigNode node) { ConfigNode = node; }
    }
}
