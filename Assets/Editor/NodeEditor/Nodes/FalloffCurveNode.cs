using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class FalloffCurveNode : Node
{
    public AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public FalloffCurveNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 60;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.falloffCurveNode;
        windowTitle = "Curved Function Node";
    }

    public override void Draw()
    {
        base.Draw();
        animationCurve = EditorGUI.CurveField(new Rect(15, 30, windowRect.width - 30, 20), "Falloff Map",animationCurve);
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        int size = GetGenerationSize(mode);
        float [,]falloffMap = new float[size, size];

        var center = size / 2f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var distance = Vector2.Distance(new Vector2(i, j), new Vector2(center, center)) / size / 0.5f;
                falloffMap[i, j] = animationCurve.Evaluate(Mathf.Clamp01(distance)) * Mathf.PerlinNoise(i, j);
            }
        }
        return falloffMap;
    }

    public override SaveNode GetSaveNode()
    {
        List<SavedOutPointNode> outPointNodes = null;

        if (outPoint.ConnectedTo != null && outPoint.ConnectedTo.Count > 0)
        {
            outPointNodes = new List<SavedOutPointNode>();
            foreach (var node in outPoint.ConnectedTo)
            {
                outPointNodes.Add(new SavedOutPointNode(node.GetIntPosition, node.nodeType));
            }
        }

        return new SaveNode(nodeType, GetIntPosition, outPointNodes, animationCurve:this.animationCurve);
    }

}
