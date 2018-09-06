using System;
using System.Collections.Generic;

namespace SpaceAge
{
    public class ProtoAchievement
    {
        public string Name { get; set; }

        string title = null;
        public string Title
        {
            get => title ?? (Name + (IsBodySpecific ? " @ " : ""));
            set => title = value;
        }

        public string OnEvent { get; set; } = "";
        public bool IsBodySpecific { get; set; } = false;

        public enum HomeCountTypes { Default, Only, Exclude };
        public HomeCountTypes Home { get; set; } = HomeCountTypes.Default;

        public enum Types { Max, Total, First };
        public Types Type { get; set; }

        public bool HasValue => (Type == Types.Max) || (Type == Types.Total);

        public enum ValueTypes { None, Cost, Mass, PartsCount, CrewCount, Funds };
        public ValueTypes ValueType { get; set; } = ValueTypes.None;

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

        public bool Unique = false;

        public bool HasTime => (Type == Types.First) || (Type == Types.Max);

        public bool CrewedOnly { get; set; } = false;

        public string StockSynonym { get; set; } = null;

        public Achievement GetAchievement() => new SpaceAge.Achievement(this);

        public ConfigNode ConfigNode
        {
            set
            {
                try
                {
                    Core.Log("Loading protoachievement " + value.GetValue("name") + "...");
                    Name = value.GetValue("name");
                    Title = Core.GetString(value, "title");
                    OnEvent = Core.GetString(value, "onEvent", "");
                    Type = (Types)Enum.Parse(typeof(Types), value.GetValue("type"), true);
                    if (value.HasValue("valueType")) ValueType = (ValueTypes)Enum.Parse(typeof(ValueTypes), value.GetValue("valueType"), true);
                    Unique = Core.GetBool(value, "unique");
                    IsBodySpecific = Core.GetBool(value, "bodySpecific");
                    if (value.HasValue("home")) Home = (HomeCountTypes)Enum.Parse(typeof(HomeCountTypes), value.GetValue("home"), true);
                    //Home = Core.GetBool(value, "excludeHome");
                    CrewedOnly = Core.GetBool(value, "crewedOnly");
                    StockSynonym = Core.GetString(value, "stockSynonym");
                }
                catch (Exception) { Core.Log("Error parsing a ProtoAchievement node: " + value, Core.LogLevel.Error); }
            }
        }

        public ProtoAchievement() { }
        public ProtoAchievement(string name) => Name = name;
        public ProtoAchievement(ConfigNode node) => ConfigNode = node;
    }
}
