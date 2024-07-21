using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class PerlinNoiseNode : NoiseNode
{
    private NoiseModule perlinNoiseModule;
    

    public PerlinNoiseNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 210;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);
        perlinNoiseModule = new PerlinNoise(1);

        nodeType = NodeType.perlinNoiseNode;
        windowTitle = "Perlin Noise Node";
        octaves = 4;
        frequency = 3000;
    }

    public override void Draw()
    {
        outPoint.Draw();

        EditorGUI.LabelField(new Rect(15, 20, windowRect.width - 30, 20), "Octaves");
        octaves = EditorGUI.IntSlider(new Rect(15, 40, windowRect.width - 30, 20), octaves, 1, 10);

        EditorGUI.LabelField(new Rect(15, 70, windowRect.width - 30, 20), "Frequency");
        frequency = EditorGUI.IntSlider(new Rect(15, 90, windowRect.width - 30, 20), frequency, 500, 5000);

        EditorGUI.LabelField(new Rect(15, 120, windowRect.width - 30, 20), "Amplitude");
        amplitude = EditorGUI.Slider(new Rect(15, 140, windowRect.width - 30, 20), amplitude, 0.01f, 1f);
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        float multiplier;
        if (mode == GenerationMode.fullMap)
        {
            multiplier = NodeEditor.FullMapMultiplier;
        }
        else
        {
            multiplier = NodeEditor.TextureMappingMultiplier;
        }

        int size = GetGenerationSize(mode);

        float[,] heightMap = new float[size, size];
        for (int z = 0; z < size; z++)
        {
            float worldPosZ = z * multiplier;
            for (int x = 0; x < size; x++)
            {
                float worldPosX = x * multiplier;
                heightMap[z, x] = perlinNoiseModule.FractalNoise2D(worldPosX, worldPosZ, octaves, frequency, amplitude);
            }
        }

        return heightMap;
    }


}
