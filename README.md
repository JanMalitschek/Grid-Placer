# Grid-Placer
![Grid Placer Logo](/Resources/Icon_LightMode.png)

A Unity3D package to alleviate the pain of creating grid-based maps by hand

# Installation
Open the Package Manager Window, click the little + button in the top left corner and choose Git URL.
Make sure to copy the URL from the green Code button instead of taking it from the browser URL field.

# The Grid Placer Window
* **Grid**
  * **Size X** defines the grid cell size on the x-axis.
  * **Size Z** defines the grid cell size on the z-axis.
* **Height**
  * **Height** defines the grid's height on the y-axis.
  * **Small Step** defines the amount the grid height will change when scrolling while holding down Shift in the scene view.
  * **Big Step** defines the amount the grid height will change when scrolling in the scene view.
* **Rotation**
  * **Rotation** defines the currently selected prefab's rotation on the y-axis.
  * **Small Step** defines the amount the rotation will change when pressing E/Q while holding down Shift in the scene view.
  * **Big Step** defines the amount the rotation will change when pressing E/Q in the scene view.
* **Grid Snapping**
  * **Center** makes the currently selected prefab snap to the center of the active grid cell.
  * **Edges** makes the currently selected prefab snap to one of the four edges of the active grid cell.
  * **Corners** makes the currently selected prefab snap to one of the four corners of the active grid cell.
  * **None** disables snapping all together.
  * The Snap Settings are also displayed in the scene view.
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
