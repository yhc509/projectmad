using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
public class SumNode : Node
{
    public SumNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
         : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 60;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.sumNode;
        windowTitle = "Sum node";
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
        if(inPoint.ConnectedTo.Count != 2)
        {
            return null;
        }

        return SumArrays(inPoint.ConnectedTo[0].Calculate(mode), inPoint.ConnectedTo[1].Calculate(mode), mode);
    }

    private float[,] SumArrays(float[,] x, float[,] y, GenerationMode mode)
    {
        if(x == null || y == null)
        {
            return null;
        }
        int size = GetGenerationSize(mode);

        float[,] result = new float[size, size];
        
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                result[i, j] = Mathf.Clamp01(x[i, j] + y[i, j]);
            }
        }

        return result;
    }


}
