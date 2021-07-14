using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GridPlacer{
    [CustomEditor(typeof(PrefabPalette))]
    public class PrefabPaletteEditor : Editor {
        //The PrefabPalette that's currently being edited
        private PrefabPalette palette;
        //The scroll position for the prefab palette
        private Vector2 paletteScrollPosition = Vector2.zero;

        private void OnEnable() {
            //Get the reference to the PrefabPalette
            palette = target as PrefabPalette;
        }

        public override void OnInspectorGUI() {
            //All of this is the exact same as the palette editor in the GridPlacer window
            //Please refer to that for more detailed comments
            Rect paletteRect = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(paletteRect, "Drag Prefabs here");
            Event current = Event.current;
            switch(current.type){
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if(!paletteRect.Contains(current.mousePosition))
                        break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if(current.type == EventType.DragPerform){
                        DragAndDrop.AcceptDrag();
                        foreach(UnityEngine.Object o in DragAndDrop.objectReferences)
                            if(o is GameObject && !palette.palette.Contains(o as GameObject))
                                palette.palette.Add(o as GameObject);
                        EditorUtility.SetDirty(palette);
                    }
                    break;
            }
            paletteScrollPosition = EditorGUILayout.BeginScrollView(paletteScrollPosition, GUILayout.ExpandHeight(true));
            foreach(GameObject g in palette.palette){
                if(g == null)
                    continue;
                GUILayout.BeginHorizontal();
                //Instead of having a separate "Remove" button a prefab is removed from the palette when clicking on it's thumbnail
                if(GUILayout.Button(AssetPreview.GetAssetPreview(g), GUILayout.Width(50.0f), GUILayout.Height(50.0f))){
                    palette.palette.Remove(g);
                    EditorUtility.SetDirty(palette);
                    break;
                }
                EditorGUILayout.LabelField(g.name, EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}