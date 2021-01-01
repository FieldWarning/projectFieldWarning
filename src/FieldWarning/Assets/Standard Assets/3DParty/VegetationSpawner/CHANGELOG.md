1.0.3

Added:
- Callback delegate for when a grass/tree item respawns (see documentation for example).
- Terrain extension function for getting a tree instances of a specific prefab.
- Water level is now visualized in the scene view when the Settings tab is active.
- Option to disable automatic respawning of trees when a spawn rule is modified.

Changed:
- Renamed "Opacity threshold" for terrain layer masks to "Minimum strength", for clarity.
- Height range maximum value is now based on the height of the largest terrain (instead of being fixed at 2000).
- Collision detection now works for non-square terrains

Fixed:
- UI error when switching to a scene with fewer grass items than the last

1.0.2

Fixed:
- Trees appearing black when using Tree Creator shaders.

1.0.1

Added:
- Grass items can now be duplicated.
- Exposed seed value in settings tab (global, added to each item's own seed).

Fixed:
- Last selected tree item respawning when changing grass spawn rules.

1.0.0
Initial release