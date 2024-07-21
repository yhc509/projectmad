using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public enum NodeType
{
    simplePerlinNoise, falloffCurveNode, 
    noiseDisplayNode, sumNode, perlinNoiseNode, ridgedNoiseNode, subtractNode,
    multiplicationNode, divisionNode, constantNode, terrainGeneratorNode, customTransformNode, 
    gaussianBlurNode, customSourceNode, convolutionNode, diamondSquareNode
}

public enum GenerationMode
{
    texture,
    fullMap
}

public class NodeEditor : EditorWindow
{
    private static List<Node> nodes = new List<Node>();
    private static List<Connection> connections = new List<Connection>();

    public static GUIStyle ConnectionPointStyle;

    private static ConnectionPoint selectedInPoint;
    private static ConnectionPoint selectedOutPoint;

    //we have it downscaled by dim
    public static int TextureDisplaySize = 257; // must be a power of two plus one e.g. 65, 257, ...
    public static float TextureMappingMultiplier
    {
        get
        {
            return (TerrainOptiones.LandscaleSize / (float)TextureDisplaySize) * TerrainOptiones.TerrainDimension;
        }
    }

    public static float FullMapMultiplier = (TerrainOptiones.LandscaleSize / (float)TerrainOptiones.HeightMapSize);
    public static int FullMapSize = TerrainOptiones.HeightMapSize * TerrainOptiones.TerrainDimension;

    [MenuItem("Terrain Generator/Height Map Node-Editor")]
    private static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Terrain Editor");

