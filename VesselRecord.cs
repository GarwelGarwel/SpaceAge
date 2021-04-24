using System;

namespace SpaceAge
{
    public class VesselRecord : IConfigNode
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
        public long LaunchTime { get; set; }

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
                    LaunchTime = (long)value.launchTime;
                }
            }
        }

        public void Save(ConfigNode node)
        {
            if (Id == null)
                return;
            node.AddValue("id", Id);
            node.AddValue("name", Name);
            node.AddValue("launchTime", LaunchTime);
        }

        public void Load(ConfigNode node)
        {
            Id = node.GetString("id");
            Name = node.GetString("name");
            LaunchTime = node.GetLongOrDouble("launchTime", (long)Planetarium.GetUniversalTime());
            if (string.IsNullOrEmpty(Id))
                Core.Log($"Incorrect vessel id in node: {node}", LogLevel.Error);
        }

        public VesselRecord(ConfigNode node) => Load(node);

        public VesselRecord(Vessel vessel) => Vessel = vessel;

        public VesselRecord(Guid id) => Vessel = FlightGlobals.FindVessel(id);

        public VesselRecord(string id)
            : this(new Guid(id))
        { }

        public VesselRecord(ProtoVessel protoVessel)
        {
            Guid = protoVessel.vesselID;
            Name = protoVessel.vesselName;
            LaunchTime = (long)protoVessel.launchTime;
        }
    }
}
