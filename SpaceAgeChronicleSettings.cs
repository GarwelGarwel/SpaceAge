using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceAge
{
    class SpaceAgeChronicleSettings : GameParameters.CustomParameterNode
    {
        public override string Title
        { get { return "Chronicle Settings"; } }

        public override GameParameters.GameMode GameMode
        { get { return GameParameters.GameMode.ANY; } }

        public override string Section
        { get { return "Space Age"; } }

        public override string DisplaySection
        { get { return Section; } }

        public override int SectionOrder
        { get { return 1; } }

        public override bool HasPresets
        { get { return false; } }

        [GameParameters.CustomParameterUI("Use Blizzy's Toolbar", toolTip = "Show icon in Blizzy's Toolbar, if available, instead of stock AppLauncher")]
        public bool UseBlizzysToolbar = true;

        [GameParameters.CustomParameterUI("Show Notifications", toolTip = "Show on-screen notifications when new items added to the Chronicle")]
        public bool showNotifications = true;

        [GameParameters.CustomIntParameterUI("Items per Page", toolTip = "How many Chronicle entries to show in one page", minValue = 5, maxValue = 25, stepSize = 5)]
        public int linesPerPage = 10;

        [GameParameters.CustomParameterUI("Newest First", toolTip = "Show most recent events first in the Chronicle")]
        public bool newestFirst = true;

        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "Verbose logging, obligatory for bug submissions")]
        public bool debugMode = true;
    }
}
