using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CreateAssetMenu]
public class TextureSettings : ScriptableObject
{
    #region SetUp
    private static string fileName = "TerrainTextureSettings.asset";
    private static string filePath => TerrainOptiones.BaseAssetFolder + "/" + fileName;

    public static TextureSettings Load()
    {
        TextureSettings textureSettings;

        if (!Directory.Exists(TerrainOptiones.BaseAssetFolder))
        {
            Directory.CreateDirectory(TerrainOptiones.BaseAssetFolder);
        }

        if (!File.Exists(TextureSettings.filePath))
        {
            textureSettings = CreateInstance<TextureSettings>();
            AssetDatabase.CreateAsset(textureSettings, filePath);
            Debug.Log("Creating new Texture Asset");
        }
        else
        {
            textureSettings = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextureSettings)) as TextureSettings;
        }

        return textureSettings;
    }
    #endregion

    private const string dirtTexture = "TerrainGenerationResources/Textures/Dirt";
    private const string forstTexture = "TerrainGenerationResources/Textures/Forst";
    private const string rocksHighTexture = "TerrainGenerationResources/Textures/Rocks_High";
    private const string rocksLowTexture = "TerrainGenerationResources/Textures/Rocks_Low";
    private const string snowTexture =  "TerrainGenerationResources/Textures/Snow";
    private const string waterTexture = "TerrainGenerationResources/Textures/Water";
    private const string waterMaterial = "TerrainGenerationResources/Materials/WaterMaterial";


    [HideInInspector]
    public float MaxTerrainHeight; //used for texturing

    public float SnowHeight;
    public Material WaterMaterial;
    public TextureLayer[] Layers;

    public int LayerCount => Layers?.Length ?? 0;

    public SerializedObject GetSerializedObject()
    {
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        return so;
    }

    [System.Serializable]
    public class TextureLayer
    {
        public Texture2D Texture;
        public Texture2D NormalMap;
        [Range(0, 1)]
        public float StartHeight;
        public int Overlap;
        public float TextureScale;
    }

    //default settings
    private void OnEnable()
    {
        if (File.Exists(filePath)) return;

        Debug.Log("Loading Default Texture Settings ...");

        SnowHeight = 400f;
        WaterMaterial = Resources.Load<Material>(waterMaterial);

        Layers = new TextureLayer[6];

        //water
        Layers[0] = new TextureLayer();
        Layers[0].StartHeight = 0f;
        Layers[0].Texture = Resources.Load<Texture2D>(waterTexture);
        Layers[0].NormalMap = Resources.Load<Texture2D>(waterTexture + "_NormalMap");
        Layers[0].Overlap = 2;
        Layers[0].TextureScale = 30;

        //Dirt
        Layers[1] = new TextureLayer();
        Layers[1].StartHeight = 0.0001f;
        Layers[1].Texture = Resources.Load<Texture2D>(dirtTexture);
        Layers[1].NormalMap = Resources.Load<Texture2D>(dirtTexture + "_NormalMap");
        Layers[1].Overlap = 25;
        Layers[1].TextureScale = 1;

        //forst
        Layers[2] = new TextureLayer();
        Layers[2].StartHeight = 0.15f;
        Layers[2].Texture = Resources.Load<Texture2D>(forstTexture);
        Layers[2].NormalMap = Resources.Load<Texture2D>(forstTexture + "_NormalMap");
        Layers[2].Overlap = 35;
        Layers[2].TextureScale = 5;

        //rock- low
        Layers[3] = new TextureLayer();
        Layers[3].StartHeight = 0.2f;
        Layers[3].Texture = Resources.Load<Texture2D>(rocksLowTexture);
        Layers[3].NormalMap = Resources.Load<Texture2D>(rocksLowTexture + "_NormalMap");
        Layers[3].Overlap = 35;
        Layers[3].TextureScale = 300;

        //rock- high
        Layers[4] = new TextureLayer();
        Layers[4].StartHeight = .6f;
        Layers[4].Texture = Resources.Load<Texture2D>(rocksHighTexture);
        Layers[4].NormalMap = Resources.Load<Texture2D>(rocksHighTexture + "_NormalMap");
        Layers[4].Overlap = 35;
        Layers[4].TextureScale = 300;

        //snow
        Layers[5] = new TextureLayer();
        Layers[5].StartHeight = 0.617f;
        Layers[5].Texture = Resources.Load<Texture2D>(snowTexture);
        Layers[5].NormalMap = Resources.Load<Texture2D>(snowTexture + "_NormalMap");
        Layers[5].Overlap = 20;
        Layers[5].TextureScale = 200;
    }
}
