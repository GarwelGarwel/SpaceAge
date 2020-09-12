# SpaceAge

Space Age is a mod for Kerbal Space Program, which lets you see important events and achievements of your playthrough.

**Chronicle**

The Chronicle shows a history of notable events in the playthrough (adjustable via Difficulty Settings):
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

**Ship Log**

Ship Log is a part of Chronicle that shows all events that involved a particular vessel. It also displays some events that are not normally listed in the Chronicle, e.g. burns and takeoffs (hops). To open a ship log, simply click the Log button next to an event related to that vessel. You can switch between displaying normal Universal Time (UT) and the Mission Elapsed Time (MET) of the event. You can also search or export events similarly to the Chronicle. Click Back to return to normal Chronicle view.

**Achievements**

Achievements tab shows your playthrough's statistics, records, and firsts. Unlike the stock progress tracking, it allows you to see this data any time you want and you can even easily adjust it by changing or even adding new achievements.

Currently, the module tracks this data:
- total (lifetime) income
- number of launched and lost vessels and kerbals
- total mass of vessels launched
- number of unique vessels that reached space
- total numbers of planted flags and discovered anomalies
- heaviest vessel launched
- most expensive vessel launched
- most complex (by parts count) vessel launched
- max number of kerbals simultaneously assigned to missions
- max crew in a vessel (on launch)
- your first launch (crewed and total)
- the first time your vessel (or your crew) reached the space
- the first times you recovered or lost your vessels or crew
- number of destroyed KSC buildings

For every celestial body, it tracks:
- total numbers of landings (and, separately, crewed landings) and lost vessels
- total numbers of discovered anomalies (untested)
- the masses of the heaviest vessels that landed on or orbited the celestial body
- the first flybys, orbits, reentries, landings, and returns (crewed and overall) as well as flags planted on the body

Space Age can parse your save data to find and import records of previous discoveries made by the stock ProgressTracking system. This option can be enabled in the settings (default is off). This is handy if you've installed the mod mid-game or added some new achievements to it. However, the stock system saves much less information, so Space Age can only learn so much from it.

**Configuring your own achievements**

You can easily add, modify or remove achievements by editing `achievements.cfg` file in the mod's directory. Each `ACHIEVEMENT` record there corresponds to one type of an achievement. The following fields are used:
- `name` (obligatory): the internal unique name of the achievement
- `title`: how the achievement is displayed in the UI; the celestial body's name is added to it for body-specific achievements
- `type` (obligatory): set to `Total`, `Max` or `First` to define the achievement's behaviour to either add values, select the highest values or just mark the first time the achievement is completed
- `valueType` (only for `Total` and `Max` achievements): defines, which value to use for the achievement. Can be `Mass`, `PartsCount`, `CrewCount`, `Cost`, `TotalAssignedCrew` or `Funds` (only with `Income` or `Expense` events). Note that not every event is associated with a vessel, so sometimes you may not be able to access these values (e.g. you obviously can't get PartsCount for a StructureCollapsed event)..
- `onEvent` (at least one): event names that activate the achievemnt (see list below), can have multiple entries
- `bodySpecific`: set to `true` if the achievement should be tracked separately for each celestial body (default is false)
- `home`: can be `Only` (to count only events in SOI of home planet), `Exclude` (to ignore events in home SOI) or `Default`
- `crewedOnly`: set to `true` if the achievement is only activated when the vessel has crew
- `unique` (only for `Total` achievements): set to true if you only want to count each vessel (or kerbal in appropriate cases) once
- `stockSynonym`: id of the relevant achievement in the stock ProgressTracking system; only makes sense for `First` achievements
- `score`: How much base score this achievement awards; it is multiplied by celestial body's recovery science multiplier. For `Max` type achievements, it is also multiplied by the corresponding value
- `scoreName`: Name (also used as id) of a score category; only applies to achievements with `score` > 0

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

**Score**

This feature is tied to the Achievements system and it tracks your overall game progress by awarding you score for certain important achievements. Number of points depends on the achievement (e.g. landing is worth more than a flyby), celestial body (proportional to its science multiplier for recovery) and whether it was a crewed vessel or a probe. It should be compatible with all planet packs and works in all game modes.

Achievements in the following categories award score:

- First suborbital flight
- First reaching orbit
- First flyby of a celestial body
- First orbiting a celestial body
- First docking in a SOI of a celestial body (this category that doesn't distinguish between manned and unmanned vessels)
- First landing on a celestial body
- First return from orbit of a celestial body
- First return from the surface of a celestial body

Earned score is displayed in the Score tab for each category and each (unlocked) celestial body. If you have unlocked this achievement only with an unmanned vessel, it will be in yellow and with a `U` mark. If you have unlocked it with a manned vessel, the number will be preceded with a green `M` mark. The total score is shown in the bottom.

You can set the game to award you funds, science and/or reputation for gaining score. Just set the desired amounts per score point in the difficulty settings. These values can also be set by third-party mods using Module Manager (but still can be amended manually in-game).

To define your own score categories or change base score values, you may edit the Achievements.cfg file or use Module Manager. See chapter Achievements for details.

**Future Plans**

- More events, achievements and data to track (requests sought!)
- Tracking unique kerbals in more cases
- Average values tracking

**Supported Mods & Known Issues**

- [KSP-AVC](https://forum.kerbalspaceprogram.com/index.php?/topic/72169-12-ksp-avc-add-on-version-checker-plugin-1162-miniavc-ksp-avc-online-2016-10-13/)
- [Blizzy's Toolbar](https://forum.kerbalspaceprogram.com/index.php?/topic/55420-120-toolbar-1713-common-api-for-draggableresizable-buttons-toolbar/)

Space Age changes almost nothing in the universe, so it shouldn't affect playability of save games.

Tracking of returns from orbit/surface is far from perfect (it uses the stock system, which was intended for internal purposes) and can sometimes generate too many events. Delete unnecessary ones or disable tracking it in the Settings.

The mod has minor issues with Kerbal Construction Time, which handles technology discoveries and facility upgrades in its own way. You can simply disable or manually delete these entries.

**License**

The mod is distributed under MIT license. The icon has been created by [Delapouite](http://delapouite.com) and is licensed under CC-BY 3.0.
