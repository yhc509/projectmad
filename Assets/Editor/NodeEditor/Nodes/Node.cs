using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Node
{
    public Rect windowRect;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public Action<Node> OnRemoveNode;

    public string windowTitle;

    protected int baseWidth = 200;
    protected int baseHeight = 150;

    public NodeType nodeType;

    public Node(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
    {
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, OnClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, OnClickOutPoint);
        windowTitle = "Title NOT SET";
    }

    public virtual void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();
    }

    protected static int GetGenerationSize(GenerationMode mode)
    {
        if(mode == GenerationMode.texture)
        {
            return NodeEditor.TextureDisplaySize;
        }
        else if(mode == GenerationMode.fullMap)
        {
            return NodeEditor.FullMapSize;
        }

        return -1; // should never occur
    }

    public abstract float[,] Calculate(GenerationMode mode);


    public Vector2Int GetIntPosition
    { 
        get { return new Vector2Int((int)Math.Floor(windowRect.x), (int)Math.Floor(windowRect.y)); } 
    }

    public virtual SaveNode GetSaveNode()
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
        
        return new SaveNode(nodeType, GetIntPosition, outPointNodes);
    }
}