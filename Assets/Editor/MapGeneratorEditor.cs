using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; //needed to caccess the editor

//Map Generator Editor: created in order to generate a button to generate our scene without
//needed to access game mode every time,
//so we can edit the scene from the Unity Editor

[CustomEditor(typeof(MapGenerator))] //Custom editor so it can be display in the inspector
public class MapGeneratorEditor : Editor    //We inherit from Editor here instead of the MonoBehaviour class
{
    public override void OnInspectorGUI()   //We override the Inspector (custom)
    {
        MapGenerator mapGen = (MapGenerator)target; //cast target to map generation
        //DrawDefaultInspector();

        if (DrawDefaultInspector()) //if any value change in the inspector
        {
            if (mapGen.autoUpdate) // if we auto update
            {
                mapGen.DrawMapInEditor();   //we generate the map
            }
        }

        //Create the button
        if (GUILayout.Button("Generate"))
        {
            //if pressed then generate the map
            mapGen.DrawMapInEditor();
        }
    }
}
