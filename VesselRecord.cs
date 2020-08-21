using System;

namespace SpaceAge
{
    public class VesselRecord
    {
        /// <summary>
        /// Vessel's Guid as a string
        /// </summary>
        public string Id { get; protected set; }

        public Guid Guid
        {
            get => new Guid(Id);
            protected set => Id = value.ToString();
        }

        /// <summary>
        /// Vessel's user-readable name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Universal Time of launch
        /// </summary>
        public double LaunchTime { get; set; }

        /// <summary>
        /// Reference to the vessel (may be null if the vessel is destroyed)
        /// </summary>
        public Vessel Vessel
        {
            get => FlightGlobals.FindVessel(Guid);
            set
            {
                if (value != null)
                {
                    Guid = value.id;
                    Name = value.vesselName;
                    LaunchTime = value.launchTime;
                }
            }
        }

        /// <summary>
        /// Time since vessel's launch (may or may not be equal to MissionTime)
        /// </summary>
        public double Age => Planetarium.GetUniversalTime() - LaunchTime;

        public ConfigNode ConfigNode
        {
            get
            {
                if (Id == null)
                    return null;
                ConfigNode node = new ConfigNode("VESSEL");
                node.AddValue("id", Id);
                node.AddValue("name", Name);
                node.AddValue("launchTime", LaunchTime);
                return node;
            }
            set
            {
                Id = value.GetString("id");
                Name = value.GetString("name");
                LaunchTime = value.GetDouble("launchTime", Planetarium.GetUniversalTime());
                if (string.IsNullOrEmpty(Id))
                {
                    Core.Log("Incorrect vessel id in node: " + value, LogLevel.Error);
                    return;
                }
            }
        }

        public VesselRecord(ConfigNode node) => ConfigNode = node;

        public VesselRecord(Vessel vessel) => Vessel = vessel;

        public VesselRecord(ProtoVessel protoVessel)
        {
            Guid = protoVessel.vesselID;
            Name = protoVessel.vesselName;
            LaunchTime = protoVessel.launchTime;
        }
    }
}
