using UnityEngine;
using UnityEditor;

public class ErosionSettings
{
    private const float width = 300;
    private const float height = 500;

    private int iterations = 50000;
    private int windSimulationCount = 20;
    private int dropletLifetime = 20;
    private float erosionRate = 0.01f;
    private float depositRate = 0.001f;
    private float maxErosionAmt = 0.01f;

    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(ErosionSettings));
        window.minSize = new Vector2(width -1, height-1);
        window.maxSize = new Vector2(width, height);
    }

    void OnGUI()
    {
        float currentItemHeight = 40f;
        float itemHeightAddAmt = 30f;

        GUILayout.Label("Hydrolic Erosion Settings");

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Simulated Droplet Count");
        currentItemHeight += itemHeightAddAmt;
        iterations = EditorGUI.IntSlider(new Rect(15, currentItemHeight, width - 30, 20), iterations, 25000, 250000);
        currentItemHeight += itemHeightAddAmt;

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Simulated Wind Erosion Count");
        currentItemHeight += itemHeightAddAmt;
        windSimulationCount = EditorGUI.IntSlider(new Rect(15, currentItemHeight, width - 30, 20), windSimulationCount, 0, 100);
        currentItemHeight += itemHeightAddAmt;

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Droplet Lifetime");
        currentItemHeight += itemHeightAddAmt;
        dropletLifetime = EditorGUI.IntSlider(new Rect(15, currentItemHeight, width - 30, 20), dropletLifetime, 1, 30);
        currentItemHeight += itemHeightAddAmt;

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Erosion Rate");
        currentItemHeight += itemHeightAddAmt;
        erosionRate = EditorGUI.Slider(new Rect(15, currentItemHeight, width - 30, 20), erosionRate, 0.001f, 0.1f);
        currentItemHeight += itemHeightAddAmt;

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Deposite Rate");
        currentItemHeight += itemHeightAddAmt;
        depositRate = EditorGUI.Slider(new Rect(15, currentItemHeight, width - 30, 20), depositRate, 0.0001f, 0.01f);
        currentItemHeight += itemHeightAddAmt;

        EditorGUI.LabelField(new Rect(15, currentItemHeight, width - 30, 20), "Maximal Erosion Amount");
        currentItemHeight += itemHeightAddAmt;
        maxErosionAmt = EditorGUI.Slider(new Rect(15, currentItemHeight, width - 30, 20), maxErosionAmt, 0.001f, 0.1f);
        currentItemHeight += itemHeightAddAmt;

        if (GUI.Button(new Rect(15, currentItemHeight, width - 30, 20), "Simulate Erosion"))
        {
            TerrainOptiones.ApplyHydrolicErosion(
                iterations, windSimulationCount, dropletLifetime,
                erosionRate, depositRate, maxErosionAmt);
        }

        if (HydrolicErosion.CanRevertSimulation)
        {
            currentItemHeight += itemHeightAddAmt;
            if (GUI.Button(new Rect(15, currentItemHeight, width - 30, 20), "Revert Simulation"))
            {
                HydrolicErosion.RevertSimulation();
            }
        }
        
    }
}
