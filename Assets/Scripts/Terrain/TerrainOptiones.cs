using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TerrainOptiones
{
    public static string BaseAssetFolder = "Assets/TerrainGenerationAssets";
    private const string generationParent = "TerrainParent";

    private static TextureSettings settings;
    private static TextureSettings textureSettings
    {
        get 
        {
            if (settings == null)
                settings = TextureSettings.Load();

            return settings;
        }
    }

    private static Transform transform
    {
        get
        {
            var parent = GameObject.Find(generationParent);
            if (parent == null)
            {
                parent = new GameObject(generationParent);
            }
            return parent.transform;
        }
    }

    public static float LandscaleSize = 3072;
    public const int TerrainDimension = 3;
    public const int WaterTileSize = 220;
    public const float WaterTileScale = 2f;
    public const float WaterHeight = 1.5f;

    public static int WaterTileCount
    {
        get
        {
            var tileCount = (LandscaleSize / WaterTileScale) / WaterTileSize;
            return Mathf.CeilToInt(tileCount);
        }
    }

    public static int TerrainChunkCount => TerrainDimension * TerrainDimension;

    public const int HeightMapSize = 513;
    public const float TerrainHeight = 1500;
    public static float[,] TerrainHeights = new float[HeightMapSize, HeightMapSize];

    public static Terrain[,] TerrainGrid = new Terrain[TerrainDimension, TerrainDimension];
    //Textures (Splat)
    public const int alphaMapSize = 513;

    public static void GenerateMap(float[,] fullheightMap)
    {
        ResetGeneration();

        TerrainLayer[] splatPrototypes = new TerrainLayer[textureSettings.LayerCount];

        //create one splat for each texture and set it up
        for (int i = 0; i < textureSettings.LayerCount; i++)
        {
            splatPrototypes[i] = new TerrainLayer();
            splatPrototypes[i].diffuseTexture = textureSettings.Layers[i].Texture;
            if (textureSettings.Layers[i].NormalMap != null)
                splatPrototypes[i].normalMapTexture = textureSettings.Layers[i].NormalMap;
            splatPrototypes[i].tileOffset = Vector2.zero;
            splatPrototypes[i].tileSize = new Vector2(textureSettings.Layers[i].TextureScale, textureSettings.Layers[i].TextureScale);
        }

        for (int i = 0; i < TerrainDimension; i++)
        {
            for (int j = 0; j < TerrainDimension; j++)
            {
                //create a terrain Data object for each TerrainChunk
                TerrainData terrainData = new TerrainData();

                terrainData.heightmapResolution = HeightMapSize;
                terrainData.size = new Vector3(LandscaleSize, TerrainHeight, LandscaleSize);
                terrainData.alphamapResolution = alphaMapSize;
                terrainData.terrainLayers = splatPrototypes;

                //creates a terrain object from Terrain Data
                TerrainGrid[i, j] = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
                TerrainGrid[i, j].gameObject.name = $"Terrain [{i},{j}]";
            }
        }

        SetMaxHeight(fullheightMap); //for proper texturing

        for (int i = 0; i < TerrainDimension; i++)
        {
            for (int j = 0; j < TerrainDimension; j++)
            {
                TerrainGrid[i, j].transform.parent = transform; //set a new parent for the game object                
                TerrainGrid[i, j].transform.localPosition = new Vector3((i - (TerrainDimension - 1) / 2) * LandscaleSize, 0f, (j - (TerrainDimension - 1) / 2) * LandscaleSize);

                //terrainGrid[i, j].GetComponent<Collider>().enabled = true; 
                TerrainGrid[i, j].basemapDistance = 4000;
                TerrainGrid[i, j].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; //dont cast shadows from this object, perfrmance boost
                TerrainGenerationManager.CreateMap(TerrainGrid[i, j], TerrainGrid[i, j].transform.position, SplitHeightMap(i, j, fullheightMap));
            }
        }

        TerrainGenerationManager.Execute();
        ConnectTerrainChunks();
    }

    public static void ApplyHydrolicErosion(int iterations = 50000, int windSimulationCount = 10, int dropetLifetime = 20,
            float erosionRate = 0.01f, float depositRate = 0.001f, float maxErosionAmt = 0.01f)
    {
        var terrains = GetTerrainList();
        if (terrains == null)
            return;

        var settings = new HydrolicErosionSettings( 
            iterations, windSimulationCount, dropetLifetime,
            erosionRate, depositRate, maxErosionAmt);

        HydrolicErosion.SimulateHydrolicErosion(terrains, settings);
    }

    public static void BlurrTextures(int blurRadius)
    {
        var terrains = GetTerrainList();
        if (terrains == null)
            return;

        foreach(var terrain in terrains)
        {
            var terData = terrain.terrainData;
            float[,,] splatMapData = terData.GetAlphamaps(0, 0, terData.alphamapWidth, terData.alphamapHeight);
            BlurSplatMap(splatMapData, blurRadius);
            terData.SetAlphamaps(0, 0, splatMapData);
        }
    }

    private static void BlurSplatMap(float[,,] splats, int blurRadius)
    {
        for (int y = 0; y < splats.GetLength(0); y++)
        {
            for (int x = 0; x < splats.GetLength(1); x++)
            {

                //neighbours
                float[] c = new float[splats.GetLength(2)];
                float[] cr = new float[splats.GetLength(2)];
                float[] cl = new float[splats.GetLength(2)];
                float[] cu = new float[splats.GetLength(2)];
                float[] cd = new float[splats.GetLength(2)];

                for (int i = 0; i < c.Length; i++)
                {
                    c[i] = splats[y, x, i];
                }

                for (int i = 0; i < cr.Length; i++)
                {
                    cr[i] = splats[y, Mathf.Clamp(x + blurRadius, 0, splats.GetLength(1) - 1), i];
                }

                for (int i = 0; i < cl.Length; i++)
                {
                    cl[i] = splats[y, Mathf.Clamp(x - blurRadius, 0, splats.GetLength(1) - 1), i];
                }

                for (int i = 0; i < cu.Length; i++)
                {
                    cu[i] = splats[Mathf.Clamp(y - blurRadius, 0, splats.GetLength(0) - 1), x, i];
                }

                for (int i = 0; i < cd.Length; i++)
                {
                    cd[i] = splats[Mathf.Clamp(y + blurRadius, 0, splats.GetLength(0) - 1), x, i];
                }

                for (int i = 0; i < c.Length; i++)
                {
                    c[i] = (c[i] + cr[i] + cl[i] + cu[i] + cd[i]) / 5;
                }

                for (int i = 0; i < c.Length; i++)
                {
                    splats[y, x, i] = c[i];
                }

            }
        }
    }

    public static void GenerateWater()
    {
        var terrainList = GetTerrainList();
        if (terrainList == null)
            return;

        Vector3 offset = new Vector3(LandscaleSize, WaterHeight, LandscaleSize);
        foreach (var terrain in terrainList)
        {
            WaterGenerator.GenerateWater(terrain.transform, offset);
        }
    }

    public static void PlaceTrees()
    {
        var terrains = GetTerrainList();
        if (terrains == null)
            return;


        VegetationGenerator.PlaceTrees(terrains);
    }

    public static void RetextureMap()
    {
        Transform t = transform;
        if (t.childCount <= 0)
        {
            Debug.LogError("No Terrain has been generated, please use the node editor to generate a terrain first.");
            return;
        }

        var terrains = GetTerrainList();
        if (terrains == null)
            return;


        foreach(var terrain in terrains)
        {
            TerrainGenerationManager.RetextureTerrain(terrain);
        }

        TerrainGenerationManager.Execute();
    }

    private static float[,] SplitHeightMap(int i, int j, float[,] fullHeightMap)
    {
        float[,] splitHeightMap = new float[HeightMapSize, HeightMapSize];

        for (int x = 0; x < HeightMapSize; ++x)
        {
            int relativePosX = x + j * (HeightMapSize - 1);

            for (int z = 0; z < HeightMapSize; ++z)
            {
                int relativePosZ = z + i * (HeightMapSize - 1);
                splitHeightMap[x, z] = fullHeightMap[relativePosX, relativePosZ];
            }
        }

        return splitHeightMap;
    }

    private static List<Terrain> GetTerrainList()
    {
        List<Terrain> terrains = new List<Terrain>();

        Transform t = transform;
        if (t.childCount <= 0)
        {
            Debug.LogError("No Terrain has been generated, please use the node editor to generate a terrain first.");
            return null;
        }

        int idx = 0;
        if (TerrainGrid[0, 0] == null)
        {
            for (int i = 0; i < TerrainDimension; i++)
            {
                for (int j = 0; j < TerrainDimension; j++)
                {
                    TerrainGrid[i, j] = t.GetChild(idx++).gameObject.GetComponent<Terrain>();
                }
            }
        }

        for (int i = 0; i < TerrainDimension; i++)
        {
            for (int j = 0; j < TerrainDimension; j++)
            {
                terrains.Add(TerrainGrid[i, j]);
            }
        }
                
        return terrains;
    }

    private static void SetMaxHeight(float[,] heightMap)
    {
        int size = heightMap.GetLength(0);

        float maxValue = float.MinValue;

        for (int x = 0; x < size; ++x)
        {
            for (int j = 0; j < size; ++j)
            {
                float val = heightMap[x, j];
                if (val > maxValue)
                    maxValue = val;
            }
        }

        textureSettings.MaxTerrainHeight = maxValue;
    }

    private static void ResetGeneration()
    {
        //remove existing terrain
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
       
        TerrainGrid = new Terrain[TerrainDimension, TerrainDimension]; //reset static field
    }
    private static void ConnectTerrainChunks()
    {
        //Generates 9 Terrain chunks
        int iC = 1; //set the middle chunk idx: [1,1], since we have a 9 piece terrain chunk generation
        int jC = 1;
        int iP = PreviousCyclicIndex(iC);
        int jP = PreviousCyclicIndex(jC);
        int iN = NextCyclicIndex(iC);
        int jN = NextCyclicIndex(jC);

        TerrainGrid[iP, jP].SetNeighbors(null, TerrainGrid[iP, jC], TerrainGrid[iC, jP], null);
        TerrainGrid[iC, jP].SetNeighbors(TerrainGrid[iP, jP], TerrainGrid[iC, jC], TerrainGrid[iN, jP], null);
        TerrainGrid[iN, jP].SetNeighbors(TerrainGrid[iC, jP], TerrainGrid[iN, jC], null, null);
        TerrainGrid[iP, jC].SetNeighbors(null, TerrainGrid[iP, jN], TerrainGrid[iC, jC], TerrainGrid[iP, jP]);
        TerrainGrid[iC, jC].SetNeighbors(TerrainGrid[iP, jC], TerrainGrid[iC, jN], TerrainGrid[iN, jC], TerrainGrid[iC, jP]);
        TerrainGrid[iN, jC].SetNeighbors(TerrainGrid[iC, jC], TerrainGrid[iN, jN], null, TerrainGrid[iN, jP]);
        TerrainGrid[iP, jN].SetNeighbors(null, null, TerrainGrid[iC, jN], TerrainGrid[iP, jC]);
        TerrainGrid[iC, jN].SetNeighbors(TerrainGrid[iP, jN], null, TerrainGrid[iN, jN], TerrainGrid[iC, jC]);
        TerrainGrid[iN, jN].SetNeighbors(TerrainGrid[iC, jN], null, null, TerrainGrid[iN, jC]);
    }

    private static int NextCyclicIndex(int i)
    {
        if (i < 0 || i > TerrainDimension - 1)
            Debug.LogError("index outside dim");
        return (i + 1) % TerrainDimension;
    }

    private static int PreviousCyclicIndex(int i)
    {
        if (i < 0 || i > TerrainDimension - 1)
            Debug.LogError("index outside dim");
        return i == 0 ? TerrainDimension - 1 : (i - 1) % TerrainDimension;
    }
}
