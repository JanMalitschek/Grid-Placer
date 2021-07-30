using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

#if UNITY_EDITOR //This absolutely shouldn't be here! This is why all of this is inside the Editor folder but Unity doesn't care, I guess
namespace GridPlacer{
    public class GridPlacer : EditorWindow {
        [MenuItem("Tools/Grid Placer")]
        private static void ShowWindow() {
            var window = GetWindow<GridPlacer>();
            //Load the window Icon from resources depending on the editor skin
            window.titleContent = new GUIContent("Grid Placer", Resources.Load<Texture>(EditorGUIUtility.isProSkin ? "WindowIcon_DarkMode" : "WindowIcon_LightMode"));
            window.Show();
        }

        //Grid Settings
        public float gridSizeX = 5.0f, gridSizeZ = 5.0f;
        public float gridHeight = 0.0f;
        public float smallGridHeightStep = 1.0f;
        public float bigGridHeightStep = 5.0f;

        //Snapping Settings
        public int snapSetting = 0;
        //List of entries to use in the snapping mode selection grid
        private string[] snapSettingsNames = {
            "Center",
            "Edges",
            "Corners",
            "None"
        };

        //Rotation Settings
        public int currentRotation = 0;
        public int smallRotationStep = 10;
        public int bigRotationStep = 90;

        //Palette Settings
        public List<GameObject> prefabPalette = new List<GameObject>();
        //The prefab currently selected from the palette
        public GameObject selectedPrefab = null;
        //The selected prefabs instance used to preview its placement in the scene view
        private GameObject prefabScenePreview = null;
        public Transform instantiationParent = null;
        //Scroll position for the palette scroll view
        private Vector2 paletteScrollPosition = Vector2.zero;

