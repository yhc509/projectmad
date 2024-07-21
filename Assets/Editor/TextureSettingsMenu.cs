using UnityEngine;
using UnityEditor;

public class TextureSettingsMenu : EditorWindow
{
    private const float width = 300;
    private const float height = 680;
    
    private int blurRadius = 1;

    public TextureSettings textureSettings;

    [MenuItem("Terrain Generator/Texture Settings")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(TextureSettingsMenu));
        window.minSize = new Vector2(width - 1, height - 1);
        window.maxSize = new Vector2(width, 1080);
    }

    [System.Obsolete]
    void OnGUI()
    {
        GUILayout.Label("");
        GUILayout.Label("Blur Radius");
        blurRadius = EditorGUILayout.IntSlider(blurRadius, 1, 30);
        GUILayout.Label("");
        blurRadius = Mathf.Clamp(blurRadius, 1, int.MaxValue);

        if (GUILayout.Button("Blur splatmap"))
        {
            TerrainOptiones.BlurrTextures(blurRadius);
        }

        if (textureSettings == null)
        {
            textureSettings = TextureSettings.Load();
        }

        GUILayout.Label("");
        GUILayout.Label("Textures");
        DisplayTextureLayers();

        if(GUILayout.Button("Texture Terrain"))
        {
            textureSettings.SetDirty();
            
            //update when texturing
            var vegetationSettings = VegetationSettings.Load();
            vegetationSettings.Update();

            TerrainOptiones.RetextureMap();
        }

        if (GUILayout.Button("Generate Water"))
        {
            TerrainOptiones.GenerateWater();
        }

    }    

    private void DisplayTextureLayers()
    {
        SerializedObject so = textureSettings.GetSerializedObject();
        SerializedProperty stringsProperty = so.FindProperty("Layers");
        EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
        so.ApplyModifiedProperties();
    }
}


