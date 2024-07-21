using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(TerrainOptiones))]
public class EditorExtention : Editor
{
    public override void OnInspectorGUI()
    {
        //TerrainOptiones terrainOptiones = (TerrainOptiones)target;
        
        //DrawDefaultInspector();

        ////if (GUILayout.Button("Generate Map"))
        ////{

        ////    terrainOptiones.GenerateMap();
        ////}

        //if(GUILayout.Button("Retexture Map"))
        //{
        //    terrainOptiones.RetextureMap();
        //}

        //if(GUILayout.Button("Simulate Hydrolic-Erosion"))
        //{
        //    terrainOptiones.ApplyHydrolicErosion();
        //}

        //if(GUILayout.Button("Generate Water"))
        //{
        //    terrainOptiones.GenerateWater();
        //}
    }
}
