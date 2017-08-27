using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    abstract class ChronicleEvent
    {
        double time;
        public double Time
        {
            get { return time; }
            set { time = value; }
        }

        abstract public string Name
        { get; }

        abstract public string Description
        { get; }

        abstract public ConfigNode ConfigNode
        { get; set; }

        public ChronicleEvent()
        { Time = Planetarium.GetUniversalTime(); }

        public ChronicleEvent(double time)
        { Time = time; }
    }
}
