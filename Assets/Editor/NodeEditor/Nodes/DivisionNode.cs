using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class DivisionNode : Node
{
    public DivisionNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 60;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.divisionNode;
        windowTitle = "Division Node";
    }

    public override void Draw()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperCenter;

        base.Draw();
        if (inPoint.ConnectedTo.Count != 2)
        {
            EditorGUI.LabelField(new Rect(20, 20, windowRect.width - 40, 20), "Two entry nodes are required");
        }
        else
        {
            if (GUI.Button(new Rect(20, 100, windowRect.width - 40, 20), "Switch Input"))
            {
                var tmp = inPoint.ConnectedTo[0];
                inPoint.ConnectedTo[0] = inPoint.ConnectedTo[1];
                inPoint.ConnectedTo[1] = tmp;
            }

            EditorGUI.LabelField(new Rect(20, 30, windowRect.width - 40, 20), inPoint.ConnectedTo[0].windowTitle, style);
            EditorGUI.LabelField(new Rect(20, 50, windowRect.width - 40, 20), "/", style);
            EditorGUI.LabelField(new Rect(20, 70, windowRect.width - 40, 20), inPoint.ConnectedTo[1].windowTitle, style);
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 2)
        {
            Debug.LogError("Node does NOT have EXACTLY 2 Connections, it has: " + inPoint.ConnectedTo.Count.ToString());
            return null;
        }

        return DivideArrays(inPoint.ConnectedTo[0].Calculate(mode), inPoint.ConnectedTo[1].Calculate(mode), mode);
    }

    private float[,] DivideArrays(float[,] x, float[,] y, GenerationMode mode)
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
                if( Mathf.Abs(y[i, j]) <=  0.01f)
                {
                    result[i, j] = x[i, j]; //so we dont divide by rediculus numbers
                }
                else
                {
                    result[i, j] = x[i, j] / y[i, j];
                }
                
            }
        }

        return result;
    }
}
