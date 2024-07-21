using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class SubtractNode : Node
{
    public SubtractNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 140;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.subtractNode;
        windowTitle = "Subtraction Node";
    }

    public override void Draw()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperCenter;

        base.Draw();
        if (inPoint.ConnectedTo.Count != 2)
        {
            windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width, baseHeight / 2f);
            EditorGUI.LabelField(new Rect(20, 20, windowRect.width - 40, 20), "Two entry nodes are required");
        }
        else
        {
            windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width, baseHeight);

            if (GUI.Button(new Rect(20, 100, windowRect.width - 40, 20), "Switch Input"))
            {
                var tmp = inPoint.ConnectedTo[0];
                inPoint.ConnectedTo[0] = inPoint.ConnectedTo[1];
                inPoint.ConnectedTo[1] = tmp;
            }

            EditorGUI.LabelField(new Rect(20, 30, windowRect.width - 40, 20), inPoint.ConnectedTo[0].windowTitle, style);
            EditorGUI.LabelField(new Rect(20, 50, windowRect.width - 40, 20), "-", style);
            EditorGUI.LabelField(new Rect(20, 70, windowRect.width - 40, 20), inPoint.ConnectedTo[1].windowTitle, style);
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 2)
        {
            Debug.LogError("Node does NOT have EXACTLY 2 Connections -> " + inPoint.ConnectedTo.Count.ToString());
            return null;
        }

        return SubtractArrays(inPoint.ConnectedTo[0].Calculate(mode), inPoint.ConnectedTo[1].Calculate(mode), mode);
    }

    private float[,] SubtractArrays(float[,] x, float[,] y, GenerationMode mode)
    {
        if (x == null || y == null)
        {
            return null;
        }

        int size = GetGenerationSize(mode);

        float[,] result = new float[size, size];

        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                result[i, j] = Mathf.Clamp01(x[i, j] - y[i, j]);
            }
        }

        return result;
    }
}