        private void OnGUI() {
            //Set the global label width to 80px - otherwise field labels will be unnecessarily wide and will expand past the windows width
            EditorGUIUtility.labelWidth = 80;

            //Grid Settings
            GUILayout.Label("Grid", EditorStyles.boldLabel);
            //Indent the settings from the label
            EditorGUI.indentLevel++;
            gridSizeX = EditorGUILayout.FloatField("Size X", gridSizeX);
            gridSizeZ = EditorGUILayout.FloatField("Size Z", gridSizeZ);
            //Reset the indentation so that the next bold label will be drawn normally
            EditorGUI.indentLevel--;

            //Height Settings
            //Create some space from the previous settings group
            GUILayout.Space(10);
            GUILayout.Label("Height", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            gridHeight = EditorGUILayout.FloatField("Height", gridHeight);
            //BeginHorizontal so that the following settings will be drawn in the same line
            GUILayout.BeginHorizontal();
            smallGridHeightStep = EditorGUILayout.FloatField("Small Step", smallGridHeightStep);
            bigGridHeightStep = EditorGUILayout.FloatField("Big Step", bigGridHeightStep);
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            //Rotation Settings
            GUILayout.Space(10);
            GUILayout.Label("Rotation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            currentRotation = EditorGUILayout.IntSlider("Rotation", currentRotation, 0, 360);
            GUILayout.BeginHorizontal();
            smallRotationStep = EditorGUILayout.IntField("Small Step", smallRotationStep, GUILayout.ExpandWidth(true));
            bigRotationStep = EditorGUILayout.IntField("Big Step", bigRotationStep);
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            //Snap Settings
            GUILayout.Space(10);
            GUILayout.Label("Grid Snapping", EditorStyles.boldLabel);
            //Here we make use of the snapSettingsNames array
            snapSetting = GUILayout.SelectionGrid(snapSetting, snapSettingsNames, 4);

            //Palette Settings
            GUILayout.Space(10);
            GUILayout.Label("Palette", EditorStyles.boldLabel);
            //Increase the global label width a bit - otherwise "Instance Parent" will be cut off
            EditorGUIUtility.labelWidth = 100;
            //Object field of type Transform for the instantiation parent
            //This needs to have allowSceneObjects set to true so that we can use scene objects as the instantiation parent
            //Unity also complains if you don't do this^^
            instantiationParent = (Transform)EditorGUILayout.ObjectField("Instance Parent", instantiationParent, typeof(Transform), true);
            //Create the rectangle for the prefab drag area
            Rect paletteRect = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(paletteRect, "Drag Prefabs here");
            //Get a reference to the current event
            Event current = Event.current;
            switch(current.type){
                //We need to check for Drag and Drop events
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    //The mouse cursor must be inside the rectangle we defined before
                    if(!paletteRect.Contains(current.mousePosition))
                        break;
                    //Change the mouse cursor to give some visual prefab
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    //Check if the user let go of the mouse
                    if(current.type == EventType.DragPerform){
                        //This stops the drag/drops what ever was dragged
                        DragAndDrop.AcceptDrag();
                        //Loop through all objects that were dropped into the rectangle
                        foreach(UnityEngine.Object o in DragAndDrop.objectReferences)
                            //Add the object if it's a GameObject and it's not yet contained in the palette
                            if(o is GameObject && !prefabPalette.Contains(o as GameObject))
                                prefabPalette.Add(o as GameObject);
                            //If a prefab palette was dropped we iterate through all it's objects and add them to the palette
                            else if(o is PrefabPalette){
                                PrefabPalette palette = o as PrefabPalette;
                                foreach(GameObject p in palette.palette)
                                    //Again, make sure the gameobject is not yet part of the palette
                                    if(!prefabPalette.Contains(p as GameObject))
                                        prefabPalette.Add(p);
                            }
                    }
                    break;
            }
            //Button for clearing the entire palette
            if(GUILayout.Button("Clear Palette")){
                prefabPalette.Clear();
                //The selected prefab is gone so we should set the reference to null
                selectedPrefab = null;
                //And since no prefab is selected anymore we also need to remove the scene view preview
                //When destroying GameObjects from a custom editor or editor window we need to use DestroyImmediate() instead of just Destroy()!
                DestroyImmediate(prefabScenePreview);
            }
            //Begin the palette scroll view
            //This will only appear once there are too many prefabs to fit in the window
            paletteScrollPosition = EditorGUILayout.BeginScrollView(paletteScrollPosition, GUILayout.ExpandHeight(true));
            int prefabIndex = 0;
            //Iterate through all prefabs in the current palette
            foreach(GameObject g in prefabPalette){
                if(g == null)
                    continue;
                GUILayout.BeginHorizontal();
                //AssetPreview.GetAssetPreview() returns the image you would see in the project browser
                //So for prefabs we get a nice little 3D thumbnail
                if(GUILayout.Button(AssetPreview.GetAssetPreview(g), GUILayout.Width(50.0f), GUILayout.Height(50.0f))){
                    //When this button is clicked the corresponding prefab is selected
                    selectedPrefab = g;
                    //Destroy the current scene view preview object
                    if(prefabScenePreview)
                        DestroyImmediate(prefabScenePreview);
                    //Then create a new one based on the newly selected prefab
                    //Instead of using Instantiate() we use PrefabUtility.InstantiatePrefab()
                    //This maintains the link to the prefab, meaning it has that blue cube icon in the hierarchy and you can click the
                    //little arrow to edit it.
                    //Just using Instantiate() would destroy this link - equivalent to unpacking the prefab
                    prefabScenePreview = (GameObject)PrefabUtility.InstantiatePrefab(g);
                    //Override it's name - let's be honest, nobody likes the "(Clone)" postfix
                    prefabScenePreview.name = g.name;
                    //These hideFlags prevent the preview object from showing up in the hierarchy.
                    //It also won't be saved if for some reason it stays in the scene view and we lose it's reference
                    prefabScenePreview.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                    //Everytime we click this button we wan't to switch to the GridPlacerTool
                    //This makes the transform gizmos disappear
                    EditorTools.SetActiveTool<GridPlacerTool>();
                }
                //If this prefab is selected it's label should be drawn in green
                if(selectedPrefab == g)
                    GUI.color = Color.green;
                EditorGUILayout.LabelField(g.name, EditorStyles.boldLabel);
                GUI.color = Color.white;
                if(GUILayout.Button("X")){
                    //When the remove button is clicked we wan't to remove the corresponding prefab from the palette
                    if(selectedPrefab == g){
                        //If this prefab is also the selected prefab we need to remove it's reference and destroy the scene preview
                        selectedPrefab = null;
                        DestroyImmediate(prefabScenePreview);
                    }
                    prefabPalette.RemoveAt(prefabIndex);
                    break;
                }
                GUILayout.EndHorizontal();
                prefabIndex++;
            }
            EditorGUILayout.EndScrollView();

            RotatePrefab();
            SwitchSnappingMode();
        }

        private void OnEnable() {
            //Register a callback that notifies us when the active editor tool was changed
            EditorTools.activeToolChanged += ToolChanged;
        }

        void ToolChanged(){
            //When the active editor Tool is changed to something other than the Grid Placer i.e. the Transform tool
            //we need to destroy the current scene preview object
            //Again using DestroyImmediate() instead of Destroy()
            if(EditorTools.activeToolType != typeof(GridPlacerTool))
                DestroyImmediate(prefabScenePreview);
        }

        private void OnFocus() {
            //When we focus on, as in click on anything inside this window, we need to register the OnSceneGUI callback
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            //And switch to the Grid Placer Tool
            EditorTools.SetActiveTool<GridPlacerTool>();
        }

        private void OnDestroy() {
            //When this window is destroyed, as in closed, we should unregister all callbacks as to not cause any memory leaks
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorTools.activeToolChanged -= ToolChanged;
        }
        
        //In here happens everything scene view related like drawing Handles and scene view overlay GUI
        private void OnSceneGUI(SceneView sceneView) {
            //If the Grid Placer tool is not selected or the game is playing we want to skip this entire method
            if(EditorTools.activeToolType != typeof(GridPlacerTool) || Application.isPlaying)
                return;
            //This prevents the user from selecting things in the scene view
            //This is necessary because we don't want to immediately select objects we've just placed
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            //Get a reference to the current event
            Event current = Event.current;
            //Now we calculate a ray starting from the viewport camera and pointing along its forward vector
            Ray screenRay = Camera.current.ViewportPointToRay(new Vector3(current.mousePosition.x / Camera.current.pixelWidth,
                                                                1.0f - current.mousePosition.y / Camera.current.pixelHeight,
                                                                Camera.current.nearClipPlane));
            //Draw Crosshair
            //If the ray is exactly parallel to the XZ-plane we don't want to do any of the following calculations as it would result
            //in divisions by 0
            if(screenRay.direction.y != 0.0f){
                //Calculate the position where the camera ray would hit the XZ-Plane at gridHeight
                Vector3 hitPosition = screenRay.origin - ((screenRay.origin.y - gridHeight) / screenRay.direction.y) * screenRay.direction;
                Handles.color = new Color(0.5f, 1.0f, 0.0f, 0.5f);
                //Draw a little sphere at the hitPosition
                Handles.SphereHandleCap(0, hitPosition, Quaternion.identity, 0.5f, EventType.Repaint);
                //Calculate the origin point of the grid cell the current hitPosition is inside of
                //Simply do some rounding
                Vector3 gridPosition = new Vector3(Mathf.Floor(hitPosition.x / gridSizeX) * gridSizeX, gridHeight, Mathf.Floor(hitPosition.z / gridSizeZ) * gridSizeZ);
                //Actually draw the grid
                DrawGridAtPosition(gridPosition, gridSizeX, gridSizeZ);
                //Set the scene preview object's position to the snap position based on the current snap setting
                if(prefabScenePreview)
                    prefabScenePreview.transform.position = GetSnapPosition(hitPosition);
            }

            //Now draw the snap mode selection grid as a scene view overlay
            GUILayout.BeginArea(new Rect(5, 5, 50, 90));
            EditorGUI.BeginChangeCheck();
            snapSetting = GUILayout.SelectionGrid(snapSetting, snapSettingsNames, 1);
            //If this selection grid was used this window needs repainting otherwise the two selection grids will appear out of sync
            if(EditorGUI.EndChangeCheck())
                Repaint();
            GUILayout.EndArea();

            //Now we will catch some events
            //When the scroll wheel is moved we want to modify the current grid height
            if(current.type == EventType.ScrollWheel){
                //When no modifiers are active the grid height is offset using the bigGridHeightStep
                if(current.modifiers == EventModifiers.None)
                    //current.delta.y, which is the scroll wheel offset must be clamped between -1 and 1
                    //At least for me current.delta.y returned 3 and -3 for some reason...
                    gridHeight -= Mathf.Clamp(current.delta.y, -1.0f, 1.0f) * bigGridHeightStep;
                //When holding down the shift key the grid height is offset using the smallGridHeightStep
                else if(current.modifiers == (EventModifiers.Shift))
                    gridHeight -= Mathf.Clamp(current.delta.y, -1.0f, 1.0f) * smallGridHeightStep;
                //Use this event so no other listeners can catch it anymore
                current.Use();
                //Again this window needs repainting because we modified its settings from the scene view
                Repaint();
            }
            //Was the left(0) mouse button clicked?
            else if(current.type == EventType.MouseDown && current.button == 0){
                //When the alt key was held down we want to sample the scene height
                if(current.modifiers == EventModifiers.Alt){
                    SampleHeight(screenRay);
                    current.Use();
                }
                //Otherwise we will place our selected prefab
                else{
                    //Of course we cannot do that if we don't have anything selected
                    if(!selectedPrefab)
                        return;
                    //We use InstantiatePrefab() again to maintain the prefab link, as explained before
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, instantiationParent);
                    //We simply copy the scene preview object's properties as they are already exactly what we want
                    instance.name = prefabScenePreview.name;
                    instance.transform.position = prefabScenePreview.transform.position;
                    instance.transform.eulerAngles = prefabScenePreview.transform.eulerAngles;
                    //Register an object creation undo action so that we can use Ctrl/Cmd+Z to remove misplaced objects
                    Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
                    //Consume the event
                    current.Use();
                }
            }

            RotatePrefab();
            SwitchSnappingMode();

            //We need to repaint the scene view every frame else our grid handles won't update their positions
            SceneView.lastActiveSceneView.Repaint();
        }

        //A little helper function to wrap the current rotation
        //Can't use the modulo/% operator here because it doesn't work for negative numbers
        void WrapCurrentRotation(){
            while(currentRotation >= 360)
                currentRotation -= 360;
            while(currentRotation < 0)
                currentRotation += 360;
        }
        //This method handles the shortcuts to rotate the selected prefab
        void RotatePrefab(){
            Event current = Event.current;
            if(current.type == EventType.KeyDown){
                //When the E key is pressed we want to rotate the prefab to the right
                if(current.keyCode == KeyCode.E){
                    //Check for modifiers to decide wether to use the small- or the bigRotationStep
                    if(current.modifiers == EventModifiers.Shift)
                        currentRotation += smallRotationStep;
                    else
                        currentRotation += bigRotationStep;
                    //Then wrap the rotation using our little helper function
                    WrapCurrentRotation();
                    //Consume the event and repaint
                    current.Use();
                    Repaint();
                }
                //When the Q key is pressed we want to rotate the prefab to the left
                else if(current.keyCode == KeyCode.Q){
                    if(current.modifiers == EventModifiers.Shift)
                        currentRotation -= smallRotationStep;
                    else
                        currentRotation -= bigRotationStep;
                    WrapCurrentRotation();
                    current.Use();
                    Repaint();
                }
                //When the rotation settings have been updated we also need to update our scene preview object's rotation
                if(prefabScenePreview)
                    prefabScenePreview.transform.eulerAngles = new Vector3(0.0f, currentRotation, 0.0f);
            }
        }
        //This method handles the shortcuts for switching between snapping modes
        //I think this is pretty self explanatory at this point
        void SwitchSnappingMode(){
            Event current = Event.current;
            if(current.type == EventType.KeyDown){
                if(current.keyCode == KeyCode.Alpha1){
                    snapSetting = 0;
                    current.Use();
                    Repaint();
                }
                else if(current.keyCode == KeyCode.Alpha2){
                    snapSetting = 1;
                    current.Use();
                    Repaint();
                }
                else if(current.keyCode == KeyCode.Alpha3){
                    snapSetting = 2;
                    current.Use();
                    Repaint();
                }
                else if(current.keyCode == KeyCode.Alpha4){
                    snapSetting = 3;
                    current.Use();
                    Repaint();
                }
            }
        }

        //This function snaps the raw XZ-plane hit position according to the selected snap settings
        private Vector3 GetSnapPosition(Vector3 gridPosition){
            switch(snapSetting){
                case 0: //Center
                    //For the center position we need to use the same rounding trick to get the current cell origin position
                    //then we add the halved cell dimensions to that position
                    return new Vector3(Mathf.Floor(gridPosition.x / gridSizeX) * gridSizeX + gridSizeX * 0.5f,
                                    gridHeight,
                                    Mathf.Floor(gridPosition.z / gridSizeZ) * gridSizeZ + gridSizeZ * 0.5f);
                case 1: //Edge
                    //This could probably be optimized somehow but this works perfectly fine
                    //Once again we calculate the cell center
                    Vector3 cellCenter = new Vector3(Mathf.Floor(gridPosition.x / gridSizeX) * gridSizeX + gridSizeX * 0.5f,
                                        gridHeight,
                                        Mathf.Floor(gridPosition.z / gridSizeZ) * gridSizeZ + gridSizeZ * 0.5f);
                    //Using that center position we calculate all for edge positions
                    Vector3[] edgePositions = {
                        cellCenter + Vector3.right * gridSizeX * 0.5f,
                        cellCenter + Vector3.left * gridSizeX * 0.5f,
                        cellCenter + Vector3.forward * gridSizeZ * 0.5f,
                        cellCenter + Vector3.back * gridSizeZ * 0.5f
                    };
                    //Then we loop through all those positions and check which one is closest to the original hit position
                    int closestIndex = 0;
                    float closestDistance = Mathf.Infinity;
                    for(int i = 0; i < 4; i++){
                        //We can use SqrMagnitude here as we simply wan't a heuristic for distance and don't care about accuracy
                        //This way we can avoid the expensive square root
                        float distance = Vector3.SqrMagnitude(edgePositions[i] - gridPosition);
                        if(distance < closestDistance){
                            closestDistance = distance;
                            closestIndex = i;
                        }
                    }
                    //Now return the edge position which was closest
                    return edgePositions[closestIndex];
                case 2: //Corner
                    //To calculate the closest corner position we first need to offset the original hit position by half the cell size
                    gridPosition += new Vector3(gridSizeX * 0.5f, 0.0f, gridSizeZ * 0.5f);
                    //Then we use the same way of obtaining the cell center position again, except this time we end up with an offset corner position
                    gridPosition = new Vector3(Mathf.Floor(gridPosition.x / gridSizeX) * gridSizeX + gridSizeX * 0.5f,
                                    gridHeight,
                                    Mathf.Floor(gridPosition.z / gridSizeZ) * gridSizeZ + gridSizeZ * 0.5f);
                    //Reverse the half cell offset and we're done
                    gridPosition -= new Vector3(gridSizeX * 0.5f, 0.0f, gridSizeZ * 0.5f);
                    return gridPosition;
                case 3: //No Snapping
                    return gridPosition;
            }
            return gridPosition;
        }

        //This function draws the grid and the snapping positions in the scene view
        private void DrawGridAtPosition(Vector3 origin, float cellSizeX = 5.0f, float cellSizeZ = 5.0f){
            //First we draw the actual grid
            Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            //Don't really want to explain this... it's pretty straight forward to figure out
            Handles.DrawLine(new Vector3(-cellSizeX, 0.0f, 0.0f) + origin, new Vector3(cellSizeX * 2, 0.0f, 0.0f) + origin);
            Handles.DrawLine(new Vector3(-cellSizeX, 0.0f, cellSizeZ) + origin, new Vector3(cellSizeX * 2, 0.0f, cellSizeZ) + origin);
            Handles.DrawLine(new Vector3(0.0f, 0.0f, -cellSizeZ) + origin, new Vector3(0.0f, 0.0f, cellSizeZ * 2) + origin);
            Handles.DrawLine(new Vector3(cellSizeX, 0.0f, -cellSizeZ) + origin, new Vector3(cellSizeX, 0.0f, cellSizeZ * 2) + origin);
            Handles.color = new Color(1.0f, 0.5f, 0.0f, 0.5f);
            //Depending on which snap setting is selected little spheres will be drawn at the snap positions
            switch(snapSetting){
                case 0: //Center
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX * 0.5f, 0.0f, cellSizeZ * 0.5f), Quaternion.identity, 0.5f, EventType.Repaint);
                break;
                case 1: //Edges
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX * 0.5f, 0.0f, 0.0f), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(0.0f, 0.0f, cellSizeZ * 0.5f), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX * 0.5f, 0.0f, cellSizeZ), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX, 0.0f, cellSizeZ * 0.5f), Quaternion.identity, 0.5f, EventType.Repaint);
                break;
                case 2: //Corners
                    Handles.SphereHandleCap(0, origin + new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX, 0.0f, 0.0f), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(0.0f, 0.0f, cellSizeZ), Quaternion.identity, 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(0, origin + new Vector3(cellSizeX, 0.0f, cellSizeZ), Quaternion.identity, 0.5f, EventType.Repaint);
                break;
            }
        }

        //To sample the scene height we can simply use a Phyics.Raycast call
        //Ofcourse this only works if the object you want to sample has a collider attached to it
        private void SampleHeight(Ray cameraRay){
            if(Physics.Raycast(cameraRay, out RaycastHit hit, Mathf.Infinity, ~0))
                gridHeight = hit.point.y;
        }
    }
}
#endif