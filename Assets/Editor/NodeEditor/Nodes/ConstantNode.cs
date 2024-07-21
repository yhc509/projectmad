using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ConstantNode : Node
{
    public float constant = 0.5f;

    public ConstantNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 80;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.constantNode;
        windowTitle = "Constant";
    }

    public override void Draw()
    {
        outPoint.Draw();

        EditorGUI.LabelField(new Rect(15, 20, windowRect.width - 30, 20), "Select Constant");
        constant = EditorGUI.Slider(new Rect(15, 40, windowRect.width - 30, 20), constant, 0.01f, 1f);
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        int size = GetGenerationSize(mode);

        float[,] heightMap = new float[size, size];

        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                heightMap[z, x] = constant;
            }
        }

        return heightMap;
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

        return new SaveNode(nodeType, GetIntPosition, outPointNodes, constant:this.constant);
    }
}