        bool drawGenerateTerrain = true;
        foreach(Node node in nodes)
        {
            if(node is GenerateTerrainNode)
            {
                drawGenerateTerrain = false;
            }
        }
        if(drawGenerateTerrain)
            nodes.Add(new GenerateTerrainNode(new Vector2(40, 40), OnClickInPoint, OnClickOutPoint));
    }

    private void OnEnable()
    {
        ConnectionPointStyle = new GUIStyle();
        ConnectionPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        ConnectionPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        ConnectionPointStyle.border = new RectOffset(4, 4, 12, 12);
    }

    [Obsolete]
    private void OnGUI()
    {
        bool drawGenerateTerrain = true;
        foreach (Node node in nodes)
        {
            if (node is GenerateTerrainNode)
            {
                drawGenerateTerrain = false;
            }
        }
        if (drawGenerateTerrain)
            nodes.Add(new GenerateTerrainNode(new Vector2(40, 40), OnClickInPoint, OnClickOutPoint));

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawConnectionLine(Event.current);

        ProcessEvents(Event.current);

        DrawWindows();
        
        

        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector3 newOffset = new Vector3(gridSpacing, gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    [Obsolete]
    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    foreach(Node node in nodes)
                    {
                        if(node.windowRect.Contains(e.mousePosition)){
                            return;
                        }
                    }

                    //we clicked outside of a rect, so delete it
                    ClearConnectionSelection();
                }

                if (e.button == 1)
                {
                    ClearConnectionSelection();
                    ProcessRightClick(e);
                }
                break;
            case EventType.ScrollWheel:
                /*
                if(e.button == 0)
                {
                    foreach (Node node in nodes)
                    {
                        if (node.windowRect.Contains(e.mousePosition))
                        {
                            return;
                        }
                    }
                    //TODO temporary solution, not really the best but good enaugh for now
                    OnDrag(e.delta);
                    
                }
                */
                OnDrag(e.delta);
                break;
        }
    }
    /*
     * Check the second script posted by unimechanic, you can do it using GUI.BeginGroup()
        If you don't want to use the buttons as in that example you can check the events for the middle mouse 
        and start adding any changes on the mouse position to the PanY/PanX values. Haven't done it but it should be fairly simply to do
 
     * */
    private void OnDrag(Vector2 delta)
    {
        if(nodes != null)
        {
            foreach(var node in nodes)
            {
                node.windowRect = new Rect(node.windowRect.x + delta.x, node.windowRect.y + delta.y, node.windowRect.width, node.windowRect.height);
            }
        }
    }

    private void ProcessRightClick(Event e)
    {
        foreach(Node node in nodes)
        {
            if (node.windowRect.Contains(e.mousePosition))
            {
                ModifyNodeContextMenu(node);
                return;
            }
        }

        AddNewNodeContextMenu(e.mousePosition);
    }

    private void AddNewNodeContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();

        bool terrainGeneratorAlreadyPresent = false;
        foreach(var node in nodes)
        {
            if(node is GenerateTerrainNode)
            {
                terrainGeneratorAlreadyPresent = true;
            }
        }
        if (!terrainGeneratorAlreadyPresent)
        {
            genericMenu.AddItem(new GUIContent("Terrain Generator Node"), false, () => AddNewNode(NodeType.terrainGeneratorNode, mousePosition));
        }
        
        genericMenu.AddItem(new GUIContent("Display Node"), false, () => AddNewNode(NodeType.noiseDisplayNode, mousePosition));

        genericMenu.AddItem(new GUIContent("Source Nodes/Constant Node"), false, () => AddNewNode(NodeType.constantNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Simple Perlin Noise Node"), false, () => AddNewNode(NodeType.simplePerlinNoise, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Perlin Noise Node"), false, () => AddNewNode(NodeType.perlinNoiseNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Ridged Noise Node"), false, () => AddNewNode(NodeType.ridgedNoiseNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Diamond Square Node"), false, () => AddNewNode(NodeType.diamondSquareNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Curved Function Node"), false, () => AddNewNode(NodeType.falloffCurveNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Source Nodes/Custom Source Node"), false, () => AddNewNode(NodeType.customSourceNode, mousePosition));

        genericMenu.AddItem(new GUIContent("Transformation Nodes/Sum Node"), false, () => AddNewNode(NodeType.sumNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Subtract Node"), false, () => AddNewNode(NodeType.subtractNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Multiplication Node"), false, () => AddNewNode(NodeType.multiplicationNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Division Node"), false, () => AddNewNode(NodeType.divisionNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Gaussian Blur"), false, () => AddNewNode(NodeType.gaussianBlurNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Convolution Node"), false, () => AddNewNode(NodeType.convolutionNode, mousePosition));
        genericMenu.AddItem(new GUIContent("Transformation Nodes/Custom Node"), false, () => AddNewNode(NodeType.customTransformNode, mousePosition));

        genericMenu.ShowAsContext();
    }

    private void ModifyNodeContextMenu(Node node)
    {
        if(! (node is GenerateTerrainNode))
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, () => RemoveNode(node));
            genericMenu.ShowAsContext();
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.connectionOrigin.center,
                e.mousePosition,
                selectedInPoint.connectionOrigin.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.connectionOrigin.center,
                e.mousePosition,
                selectedOutPoint.connectionOrigin.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private static void AddNewNode(NodeType type, Vector2 position)
    {
        switch (type)
        {
            case NodeType.simplePerlinNoise:
                nodes.Add(new NoiseNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.falloffCurveNode:
                nodes.Add(new FalloffCurveNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.noiseDisplayNode:
                nodes.Add(new DisplayNoiseNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.perlinNoiseNode:
                nodes.Add(new PerlinNoiseNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.ridgedNoiseNode:
                nodes.Add(new RidgedNoiseNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.sumNode:
                nodes.Add(new SumNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.subtractNode:
                nodes.Add(new SubtractNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.divisionNode:
                nodes.Add(new DivisionNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.multiplicationNode:
                nodes.Add(new MultiplicationNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.constantNode:
                nodes.Add(new ConstantNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.terrainGeneratorNode:
                nodes.Add(new GenerateTerrainNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.gaussianBlurNode:
                nodes.Add(new GaussianBlurNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.customTransformNode:
                nodes.Add(new CustomTransformNode(position, OnClickInPoint, OnClickOutPoint));
                break;

            case NodeType.customSourceNode:
                nodes.Add(new CustomSourceNode(position, OnClickInPoint, OnClickOutPoint));
                break;
            case NodeType.convolutionNode:
                nodes.Add(new ConvolutionNode(position, OnClickInPoint, OnClickOutPoint));
                break;
            case NodeType.diamondSquareNode:
                nodes.Add(new DiamondSquareNode(position, OnClickInPoint, OnClickOutPoint));
                break;
        }
    }

    private string GetWindowNameWithIdx(string windowName)
    {
        int counter = 1;
        foreach(var node in nodes)
        {
            if (node.windowTitle.Contains(windowName))
                counter++;
        }

        return windowName + " [" + counter.ToString() + "]";
    }

    private static void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private static void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private static void RemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connectionsToRemove[i].RemoveReferences();
                connections.Remove(connectionsToRemove[i]);
            }

        }

        nodes.Remove(node);
    }

    private static void OnClickRemoveConnection(Connection connection)
    {
        connection.RemoveReferences();
        connections.Remove(connection);
    }

    private static void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }
        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

    private static void CreateConnection(ConnectionPoint inPoint, ConnectionPoint outPoint)
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }
        connections.Add(new Connection(inPoint, outPoint, OnClickRemoveConnection));
    }

    private static void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }


    private void DrawWindows()
    {
        BeginWindows();

        for (int i = 0; i < nodes.Count; ++i)
        {
            nodes[i].windowRect = GUI.Window(i, nodes[i].windowRect, DrawNodeWindow, nodes[i].windowTitle);
        }

        EndWindows();
    }

    private void DrawNodeWindow(int id)
    {
        nodes[id].Draw();
        GUI.DragWindow();
    }

    [Obsolete]
    public static void SaveNodeGraph(NodeEditorSaveState saveState)
    {
        foreach(var node in nodes)
        {
            saveState.savedNodes.Add(node.GetSaveNode());
        }
        saveState.SetDirty();
    }

    public static void LoadNodeGraph(NodeEditorSaveState savedNodeGraph)
    {
        //delete all existing nodes except the generator
        for(int i = nodes.Count-1; i >= 0; --i)
        {
            if (nodes[i].nodeType != NodeType.terrainGeneratorNode)
            {
                RemoveNode(nodes[i]);
            }
        }

        Dictionary<SaveNode, Node> createdNodes = new Dictionary<SaveNode, Node>();
        foreach(var savedNode in savedNodeGraph.savedNodes)
        {
            if (savedNode.nodeType != NodeType.terrainGeneratorNode)
            {
                AddNewNode(savedNode.nodeType, savedNode.position);
                Node createdNode = nodes[nodes.Count - 1];
                createdNodes.Add(savedNode, createdNode);
                
                //now check what kind of node we have, since we could now set up the preset values and also save them
                if(createdNode is NoiseNode)
                {
                    NoiseNode n = (NoiseNode)createdNode;
                    n.amplitude = savedNode.amplitude;
                    n.frequency = savedNode.frequency;
                    n.octaves = savedNode.octaves;
                }
                else if(createdNode is ConstantNode)
                {
                    ConstantNode c = (ConstantNode)createdNode;
                    c.constant = savedNode.constant;
                }
                else if(createdNode is FalloffCurveNode)
                {
                    FalloffCurveNode f = (FalloffCurveNode)createdNode;
                    f.animationCurve = new AnimationCurve(savedNode.animationCurve.keys);
                }
                else if(createdNode is CustomTransformNode)
                {
                    if(savedNode.customScriptName != "")
                    {
                        CustomTransformNode u = (CustomTransformNode)createdNode;
                        //TODO can we somehow assign the script from knowing the script class name?
                    }
                }

            }
        }

        //create connections after we set up all the nodes, since we need all of the to create connections
        foreach(var keyValue in createdNodes)
        {
            var savedNode = keyValue.Key;
            var node = keyValue.Value;

            if (savedNode.outPointNodes != null)
            {
                foreach (var connectedTo in savedNode.outPointNodes)
                {
                    //if we are connected to a generator node, special stuff

                    //find the correct outPoint
                    Node inNode = FindNodeByPositionAndType(connectedTo);
                    if (inNode == null)
                    {
                        Debug.LogError("We could not find saved NODE!!!");
                        return;
                    }

                    CreateConnection(inNode.inPoint, node.outPoint);
                }
            }
        }
    }

    private static Node FindNodeByPositionAndType(SavedOutPointNode savedOutPointNode)
    {
        var nodeType = savedOutPointNode.nodeType;
        var nodePos = savedOutPointNode.position;
        foreach(var node in nodes)
        {
            if(nodeType == NodeType.terrainGeneratorNode && node.nodeType == NodeType.terrainGeneratorNode)
            {
                return node;
            }
            else if(node.nodeType == nodeType && nodePos == node.GetIntPosition)
            {
                return node;
            }
        }

        return null;
    }
}