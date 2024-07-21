using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NodeEditorSaveState : ScriptableObject
{
    public List<SaveNode> savedNodes;
    
    public void Init()
    {
        savedNodes = new List<SaveNode>();
    }
}

[System.Serializable]
public class SaveNode
{
    public NodeType nodeType;

    //we want to cast it to ints, so that we can COMPARE THE POSITIONS and set up connections properly
    public Vector2Int position; 

    public int octaves;
    public int frequency;
    public float amplitude;
    public float constant;
    public AnimationCurve animationCurve;
    public string customScriptName;
    
    public List<SavedOutPointNode> outPointNodes;

    public SaveNode(NodeType nodeType, Vector2Int position,
        List<SavedOutPointNode> outPointNodes = null,
        int octaves = 0, int frequency = 0, float amplitude = 0, float constant = 0, AnimationCurve animationCurve = null, string customScriptName = "")
    {
        this.nodeType = nodeType;
        this.position = position;
        this.octaves = octaves;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.constant = constant;
        this.outPointNodes = outPointNodes;
        this.customScriptName = customScriptName;
        if(animationCurve != null)
        {
            this.animationCurve = new AnimationCurve(animationCurve.keys);
        }
    }
}

[System.Serializable]
public class SavedOutPointNode
{
    public Vector2Int position;
    public NodeType nodeType;

    public SavedOutPointNode(Vector2Int position, NodeType nodeType)
    {
        this.position = position;
        this.nodeType = nodeType;
    }
}
