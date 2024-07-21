using UnityEngine;

public class ProcedureTerrainGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float oceanLevel = 0.4f;
    public float mountainLevel = 0.7f;

    [SerializeField] private Terrain terrain;
    
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        float[,] heightMap = GenerateHeightMap();
        ApplyHeightMap(heightMap);
        // ColorTerrain();
    }
    
    public float[,] GenerateHeightMap()
    {
        float[,] heightMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;

                float perlinValue = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap[x, y] = perlinValue;
            }
        }

        return heightMap;
    }

    public void ApplyHeightMap(float[,] heightMap)
    {
        terrain.terrainData = GenerateTerrainData(terrain.terrainData, heightMap);
    }

    private TerrainData GenerateTerrainData(TerrainData terrainData, float[,] heightMap)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, 10, height);
        terrainData.SetHeights(0, 0, heightMap);

        return terrainData;
    }

    public void ColorTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainLayer[] terrainLayers = new TerrainLayer[3];

        // Ocean Layer
        terrainLayers[0] = new TerrainLayer();
        terrainLayers[0].diffuseTexture = CreateSolidColorTexture(Color.blue);
        terrainLayers[0].tileSize = new Vector2(width, height);

        // Land Layer
        terrainLayers[1] = new TerrainLayer();
        terrainLayers[1].diffuseTexture = CreateSolidColorTexture(Color.green);
        terrainLayers[1].tileSize = new Vector2(width, height);

        // Mountain Layer
        terrainLayers[2] = new TerrainLayer();
        terrainLayers[2].diffuseTexture = CreateSolidColorTexture(Color.gray);
        terrainLayers[2].tileSize = new Vector2(width, height);

        terrain.terrainData.terrainLayers = terrainLayers;

        float[,,] splatmapData = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, 3];

        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                float height = terrain.terrainData.GetHeight(y, x) / terrain.terrainData.size.y;

                if (height < oceanLevel)
                {
                    splatmapData[x, y, 0] = 1;
                }
                else if (height < mountainLevel)
                {
                    splatmapData[x, y, 1] = 1;
                }
                else
                {
                    splatmapData[x, y, 2] = 1;
                }
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private Texture2D CreateSolidColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}