using System;
using UnityEditor;
using UnityEngine;

public class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;

        //set up the connection "knowledge" that they are connected
        outPoint.ConnectedTo.Add(inPoint.node);
        inPoint.ConnectedTo.Add(outPoint.node);
    }

    [Obsolete]
    public void Draw()
    {
        Handles.DrawBezier(
            inPoint.connectionOrigin.center,
            outPoint.connectionOrigin.center,
            inPoint.connectionOrigin.center + Vector2.left * 50f,
            outPoint.connectionOrigin.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((inPoint.connectionOrigin.center + outPoint.connectionOrigin.center) * 0.5f, Quaternion.identity, 4, 8, 
                Handles.SphereHandleCap))
        {
            OnClickRemoveConnection?.Invoke(this);
        }
    }

    public void RemoveReferences()
    {
        outPoint.ConnectedTo.Remove(inPoint.node);
        inPoint.ConnectedTo.Remove(outPoint.node);
    }
}