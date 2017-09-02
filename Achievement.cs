using System;
using System.Collections.Generic;

namespace SpaceAge
{
    public class Achievement
    {
        public static string[] generalAchievements = {
            "MostExpensiveVessel",
            "HeaviestVessel",
            "MostComplexVessel",
            "MaxCrewInVessel",
            "TotalFunds",
            "TotalLaunches",
            "TotalCrewedLaunches",
            "TotalMassLaunched",
            "TotalCrewLaunched",
            "FirstLaunch",
            "FirstSpace",
            "FirstOrbit",
            "FirstRecovery",
            "FirstDestroy",
            "FirstDeath"
        };

        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public enum Types { Maximum, Total, First };
        public Types Type
        {
            get
            {
                switch (Name)
                {
                    case "MostExpensiveVessel":
                    case "HeaviestVessel":
                    case "MostComplexVessel":
                    case "MaxCrewInVessel":
                        return Achievement.Types.Maximum;
                    case "TotalFunds":
                    case "TotalLaunches":
                    case "TotalCrewedLaunches":
                    case "TotalMassLaunched":
                    case "TotalCrewLaunched":
                        return Achievement.Types.Total;
                    case "FirstLaunch":
                    case "FirstSpace":
                    case "FirstOrbit":
                    case "FirstRecovery":
                    case "FirstDestroy":
                    case "FirstDeath":
                        return Achievement.Types.First;
                }
                return Achievement.Types.Total;
            }
        }

        public string Description
        {
            get
            {
                switch (Name)
                {
                    case "MostExpensiveVessel": return "Most Expensive Vessel";
                    case "HeaviestVessel": return "Heaviest Vessel";
                    case "MostComplexVessel": return "Most Complex Vessel";
                    case "MaxCrewInVessel": return "Max Crew In a Vessel On Launch";
                    case "TotalFunds": return "Total Funds Earned";
                    case "TotalLaunches": return "Total Launches";
                    case "TotalCrewedLaunches": return "Total Crewed Launches";
                    case "TotalMassLaunched": return "Total Mass of Launched Vessels";
                    case "TotalCrewLaunched": return "Total Crew of Launched Vessels";
                    case "FirstLaunch": return "First Launch";
                    case "FirstSpace": return "First Reaching the Space";
                    case "FirstOrbit": return "First Orbit";
                    case "FirstRecovery": return "First Recovery";
                    case "FirstDestroy": return "First Destroyed Ship";
                    case "FirstDeath": return "First Death of a Kerbal";
                }
                return "Unknown achievement";
            }
        }

        public bool HasValue
        { get { return (Type == Types.Maximum) || (Type == Types.Total); } }

        double value = 0;
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public string Unit
        {
            get
            {
                switch (Name)
                {
                    default: return "";
                }
            }
        }

        public bool HasTime
        { get { return (Type == Types.First) || (Type == Types.Maximum); } }

        public double time = Double.NaN;
        public double Time
        {
            get { return time; }
            set { time = value; }
        }

        public static string RelevantEvent(string name)
        {
            switch (name)
            {
                case "MostExpensiveVessel":
                case "HeaviestVessel":
                case "MostComplexVessel":
                case "MaxCrewInVessel":
                case "TotalLaunches":
                case "TotalCrewedLaunches":
                case "TotalMassLaunched":
                case "TotalCrewLaunched":
                case "FirstLaunch":
                    return "Launch";
                case "TotalFunds": return "FundsChanged";
                case "FirstSpace": return "First Reaching the Space";
                case "FirstOrbit": return "First Orbit";
                case "FirstRecovery": return "Recovery";
                case "FirstDestroy": return "Destroy";
                case "FirstDeath": return "Death";
            }
            return "";
        }

        public static List<string> RelevantAchievements(string eventName)
        {
            List<string> res = new List<string>();
            foreach (string achievementName in generalAchievements)
                if (RelevantEvent(achievementName) == eventName)
                    res.Add(achievementName);
            return res;
        }

        public bool Register()
        {
            if (Type == Types.First && Double.IsNaN(Time))
            {
                Time = Planetarium.GetUniversalTime();
                return true;
            }
            return false;
        }

        public bool Register(double value)
        {
            if ((Type == Types.Maximum) && (value > Value))
            {
                Value = value;
                Time = Planetarium.GetUniversalTime();
                return true;
            }
            if ((Type == Types.Total) && (value > 0))
            {
                Value += value;
                return true;
            }
            return false;
        }

        public bool Register(Vessel v)
        {
            switch (Name)
            {
                case "MostExpensiveVessel":
                    return false;  // Not implemented
                case "HeaviestVessel":
                case "TotalMassLaunched":
                    return Register(v.totalMass);
                case "MostComplexVessel":
                    return Register(v.Parts.Count);
                case "MaxCrewInVessel":
                case "TotalCrewLaunched":
                    return Register(v.GetCrewCount());
                case "TotalLaunches":
                    return Register(1);
                case "TotalCrewedLaunches":
                    return (v.GetCrewCount() > 0) && Register(1);
                case "FirstLaunch":
                    return Register();
                case "FirstRecovery": return Register();
                case "FirstDestroy": return Register();
            }
            return false;
        }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode node = new ConfigNode("ACHIEVEMENT");
                node.AddValue("name", Name);
                if (HasValue) node.AddValue("value", Value);
                if (HasTime) node.AddValue("time", Time);
                return node;
            }
            set
            {
                Name = value.GetValue("name");
                if (value.HasValue("value")) Value = Double.Parse(value.GetValue("value"));
                if (value.HasValue("time")) Time = Double.Parse(value.GetValue("time"));
            }
        }

        public Achievement() { }
        public Achievement(string name) { Name = name; }
        public Achievement(ConfigNode node) { ConfigNode = node; }
    }
}
