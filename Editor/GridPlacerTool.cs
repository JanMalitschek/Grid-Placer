using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace GridPlacer{
    //This is a completely blank tool that is only used to keep track of when the grid placer tool is selected
    //I don't think this is how EditorTools are supposed to be used but I don't care.
    [EditorTool("Grid Placer")]
    public class GridPlacerTool : EditorTool
    {
        GUIContent iconContent;

        private void OnEnable() {
            iconContent = new GUIContent(){
                //Load in this tools icon
                image = Resources.Load<Texture>("Icon_DarkMode"),
                text = "Grid Placer",
                tooltip = "Grid Place"
            };
        }

        public override GUIContent toolbarIcon => iconContent;
    }
}