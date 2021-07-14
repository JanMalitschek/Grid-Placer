using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridPlacer{
    //This scriptable object holds a list of GameObjects. That's it.
    [CreateAssetMenu(fileName = "New Prefab Palette", menuName = "Grid Placer/Prefab Palette", order = 3)]
    public class PrefabPalette : ScriptableObject {
        public List<GameObject> palette = new List<GameObject>();
    }
}