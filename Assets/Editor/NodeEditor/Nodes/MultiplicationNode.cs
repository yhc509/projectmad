using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class MultiplicationNode : Node{
    public MultiplicationNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 60;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.multiplicationNode;
        windowTitle = "Multiplication Node";
    }

    public override void Draw()
    {
        base.Draw();
        if (inPoint.ConnectedTo.Count != 2)
        {
            EditorGUI.LabelField(new Rect(20, 20, windowRect.width - 40, 20), "Two entry nodes are required");
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 2)
        {
            Debug.LogError("Node does NOT have EXACTLY 2 Connections, it has: " + inPoint.ConnectedTo.Count.ToString());
            return null;
        }

        return MultiplyArrays(inPoint.ConnectedTo[0].Calculate(mode), inPoint.ConnectedTo[1].Calculate(mode), mode);
    }

    private float[,] MultiplyArrays(float[,] x, float[,] y, GenerationMode mode)
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
                result[i, j] = x[i, j] * y[i, j];
            }
        }

        return result;
    }
}
