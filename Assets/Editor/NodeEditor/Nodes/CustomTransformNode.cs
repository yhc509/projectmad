using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class CustomTransformNode : Node
{
    public MonoScript selectedScript;

    public CustomTransformNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 100;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.customTransformNode;
        windowTitle = "Custom Transform Node";
    }

    public override void Draw()
    {
        base.Draw();

        selectedScript = EditorGUILayout.ObjectField(selectedScript, typeof(MonoScript), false) as MonoScript;

        if (inPoint.ConnectedTo.Count != 1)
        {
            EditorGUI.LabelField(new Rect(20, 40, windowRect.width - 40, 20), "One Entry Node Required");
        }
        else if (selectedScript != null)
        {
            if(! selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomTransformNode)))
            {
                EditorGUI.LabelField(new Rect(20, 40, windowRect.width - 40, 20), "Script must implement\nICustonTransformNode");
            }
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 1)
        {
            Debug.LogError($"{windowTitle} requires 1 Connection -> " + inPoint.ConnectedTo.Count.ToString());
            return null;
        }

        if (selectedScript == null || !selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomTransformNode)))
        {
            Debug.LogError($"{windowTitle} Insert script implementing ICustomTransformNode");
            return null;
        }

        int size = GetGenerationSize(mode);

        var userClassType = selectedScript.GetClass();
        var userNodeCostructor = userClassType.GetConstructor(Type.EmptyTypes);
        object userNodeObject = userNodeCostructor.Invoke(new object[] { });

        var calculateMethod = userClassType.GetMethod("Calculate"); //if we implement the interface, we know we have this method available
        object result = calculateMethod.Invoke(userNodeObject, new object[] { size, inPoint.ConnectedTo[0].Calculate(mode) });
        float[,] userFunctionOutput = result as float[,];

        if(userFunctionOutput == null || userFunctionOutput.GetLength(0) != size || userFunctionOutput.GetLength(1) != size)
        {
            Debug.LogError("Check if yout custom function returns the same sized array as the input array (size x size)");
            return null;
        }
        
        return userFunctionOutput;
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

        if(selectedScript == null || !selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomTransformNode)))
        {
            return new SaveNode(nodeType, GetIntPosition, outPointNodes);
        }
        else
        {
            return new SaveNode(nodeType, GetIntPosition, outPointNodes, customScriptName: selectedScript.GetClass().Name);
        }
        
    }
}
