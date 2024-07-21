using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public Rect connectionOrigin;

    public ConnectionPointType type;

    public Node node;

    public GUIStyle style;

    public Action<ConnectionPoint> OnClickConnectionPoint;

    private const float pointHeight = 20f;
    private const float pointWidth = 10f;

    public List<Node> ConnectedTo = new List<Node>();
    
    public ConnectionPoint(Node node, ConnectionPointType type, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;

        this.style = NodeEditor.ConnectionPointStyle;

        this.OnClickConnectionPoint = OnClickConnectionPoint;
        connectionOrigin = new Rect();
    }

    public void Draw()
    {
        connectionOrigin.y = node.windowRect.y + (node.windowRect.height * 0.5f) - connectionOrigin.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                connectionOrigin.x = node.windowRect.x - connectionOrigin.width + 8f;

                if (GUI.Button(new Rect(0, node.windowRect.height / 2f - pointHeight/2f, pointWidth, pointHeight), "", style))
                {
                    OnClickConnectionPoint?.Invoke(this);
                }

                break;

            case ConnectionPointType.Out:
                connectionOrigin.x = node.windowRect.x + node.windowRect.width - 8f;
                
                if (GUI.Button(new Rect(node.windowRect.width - pointWidth, node.windowRect.height / 2f - pointHeight/2f, pointWidth, pointHeight), "", style))
                {
                    OnClickConnectionPoint?.Invoke(this);
                }

                break;
        }
        

        

    }
}