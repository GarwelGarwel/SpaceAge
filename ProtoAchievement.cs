using System;
using System.Collections.Generic;

namespace SpaceAge
{
    public enum AchievementType { Max, Total, First };

    public enum ValueType { None = 0, Cost, Mass, PartsCount, CrewCount, TotalAssignedCrew, Funds };

    public enum HomeConditionType { Default = 0, Only, Exclude };

    public class ProtoAchievement
    {
        public string Name { get; set; }

        string title = null;

        public string Title
        {
            get => title ?? (Name + (IsBodySpecific ? " @ " : ""));
            set => title = value;
        }

        public AchievementType Type { get; set; }

        public bool HasValue => (Type == AchievementType.Max) || (Type == AchievementType.Total);
        public bool HasTime => (Type == AchievementType.First) || (Type == AchievementType.Max);

        public ValueType ValueType { get; set; } = ValueType.None;

        public List<string> OnEvents { get; set; } = new List<string>();
        public bool IsBodySpecific { get; set; }
        public HomeConditionType Home { get; set; } = HomeConditionType.Default;
        public bool CrewedOnly { get; set; }
        public bool Unique { get; set; }
        public string StockSynonym { get; set; } = null;
        public string ScoreName { get; set; }
        public double Score { get; set; }

        public ConfigNode ConfigNode
        {
            set
            {
                try
                {
                    Core.Log("Loading protoachievement " + value.GetValue("name") + "...");
                    Name = value.GetValue("name");
                    Title = value.GetString("title");
                    Type = (AchievementType)Enum.Parse(typeof(AchievementType), value.GetValue("type"), true);
                    if (value.HasValue("valueType"))
                        ValueType = (ValueType)Enum.Parse(typeof(ValueType), value.GetValue("valueType"), true);
                    OnEvents = value.GetValuesList("onEvent");
                    IsBodySpecific = value.GetBool("bodySpecific");
                    if (value.HasValue("home"))
                        Home = (HomeConditionType)Enum.Parse(typeof(HomeConditionType), value.GetValue("home"), true);
                    CrewedOnly = value.GetBool("crewedOnly");
                    Unique = value.GetBool("unique");
                    StockSynonym = value.GetString("stockSynonym");
                    ScoreName = value.GetString("scoreName", OnEvents.Count > 0 ? OnEvents[0] : null);
                    Score = value.GetDouble("score");
                }
                catch (Exception) { Core.Log("Error parsing a ProtoAchievement node: " + value, LogLevel.Error); }
            }
        }

        public ProtoAchievement() { }

        public ProtoAchievement(string name) => Name = name;
        
        public ProtoAchievement(ConfigNode node) => ConfigNode = node;
    }
}
