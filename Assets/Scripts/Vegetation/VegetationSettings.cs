using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***
 * This class is used for saving the Users Vegetation Placment settings
 * Also pregenerates settings if needed
 * */
using System.IO;
using UnityEditor;

[CreateAssetMenu]
public class VegetationSettings : ScriptableObject
{
    #region SetUp
    private static string fileName = "VegetationSettings.asset";
    private static string filePath => TerrainOptiones.BaseAssetFolder + "/" + fileName;

    public static VegetationSettings Load()
    {
        VegetationSettings vegetationSettings;

        if (!Directory.Exists(TerrainOptiones.BaseAssetFolder))
        {
            Directory.CreateDirectory(TerrainOptiones.BaseAssetFolder);
        }

        if (!File.Exists(filePath))
        {
            vegetationSettings = CreateInstance<VegetationSettings>();
            AssetDatabase.CreateAsset(vegetationSettings, filePath);
            Debug.Log("Creating new Vegetation Settings Asset");
        }
        else
        {
            vegetationSettings = AssetDatabase.LoadAssetAtPath(filePath, typeof(VegetationSettings)) as VegetationSettings;
        }

        return vegetationSettings;
    }

    #endregion
    /// //TREES/////////
    //water
    private const string treeWater_0= "TerrainGenerationResources/VegetationPrefabs/Tree_Rock/Tree0";

    //sand
    private const string treeSand_0 = "TerrainGenerationResources/VegetationPrefabs/Tree_Beach/PalmTree";

    //forst
    private const string treeForst_0 = "TerrainGenerationResources/VegetationPrefabs/Tree_Forst/Tree_1";
    private const string treeForst_1 = "TerrainGenerationResources/VegetationPrefabs/Tree_Forst/Tree_2";
    private const string treeForst_2 = "TerrainGenerationResources/VegetationPrefabs/Tree_Forst/Tree_3";
    private const string treeForst_3 = "TerrainGenerationResources/VegetationPrefabs/Tree_Forst/Tree_4";

    //rocks
    private const string treeStone_1 = "TerrainGenerationResources/VegetationPrefabs/Tree_Rock/Tree1";
    private const string treeStone_2 = "TerrainGenerationResources/VegetationPrefabs/Tree_Rock/Tree2";
    private const string treeStone_3 = "TerrainGenerationResources/VegetationPrefabs/Tree_Rock/Tree3";
    private const string treeStone_4 = "TerrainGenerationResources/VegetationPrefabs/Tree_Rock/Tree4";

    //BUSHES + Details
    //water
    private const string detailWater_0 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_3/Rock_3";

    //sand
    private const string detailSand_0 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_1/Rock_1";
    private const string detailSand_1 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_2/Rock_2";

    //forst
    private const string detailForst_0 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_3/Rock_3";
    private const string detailForst_1 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_4/Rock_4";
    private const string bushForst_0 = "TerrainGenerationResources/VegetationPrefabs/Bushes/Bush_1";
    private const string bushForst_1 = "TerrainGenerationResources/VegetationPrefabs/Bushes/Bush_2";
    private const string bushForst_2 = "TerrainGenerationResources/VegetationPrefabs/Bushes/Bush_5";

    //rocks
    private const string detailRock_0 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_5/Rock_5";
    private const string detailRock_1 = "TerrainGenerationResources/VegetationPrefabs/Stones/Rock_6/Rock_6";
    private const string bushRock_0 = "TerrainGenerationResources/VegetationPrefabs/Bushes/Bush_3";
    private const string bushRock_1 = "TerrainGenerationResources/VegetationPrefabs/Bushes/Bush_2";



    private TextureSettings textureSettings;

    //public properties
    public float TreePlacingCoeffitient = 0.7f;
    public VegetationLayer[] Layers;
    
    public int LayerCount => Layers?.Length ?? 0;

    public void Update()
    {
        //gets called when we update texture settings, since the user can add/delete textures
        if(textureSettings == null)
        {
            textureSettings = TextureSettings.Load();
        }

        var newLayers = new VegetationLayer[textureSettings.LayerCount];

        for(int i = 0; i < textureSettings.LayerCount; ++i)
        {
            var texture = textureSettings.Layers[i].Texture;
            GetLayerSettings(texture, out int count, out GameObject prefab);
            newLayers[i] = new VegetationLayer(texture, count, prefab);
        }

        Layers = newLayers;
    }

