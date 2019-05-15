using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class ProtoScoreRecord
    {
        /// <summary>
        /// Internal name of the score record, defaults to EventType
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of event the record is associated with; must be one of OnEvent values (e.g. "Flyby", "Orbit" etc.)
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// How much score this record costs for an uncrewed vessel at body multiplier of 1
        /// </summary>
        public double BaseScore { get; set; }

        /// <summary>
        /// How much score is added to the base value for a crewed vessel (2 * BaseScore by default)
        /// </summary>
        public double BaseCrewBonus { get; set; }

        /// <summary>
        /// Whether this score record is available for the home body (i.e. Kerbin) and/or other bodies;
        /// - Default = available for all bodies
        /// - Only = available for the home body only
        /// - Exclude = available for all bodies except home
        /// </summary>
        public ProtoAchievement.HomeCountTypes Home { get; set; } = ProtoAchievement.HomeCountTypes.Default;

        public static readonly string ConfigNodeName = "PROTOSCORERECORD";
        public ConfigNode ConfigNode
        {
            set
            {
                EventType = Core.GetString(value, "type");
                Name = Core.GetString(value, "name", EventType);
                BaseScore = Core.GetDouble(value, "score");
                BaseCrewBonus = Core.GetDouble(value, "crewBonus", 2 * BaseScore);
                if (value.HasValue("home")) Home = (ProtoAchievement.HomeCountTypes)Enum.Parse(typeof(ProtoAchievement.HomeCountTypes), value.GetValue("home"), true);
            }
        }

        public ProtoScoreRecord() { }
        public ProtoScoreRecord(ConfigNode node) => ConfigNode = node;
    }
}
