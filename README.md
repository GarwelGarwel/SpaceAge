# SpaceAge

Space Age is a mod for Kerbal Space Program, which collects and shows various information about your campaign. As of v0.2, it has two modules, the Chronicle and Achievements, implemented (see below).

The mod is a beta. It shouldn't destroy your ships or kill your kerbals, but an occasional NRE or freezeup are possible. Backward compatibility between versions is not guaranteed. Please report errors in the Issues tab or in the forum post.

**Do NOT use the GameData folder on Github to install the mod as it contains WIP, often unfinished versions of files with an outdated DLL. Only use Releases tab to download the mod.**

**Chronicle**

The Chronicle module shows a history of notable events in the playthrough (adjustable in Difficulty Settings):
- launches
- reaching space
- orbiting
- reentries
- landings
- vessel recoveries
- returns from orbit and surface
- vessel destruction
- docking and undocking
- deaths
- flag plants
- anomaly discoveries
- KSC building upgrades
- KSC structures damaged
- tech nodes discoveries
- changes of SOI

You can find (filter) specific events and manually add your own events (like *"Construction of Space Station Alpha has begun."*). In addition, you can export the Chronicle into a file, located at *<your KSP install>/GameData/SpaceAge/PluginData/SpaceAge*.

**Achievements**

Achievements module shows your playthrough's statistics, records, and firsts. Unlike the stock progress tracking, it allows you to see this data any time you want and you can even easily adjust it by changing or even adding new achievements.

Currently, the module tracks this data:
- total (lifetime) income
- number of launched and lost vessels and kerbals
- total mass of vessels launched
- total numbers of planted flags and discovered anomalies
- heaviest vessel launched
- most complex (by parts count) vessel launched
- max crew in a vessel (on launch)
- your first launch (crewed and total)
- the first time your vessel (or your crew) reached the space
- the first times you recovered or lost your vessels or crew
- number of destroyed KSC buildings

For every celestial body, it tracks:
- total numbers of landings (and, separately, crewed landings)
- total numbers of discovered anomalies (untested)
- the masses of the heaviest vessels that landed on or orbited the celestial body
- the first flybys, orbits, reentries, landings, and returns (crewed and overall) as well as flags planted on the body

Space Age can parse your save data to find and import records of previous discoveries made by the stock ProgressTracking system. This option can be enabled in the settings (default is off). This is handy if you've installed the mod mid-game or added some new achievements to it. However, the stock system saves much less information, so Space Age can only learn so much from it.

**Configuring your own achievements**

You can easily add, modify or remove achievements by editing `achievements.cfg` file in the mod's directory. Each `ACHIEVEMENT` record there corresponds to one type of an achievement. The following fields are used:
- `name` (obligatory): the internal unique name of the achievement
- `title`: how the achievement is displayed in the UI; the celestial body's name is added to it for body-specific achievements
- `bodySpecific`: set to `true` if the achievement should be tracked separately for each celestial body (default is false)
- `type` (obligatory): set to `Total`, `Max` or `First` to define the achievement's behaviour to either add values, select the highest values or just mark the first time the achievement is completed
- `valueType` (only for `Total` and `Max` achievements): defines, which value to use for the achievement. Can be `Mass`, `PartsCount`, `CrewCount`, or `Funds` (only with `Income` or `Expense` events). Note that not every event is associated with a vessel, so sometimes you may not be able to access these values.
- `onEvent` (obligatory): which event activates the achievemnt (see list below)
- `crewedOnly`: set to `true` if the achievement is only activated when the vessel has crew
- `stockSynonym`: id of the relevant achievement in the stock ProgressTracking system; only makes sense for `First` achievements

The following events are available for `onEvent` field (for events in *italic* you can access mass, parts count, and crew count):
- *`Launch`*
- *`ReachSpace`*
- *`Orbit`*
- *`Reentry`*
- *`Landing`*
- *`SOIChange`*
- *`Recovery`*
- *`ReturnFromOrbit`*
- *`ReturnFromSurface`*
- *`Destroy`*
- *`Docking`*
- *`Undocking`*
- `Death`
- `FlagPlant`
- `AnomalyDiscovery`
- `FacilityUpgraded`
- `StructureCollapsed`
- `TechnologyResearched`
- `Income`
- `Expense`

**Future Plans**

- More events, achievements and data to track (requests sought!)
- Average values tracking (maybe)
- Trends: graphs of your progress (e.g. funds earned over time)

**Supported Mods**

- [KSP-AVC](https://forum.kerbalspaceprogram.com/index.php?/topic/72169-12-ksp-avc-add-on-version-checker-plugin-1162-miniavc-ksp-avc-online-2016-10-13/)
- [Blizzy's Toolbar](https://forum.kerbalspaceprogram.com/index.php?/topic/55420-120-toolbar-1713-common-api-for-draggableresizable-buttons-toolbar/)

Space Age doesn't change anything in the universe, so it shouldn't affect playability of save games.

Tracking of returns from orbit/surface is not perfect (it uses the stock system, which was intended for internal purposes) and can sometimes generate too many events. Delete unnecessary ones.

The mod has minor issues with Kerbal Construction Time, which handles technology discoveries and facility upgrades in its own way. You can simply disable or manually delete these entries.

**License**

The mod is distributed under MIT license. The icon has been created by [Delapouite](http://delapouite.com) and is licensed under CC-BY 3.0.
