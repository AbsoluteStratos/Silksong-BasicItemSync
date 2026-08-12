# BasicItemSync
Syncs items and other key completion between players in SSMP.
See the [commands section](#commands) for a list of all the things this mod syncs.

> This mod is very much in beta. Please report bugs here: https://github.com/BobbyTheCatfish/Silksong-BasicItemSync/issues

## IMPORTANT USAGE NOTE
BasicItemSync currently only supports completely synchronous playthroughs.
This means that **all players need to start from completely fresh save files**, and **new players should not join** midway through a playthrough.
Items that are obtained will only be sent to players currently connected to the server. **They will not be re-sent for new players.**

## Installation
BasicItemSync can be automatically installed by any Thunderstore compatible mod manager.

### Manual Installation
1) Install [SSMP](https://thunderstore.io/c/hollow-knight-silksong/p/SSMP/SSMP) and [AssetHelper](https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/AssetHelper/).
2) Download and extract BasicItemSync
3) Navigate to your BepInEx plugins folder
4) Copy the extracted mod folder to the plugins folder (Example: `plugins/BasicItemSync/BasicItemSync.dll`)


## Commands
```
/sync [module] [true/false]
```
Enables or disables syncing one of the following modules. All but `QuestItems` are on by default.
- Currency
	- Shell Shards
	- Rosaries
	- Chests
- Tools
	- Colored tools
	- Silk Skills
	- Crests
	- Vesticrest
- Abilities
	- Movement
	- Needolin
	- Needle Strike
- Battles
	- Boss Fights
	- Arenas
	- Void Masses
- Progression
	- Key Items (Keys, Melodies, etc.)
	- Bellshrines
	- Toll Benches
	- Important NPCs
- Collectables
	- Common Items
	- Relics
	- Fleas
	- Mementos
	- Bellhome Decor
- Upgrades
	- Masks
	- Silk Spools
	- Silk Hearts
	- Crafting Kits
	- Tool Pouches
	- Needle Upgrades
- Transit
	- Bellways
	- Ventricas
- Shortcuts
	- Levers
	- Pressure Plates
	- Breakable Walls
- Quests
	- Quest completion
	- Quest rewards
- QuestItems
- Maps
- Pins
	- Shakra Pins
	- Flea Pins