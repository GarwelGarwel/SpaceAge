using System;
using System.Collections.Generic;

namespace SpaceAge
{
    public enum AchievementType
    {
        Max,
        Total,
        First
    };

    public enum ValueType
    {
        None = 0,
        Cost,
        Mass,
        PartsCount,
        CrewCount,
        TotalAssignedCrew,
        Funds,
        Science
    };

    public enum HomeConditionType
    {
        Default = 0,
        Only,
        Exclude
    };

    public class ProtoAchievement
    {
        string title = null;

        public string Name { get; set; }

        public string Title
        {
            get => title ?? $"{Name}{(IsBodySpecific ? " @ " : "")}";
            set => title = value;
        }

        public AchievementType Type { get; set; }

        public bool HasValue => Type == AchievementType.Max || Type == AchievementType.Total;

        public bool HasTime => Type == AchievementType.First || Type == AchievementType.Max;

        public ValueType ValueType { get; set; } = ValueType.None;

        public IEnumerable<string> OnEvents { get; set; } = new List<string>();

        public bool IsBodySpecific { get; set; }

        public HomeConditionType Home { get; set; } = HomeConditionType.Default;

        public bool CrewedOnly { get; set; }

        public bool Unique { get; set; }

        public string StockSynonym { get; set; } = null;

        public string ScoreName { get; set; }

        public double Score { get; set; }

        public ProtoAchievement(string name) => Name = name;

        public ProtoAchievement(ConfigNode node) => Load(node);

        public void Load(ConfigNode node)
        {
            try
            {
                Core.Log($"Loading protoachievement {node.GetValue("name")}...");
                Name = node.GetValue("name");
                Title = node.GetString("title");
                Type = (AchievementType)Enum.Parse(typeof(AchievementType), node.GetValue("type"), true);
                if (node.HasValue("valueType"))
                    ValueType = (ValueType)Enum.Parse(typeof(ValueType), node.GetValue("valueType"), true);
                OnEvents = node.GetValuesList("onEvent");
                IsBodySpecific = node.GetBool("bodySpecific");
                if (node.HasValue("home"))
                    Home = (HomeConditionType)Enum.Parse(typeof(HomeConditionType), node.GetValue("home"), true);
                CrewedOnly = node.GetBool("crewedOnly");
                Unique = node.GetBool("unique");
                StockSynonym = node.GetString("stockSynonym");
                ScoreName = node.GetString("scoreName");
                Score = node.GetDouble("score");
            }
            catch (Exception)
            {
                Core.Log($"Error parsing a ProtoAchievement node: {node}", LogLevel.Error);
            }
        }
    }
}
