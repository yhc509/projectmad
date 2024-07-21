using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VegetationSettingsMenu : EditorWindow
{
    private const float width = 300;
    private const float height = 780;
    private float treePlacementModificator = 0.6f;

    Vector2 scrollPosition = Vector2.zero;

    public VegetationSettings vegetationSettings;

    [MenuItem("Terrain Generator/Vegetation Settings")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(VegetationSettingsMenu));
        window.minSize = new Vector2(width - 1, height - 1);
        window.maxSize = new Vector2(width * 2, 1080);
    }

    [System.Obsolete]
    void OnGUI()
    {
        if (vegetationSettings == null)
        {
            vegetationSettings = VegetationSettings.Load();
        }

        
        GUILayout.Label("Tree Placment Modifier: " + treePlacementModificator);
        treePlacementModificator = GUILayout.HorizontalSlider(treePlacementModificator, 0.3f, 0.75f);
        GUILayout.Label("");
        DisplayTextureLayers();
        if (GUILayout.Button("Generate Vegetation"))
        {
            vegetationSettings.TreePlacingCoeffitient = treePlacementModificator;
            vegetationSettings.SetDirty();
            TerrainOptiones.PlaceTrees();
        }
    }

    private void DisplayTextureLayers()
    {
        SerializedObject so = vegetationSettings.GetSerializedObject();
        SerializedProperty stringsProperty = so.FindProperty("Layers");
        EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
        so.ApplyModifiedProperties();
    }

}
