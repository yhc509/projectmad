using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class DisplayNoiseNode : Node
{
    Texture2D texture;

    public DisplayNoiseNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
          : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 240;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.noiseDisplayNode;
        windowTitle = "Display Node";
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        return null;
    }

    public override void Draw()
    {
        inPoint.Draw();

        if (inPoint.ConnectedTo.Count == 1)
        {
            if (GUI.Button(new Rect(20, (windowRect.width - 40) + 30 + 10, windowRect.width - 40, 20), "Update") || !texture)
            {
                ChangeTextureDisplay(inPoint.ConnectedTo[0].Calculate(GenerationMode.texture));
            }

            if (texture)
            {
                //width, height : 200-40 = 160
                windowRect = new Rect(windowRect.x, windowRect.y, baseWidth, baseHeight);
                EditorGUI.DrawPreviewTexture(new Rect(20, 30, windowRect.width - 40, windowRect.width - 40), texture);
            }
        }
        else
        {
            //no texture smaller display
            windowRect = new Rect(windowRect.x, windowRect.y, baseWidth, baseHeight / 4f);
            EditorGUI.LabelField(new Rect(20, 20, windowRect.width - 40, 20), "ONE entry node required");
            texture = null;
        }

        
        
    }

    private void ChangeTextureDisplay(float[,] heightMap)
    {
        if(heightMap == null)
        {
            texture = null;
            return;
        }

        
        texture = new Texture2D(NodeEditor.TextureDisplaySize, NodeEditor.TextureDisplaySize);
        Color[] colorMap = new Color[NodeEditor.TextureDisplaySize * NodeEditor.TextureDisplaySize];

        for (int y = 0; y < NodeEditor.TextureDisplaySize; ++y)
        {
            for (int x = 0; x < NodeEditor.TextureDisplaySize; ++x)
            {
                colorMap[x * NodeEditor.TextureDisplaySize + y] = Color.Lerp(Color.black, Color.white, Mathf.Clamp01(heightMap[x, y] * 2f));
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();
    }
}
