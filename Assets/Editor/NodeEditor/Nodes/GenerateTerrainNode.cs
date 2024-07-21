using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
public class GenerateTerrainNode : NoiseNode
{
    NodeEditorSaveState savedNodeGraph;
    private string folderPath => TerrainOptiones.BaseAssetFolder + "/SavedGraphs";
    int increasedSize = 240;

    public GenerateTerrainNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
          : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 140;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.terrainGeneratorNode;
        windowTitle = "Terrain Generator";
    }

    [Obsolete]
    public override void Draw()
    {
        inPoint.Draw();

        var currentY = 30f;
        var increaseY = 30f;

        if (inPoint.ConnectedTo.Count == 1)
        {
            windowRect = new Rect(windowRect.x, windowRect.y, baseWidth, increasedSize);

            EditorGUI.LabelField(new Rect(15, currentY, windowRect.width - 30, 20), "Terrain Size");
            currentY += increaseY;

            TerrainOptiones.LandscaleSize = EditorGUI.Slider(new Rect(15, currentY, windowRect.width - 30, 20), TerrainOptiones.LandscaleSize, 1024, 4096);
            currentY += increaseY;

            if (GUI.Button(new Rect(20, currentY, windowRect.width - 40, 20), "Generate Terrain"))
            {
                float[,] fullHeightMap = inPoint.ConnectedTo[0].Calculate(GenerationMode.fullMap);
                if(fullHeightMap == null)
                {
                    Debug.LogError("Some nodes are not connected correctly");
                    return;
                }

                TerrainOptiones.GenerateMap(fullHeightMap);
            }
            currentY += increaseY;
        }
        else
        {
            windowRect = new Rect(windowRect.x, windowRect.y, baseWidth, baseHeight);
        }

        if (GUI.Button(new Rect(20, currentY, windowRect.width - 40, 20), "Save Current Node Graph"))
        {
            if (savedNodeGraph == null)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string assetName = folderPath + "/newGraph.asset";
                int i = 1;
                while (File.Exists(assetName)){
                    assetName = folderPath + "/newGraph" + i++.ToString() + ".asset";
                }

                savedNodeGraph = ScriptableObject.CreateInstance<NodeEditorSaveState>();
                AssetDatabase.CreateAsset(savedNodeGraph, assetName);
            }
            savedNodeGraph.Init(); //reset the settings
            NodeEditor.SaveNodeGraph(savedNodeGraph);

        }

        currentY += increaseY;

        if (GUI.Button(new Rect(20, currentY, windowRect.width - 40, 20), "Load Selected Node Graph"))
        {
            if (savedNodeGraph != null)
            {
                NodeEditor.LoadNodeGraph(savedNodeGraph);
            }
        }

        currentY += increaseY;

        savedNodeGraph = (NodeEditorSaveState)EditorGUI.ObjectField(new Rect(20, currentY, windowRect.width - 40, 20),
        savedNodeGraph, typeof(NodeEditorSaveState), false);
    }
}
