# SpaceAge

Space Age is a mod for Kerbal Space Program, which collects and shows various information about your campaign. As of v. 0.2, it has two modules, the Chronicle and Achievements, implemented (see below).

The mod is a beta. It shouldn't destroy your ships or kill your kerbals, but an occasional NRE or freezeup are possible. Backward compatibility between versions is not guaranteed. Please report errors in the Issues tab or in the forum post.

**Chronicle**

The Chronicle module shows a history of notable events in the playthrough (adjustable in Difficulty Settings):
- launches
- vessel recoveries
- vessel destruction
- deaths
- flag plants
- KSC building upgrades
- KSC structures damaged
- tech nodes discoveries
- changes of SOI

You can also add your own events (like *"Construction of Space Station Alpha has begun."*). In addition, you can export the Chronicle into a file, located at *<your KSP install>/GameData/SpaceAge/PluginData/SpaceAge*.

**Achievements**

Achievements module shows your playthrough's statistics, records, and firsts. Unlike the stock progress tracking, it allows you to see this data any time you want and you can even easily adjust it by changing or even adding new achievements.

Currently, the module tracks this data:
- total (lifetime) income
- number of launches (crewed and total)
- total mass of vessels launched
- total number of kerbals launched
- heaviest vessel launched
- most complex (by parts count) vessel launched
- max crew in a vessel (on launch)
- your first launch (crewed and total)
- the first time your vessel (or your crew) reached the space
- the first times you recovered or lost your vessels or crew

For every celestial body, it tracks:
- total number of landings (and, separately, crewed landings)
- the masses of the heaviest vessels that landed on or orbited the celestial body
- the first flybys, orbits, and landings on the body (crewed and overall)

**Configuring your own achievements**

You can easily add, modify or remove achievements by editing achievements.cfg file in the mod's directory. Each ACHIEVEMENT record there corresponds to one type of an achievement. The following fields are used:
- `name` (obligatory): the internal unique name of the achievement
- `title`: how the achievement is shown in the UI; the celestial body's name is added to it for body-specific achievements
- `bodySpecific`: set to `true` if the achievement should be tracked separately for each celestial body (default is false)
- `type` (obligatory): set to `Total`, `Max` or `First` to define the achievement's behaviour to either add values, select the highest values or just mark the first time the achievement is completed
- `valueType` (only for `Total` and `Max` achievements): defines, which value to use for the achievement. Can be `Mass`, `PartsCount`, `CrewCount`, or `Scalar` (currently only with `Income` or `Expense` events). Note that not every event is associated with a vessel, so sometimes you may not be able to access these values.
- `onEvent` (obligatory): which event activates the achievemnt (see list below)
- `crewedOnly`: set to `true` if the achievement is only activated when the vessel has crew

The following events are available for `onEvent` field:
- Launch
- Recovery
- Destroy
- Death
- FlagPlant
- FacilityUpgraded
- StructureCollapsed
- TechnologyResearched
- SOIChange
- Landed
- Flyby
- Orbit
- Income
- Expense

**Future Plans**

- Add parsing of the stock ProgressTracking system to load old achievements when Space Age is installed mid-game
- MOAR events, achievements and data to track
- Trends: will draw graphs of your progress (e.g. funds earned)

**Supported Mods**

- [KSP-AVC](https://forum.kerbalspaceprogram.com/index.php?/topic/72169-12-ksp-avc-add-on-version-checker-plugin-1162-miniavc-ksp-avc-online-2016-10-13/)
- [Blizzy's Toolbar](https://forum.kerbalspaceprogram.com/index.php?/topic/55420-120-toolbar-1713-common-api-for-draggableresizable-buttons-toolbar/)

Space Age shouldn't conflict with any mods as it doesn't change anything in the universe.

**License**

The mod is distributed under MIT license. The icon has been created by Delapouite (http://delapouite.com) and is licensed under CC-BY 3.0.