    private void GetLayerSettings(Texture2D texture, out int count, out GameObject prefab)
    {
        count = 0;
        prefab = null;

        foreach(var layer in Layers)
        {
            if(layer.PlacementTexture == texture)
            {
                count = layer.TreeCount;
                return;
            }
        }
        
    }

    public void OnEnable()
    {
        if (File.Exists(filePath)) return;

        var textureSettings = TextureSettings.Load();

        int layerCount = textureSettings.LayerCount;
        Layers = new VegetationLayer[layerCount];

        //water
        Layers[0] = new VegetationLayer(textureSettings.Layers[0].Texture, 
            5 * TerrainOptiones.TerrainChunkCount,
            Resources.Load<GameObject>(treeWater_0),
            10 * TerrainOptiones.TerrainChunkCount,
            Resources.Load<GameObject>(detailWater_0)
        );

        //sand
        Layers[1] = new VegetationLayer(textureSettings.Layers[1].Texture,
            10 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(treeSand_0) },
            350 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(detailSand_0), Resources.Load<GameObject>(detailSand_1) }
        );

        //forst
        Layers[2] = new VegetationLayer(textureSettings.Layers[2].Texture,
            50 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(treeForst_0), Resources.Load<GameObject>(treeForst_1), Resources.Load<GameObject>(treeForst_2), Resources.Load<GameObject>(treeForst_3) },
            20 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(detailForst_0), Resources.Load<GameObject>(detailForst_1) },
            100 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(bushForst_0), Resources.Load<GameObject>(bushForst_1), Resources.Load<GameObject>(bushForst_2) } 
        );

        //rock low
        Layers[3] = new VegetationLayer(textureSettings.Layers[3].Texture,
            15 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(treeStone_1), Resources.Load<GameObject>(treeStone_2)},
            5 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(detailRock_0), Resources.Load<GameObject>(detailRock_1) },
            25 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(bushRock_1) }
        );

        //rock high
        Layers[4] = new VegetationLayer(textureSettings.Layers[4].Texture,
            5 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(treeStone_3), Resources.Load<GameObject>(treeStone_4) },
            5 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(detailRock_0), Resources.Load<GameObject>(detailRock_1) },
            25 * TerrainOptiones.TerrainChunkCount,
            new GameObject[] { Resources.Load<GameObject>(bushRock_0) }
        );

        //snow
        Layers[5] = new VegetationLayer(textureSettings.Layers[5].Texture);
    }

    public SerializedObject GetSerializedObject()
    {
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target); 
        return so;
    }


    [System.Serializable]
    public class VegetationLayer
    {
        [ReadOnly]
        public Texture2D PlacementTexture;
        public int TreeCount;
        public GameObject[] TreePrefab;
        public int DetailCount;
        public GameObject[] DetailPrefabs;
        public int BushCount;
        public GameObject[] BushPrefabs;

        public VegetationLayer(Texture2D texture, int treeCount = 0, GameObject treePrefab = null, int detailCount = 0, GameObject detail = null, int bushCount = 0, GameObject bush = null)
        {
            this.PlacementTexture = texture;
            this.TreeCount = treeCount;
            this.DetailCount = detailCount;
            this.BushCount = bushCount;

            if (treePrefab != null)
                this.TreePrefab = new GameObject[] { treePrefab };

            if (detail != null)
                this.DetailPrefabs = new GameObject[] { detail };

            if(bush != null)
                this.BushPrefabs = new GameObject[] { bush };

        }

        public VegetationLayer(Texture2D texture, int treeCount, GameObject[] treePrefabs, int detailCount, GameObject[] details, int bushCount = 0, GameObject[] bushes = null)
        {
            this.PlacementTexture = texture;
            this.TreeCount = treeCount;
            this.TreePrefab = treePrefabs;
            this.DetailPrefabs = details;
            this.DetailCount = detailCount;
            this.BushPrefabs = bushes;
            this.BushCount = bushCount;
        }
    }


    #region Custom Readonly Attribute
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endregion
}
