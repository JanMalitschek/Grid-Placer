# Grid-Placer
![Grid Placer Logo](/Resources/Icon_LightMode.png)

A Unity3D package to alleviate the pain of creating grid-based maps by hand

# Installation
Open the Package Manager Window, click the little + button in the top left corner and choose Git URL.
Make sure to copy the URL from the green Code button instead of taking it from the browser URL field.

# The Grid Placer Window
In the Toolbar go to Tools > Grid Placer to open the Grid Placer Window. I suggest docking it next to the inspector.
Said window contains the following settings:
* **Grid**
  * **Origin** defines where the grid's origin lies. Can be used to offset the grid cells or change their height.
  * **Rotation** defines how the grid is rotated around the origin.
  * **Size X** defines the grid cell size on the x-axis with respect to the grid rotation.
  * **Size Z** defines the grid cell size on the z-axis with respect to the grid rotation.
* **Height Offset**
  * **Height** defines the currently selected prefab's height offset perpendicular to the grid.
  * **Small Step** defines the amount the height offset will change when scrolling while holding down Shift in the scene view.
  * **Big Step** defines the amount the grid height will change when scrolling in the scene view.
* **Rotation**
  * **Rotation** defines the currently selected prefab's rotation around it's local y-axis.
  * **Randomize** defines if the currently selected prefab's rotation around it's local y-axis should be randomized after each placement.
  * **Small Step** defines the amount the rotation will change when pressing E/Q while holding down Shift in the scene view.
  * **Big Step** defines the amount the rotation will change when pressing E/Q in the scene view.
* **Scale**
  * **Scale** defines the currently selected prefab's uniform scale.
  * **Randomize** defines if the currently selected prefab's uniform scale should be randomized after each placement.
    * **Min Scale** defines the minimum random uniform scale.
    * **Min Scale** defines the maximum random uniform scale.
* **Grid Snapping**
  * **Center** makes the currently selected prefab snap to the center of the active grid cell.
  * **Edges** makes the currently selected prefab snap to one of the four edges of the active grid cell.
  * **Corners** makes the currently selected prefab snap to one of the four corners of the active grid cell.
  * **None** disables snapping all together.
  * The Snap Settings are also displayed in the scene view.
* **Scene Sampling**
  * **Sample Height Offset** samples the current height offset relative to the grid.
  * **Sample Origin Transform** sets the grid origin equal to the sample point and aligns the grid rotation to the sampled normal.
  * **Sample Prefab** tries to use the sampled object as a prefab to instantiate. Only works on objects that are already an instance of a prefab (no unpacked prefabs).
* **Palette**
  * **Instance Parent** - Assign a scene Transform here to automatically parent all placed prefabs to it.
  * **Drag Prefabs here** to add them to the active prefab palette.
  * **Clear Palette** romves all prefabs from the active palette.
  * All prefabs in the active palette will be listet below
    * Click the **Prefab Thumbnail** to select it.
    * The **X** button removes this prefab from the palette.

# The Scene Gizmos
In the scene view you will notice the Grid Gizmo which is constantly following your cursor.
* The **white lines** display the grid's cell which your cursor is currently pointing at as well as indicate it's neighboring cells.
* The **green dot** displays your cursors position projected onto the grid. With Snapping disabled it's also the position the selected prefab will be placed at.
* The **orange dots** display the currently available snapping positions according to the active Snap Setting. The selected prefab will be placed at the orange dot which is closest to the green dot.
* The **orange lines** that appear between the grid lines and the snapping positions are a visual indicator of the height offset when using snapping.
* The **green line** that appears between the projected cursor position and the prefab are a visual indicator of the height offset when using no snapping.

# Shortcuts
Key (In the Scene View) | Action
----|-------
1 | Switch to **Center** Snap Mode
2 | Switch to **Edges** Snap Mode
3 | Switch to **Corners** Snap Mode
4 | Disable Snapping
Scroll | Change **Height** using **Big Step**
Scroll+Shift | Change **Height** using **Small Step**
E | Rotate the selected prefab clockwise using **Big Step**
E+Shift | Rotate the selected prefab clockwise using **Small Step**
Q | Rotate the selected prefab counterclockwise using **Big Step**
Q+Shift | Rotate the selected prefab counterclockwise using **Small Step**
LMB | Place the selected prefab
LMB+Alt | Sample the height of whatever you clicked on (Requires a collider to work)

# Prefab Palettes
![Prefab Palette Icon](/Icons/PrefabPalette.png)

Prefab Palettes are a custom asset type which can be used to group prefabs. They can then be dragged into the **Drag Prefabs here** field in the Grid Placer Window to add all prefabs contained in the Prefab Palette to the active palette.

To create a Prefab Palette right click in the Project Browser > Create > GridPlacer > Prefab Palette.
You will notice that the UI looks very similar to the Prefab Palette UI in the Grid Placer Window.
You can drag prefabs into the **Drag Prefabs here** field to add them to the Prefab Palette and click on the Prefab thumbnails in the list to remove them.
