using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

public class CustomSourceNode : Node
{
    public MonoScript selectedScript;

    public CustomSourceNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 100;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.customSourceNode;
        windowTitle = "Custom Source Node";
    }

    public override void Draw()
    {
        outPoint.Draw();

        selectedScript = EditorGUILayout.ObjectField(selectedScript, typeof(MonoScript), false) as MonoScript;

        if (selectedScript != null)
        {
            if (!selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomSourceNode)))
            {
                EditorGUI.LabelField(new Rect(20, 40, windowRect.width - 40, 40), "Script must implement\nICustonSourceNode");
            }
        }
        else
        {
            EditorGUI.LabelField(new Rect(20, 40, windowRect.width - 40, 40), "Select Script implementing \nICustonSourceNode");
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (selectedScript == null || !selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomSourceNode)))
        {
            Debug.LogError($"{windowTitle} Insert script implementing ICustomSourceNode");
            return null;
        }

        int size = GetGenerationSize(mode);

        var userClassType = selectedScript.GetClass();
        var userNodeCostructor = userClassType.GetConstructor(Type.EmptyTypes);
        object userNodeObject = userNodeCostructor.Invoke(new object[] { });

        var calculateMethod = userClassType.GetMethod("Calculate"); //if we implement the interface, we know we have this method available
        object result = calculateMethod.Invoke(userNodeObject, new object[] { size });
        float[,] userFunctionOutput = result as float[,];

        if (userFunctionOutput == null || userFunctionOutput.GetLength(0) != size || userFunctionOutput.GetLength(1) != size)
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

        if (selectedScript == null || !selectedScript.GetClass().GetInterfaces().Contains(typeof(ICustomSourceNode)))
        {
            return new SaveNode(nodeType, GetIntPosition, outPointNodes);
        }
        else
        {
            return new SaveNode(nodeType, GetIntPosition, outPointNodes, customScriptName: selectedScript.GetClass().Name);
        }

    }
}
