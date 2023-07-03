# Despicaville
### This is the source code for a serial-killer/life sim game being built using OP Game Engine in VS 2022. It's still in a very early alpha state (not all core features are implemented).
# 
###### Releases will only be available up through Beta state of the game prior to actual release on Steam, but code changes will stay updated on here and remain available for those who wish to compile their own modded version. After release of the game, the game will need to be purchased on Steam to acquire media files for the Content folder (e.g. music, sounds, and textures).
# 
### Features currently in the game:
- Numerous character stats for sim/survival mechanics (5 physical stats and 5 mental stats)
- Random generation of small town, 80s based time period
- Time progression based on player movement/action (timescales down to the millisecond for realistic timeframes)
- Tank-style movement using mouse for direction/turning and W/A/S/D for moving (keys can be bound in options)
- Default 'interact' key for convenience ('E' key by default), or right-click for menu-driven interacting
- Crouching to sneak, perform actions slower/quietly, and interact with furniture underneath sinks/phones
- Running to move more quickly and perform actions quickly/loudly
- Verbose event/dialogue logging with timestamps (uses in-game time)
- Intentionally very few animations or visible gore effects to leave the details to player imagination
- Combat/attacking with body-part targeting, hit chance calculations, and different actions taking different amounts of time to execute (e.g. stabs are quick while swinging a sledgehammer takes longer)
- Character 'health' driven by wounds (e.g. bruises, cuts, stabs, gunshots) per body part, overall percentage of blood remaining in body, and total pain from wounds (e.g. too much pain will cause a character to pass out until enough of the wounds contributing to their pain have recovered... not recovering quickly enough can result in laying there bleeding out to death)
- Blood trails/pooling from bleeding
- Somewhat realistic wound recovery timescales
- Consumable item usage (right-click to use)
- Drinking from sinks
- Very basic NPC AI to satisfy survival needs (eating and drinking)
  
### Planned features not yet implemented:
- Wound Management (menu to view wounds and apply items for recovery)
- Grappling/grabbing/dragging actions and mechanics based on stats
- More dynamic and personality-based reactions for NPCs being attacked
- NPCs under attack calling for help from other nearby NPCs and/or calling the police
- NPCs witnessing crimes calling the police and/or attempting citizens arrest
- Police investigation of crimes
- Graphic effect to display sound distances in-world
- Vehicles/driving
- NPC job/ai schedules
- Key-word based NPC dialogue
- Dialogue between NPCs that the player can overhear (and a means for NPCs to share/spread event knowledge)
- Implenting memory system for NPCs (tracking of in-game events per NPC)
- News on newspapers/TV covering in-game events from the prior day
- Purchasing items from stores and restocking of stores with items
- Daytime job for player (number of tasks that can be completed to earn money), handful of job options
- Usability of most items (e.g. using Duct Tape or Rope on NPCs)
- Inflicting cut wounds on feet when walking barefoot on broken glass
- Textures for more clothing items so they can be renderered on characters
- Status effects on weapons (e.g. knocking someone out with a hammer for a period of time)
- Display of status effects on player in the UI
- Tying more stats into more gameplay mechanics
- Skills for player progression (skills increase by doing whatever the skill governs)
- Learning/unlocking skills from books
- Saving/Loading game states
- More music
