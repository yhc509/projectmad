using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HydrolicErosion
{
    private static List<PreviousTerrain> previousTerrains;
    public static bool CanRevertSimulation => !(previousTerrains == null || previousTerrains.Count <= 0);

    public static void SimulateHydrolicErosion(List<Terrain> terrains, HydrolicErosionSettings settings)
    {
        PreviousTerrain previousTerrain = new PreviousTerrain();

        foreach (var terrain in terrains)
        {
            int size = terrain.terrainData.heightmapResolution;
            settings.Size = size;

            var erosionMap = new float[size, size];
            var heightMap = terrain.terrainData.GetHeights(0, 0, size, size);
            previousTerrain.Add(terrain, heightMap);

            for (int i = 0; i <= settings.Iterations; ++i)
            {
                int posX = Random.Range(1, size - 2);
                int posY = Random.Range(1, size - 2);
                SimulateDroplet(terrain, posX, posY, size, heightMap, erosionMap, settings);
                if (i % settings.WindSimulationInterval == 0)
                {
                    //run blurr filter through height field
                    SimulateWindBlur(erosionMap, size);
                }
            }

            //apply erosion map to height map
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    heightMap[i, j] = Mathf.Clamp01(heightMap[i, j] - erosionMap[i, j]);
                }
            }

            terrain.terrainData.SetHeights(0, 0, heightMap);
        }
        
        if(previousTerrains == null)
        {
            previousTerrains = new List<PreviousTerrain>();
        }

        previousTerrains.Add(previousTerrain);
    }

    private static void SimulateDroplet(Terrain terrain, int x, int z, int size, float[,] heightMap, float[,] erosionMap, HydrolicErosionSettings settings)
    {
        float totalSoilAmt = 0f;
        int iterations = 0;

        while (iterations < settings.DropletLifetime)
        {

            //find lowest neighbour
            var newPos = GetLowestNeighbour(x, z, size, heightMap);
            if (newPos == Vector2Int.zero)
            {
                //no lower neighbor, deposit carried soil
                //DepositeSoil(x, z, depositRate); //make this a constant for now
                return;
            }

            //pick up soil from current point
            totalSoilAmt += RemoveSoil(terrain, x, z, settings, erosionMap);

            //update point
            x = newPos.x;
            z = newPos.y;

            //totalSoilAmt = Mathf.Clamp01(totalSoilAmt - RemoveCarriedSoil(x, z, totalSoilAmt));

            iterations++;
        }
    }

    private static void SimulateWindBlur(float[,] erosionMap, int size)
    {
        float[,] blurKernel = new float[,] {
            { 1 / 16f, 2 / 16f, 1 / 16f },
            { 2 / 16f, 4 / 16f, 2 / 16f },
            { 1 / 16f, 2 / 16f, 1 / 16f }
        };

        //float[,] blurKernel = new float[,] {
        //    { 1 / 9f, 1 / 9f, 1 / 9f},
        //    { 1 / 9f, 1 / 9f, 1 / 9f},
        //    { 1 / 9f, 1 / 9f, 1 / 9f}
        //};

        //float[,] blurKernel = new float[,] {
        //    { 1 / 16f, 1 / 16f, 1 / 16f },
        //    { 1 / 16f, 8 / 16f, 1 / 16f },
        //    { 1 / 16f, 1 / 16f, 1 / 16f }
        //};

        float[,] erosiomMapCopy = (float[,])erosionMap.Clone();

        const int kernelSize = 3;

        for (int x = 1; x < size - 1; ++x)
        {
            for (int y = 1; y < size - 1; ++y)
            {
                float sum = 0.0f;
                //apply kernel
                for (int i = -kernelSize / 2, kernelX = 0; i <= kernelSize / 2; ++i, ++kernelX)
                {
                    for (int j = -kernelSize / 2, kernelY = 0; j <= kernelSize / 2; ++j, ++kernelY)
                    {
                        int currentX = x - i;
                        int currentY = y - j;
                        sum += (erosiomMapCopy[currentX, currentY] * blurKernel[kernelX, kernelY]);
                    }
                }
                erosionMap[x, y] = Mathf.Clamp01(sum);
            }
        }
    }

    private static float RemoveSoil(Terrain terrain, int x, int z, HydrolicErosionSettings settings, float[,] erosionMap)
    {
        float normX = x * 1.0f / (settings.Size - 1);
        float normZ = z * 1.0f / (settings.Size - 1);
        float angle = terrain.terrainData.GetSteepness(normX, normZ) / 360f;

        float soilAmt = settings.ErosionRate * angle;
        erosionMap[x, z] = Mathf.Min(erosionMap[x, z] + soilAmt, settings.MaxErosionAmt);

        return soilAmt;
    }

    private static Vector2Int GetLowestNeighbour(int posX, int posY, int size, float[,] heightMap)
    {
        //check weather we can move in all 8 directions
        if (posX - 1 < 0 || posX + 1 >= size || posY + 1 >= size || posY - 1 < 0)
            return Vector2Int.zero;

        Vector2Int returnPos = Vector2Int.zero;

        for (int x = posX - 1; x <= posX + 1; ++x)
        {
            for (int y = posY - 1; y <= posY + 1; ++y)
            {
                if (heightMap[x, y] < heightMap[returnPos.x, returnPos.y])
                {
                    returnPos = new Vector2Int(x, y);
                }
            }
        }

        return returnPos;
    }


    

    public static void RevertSimulation()
    {
        if (!CanRevertSimulation) return;

        PreviousTerrain prevTerrain = previousTerrains[previousTerrains.Count - 1];
        previousTerrains.RemoveAt(previousTerrains.Count - 1);

        List<Terrain> terrains = prevTerrain.Terrains;
        List<float[,]> heightMaps = prevTerrain.HeightMaps;

        for(int i = 0; i < terrains.Count; ++i)
        {
            terrains[i].terrainData.SetHeights(0, 0, heightMaps[i]);
        }
    }

    private class PreviousTerrain
    {
        public List<Terrain> Terrains { get; private set; }
        public List<float[,]> HeightMaps { get; private set; }

        public PreviousTerrain()
        {
            Terrains = new List<Terrain>();
            HeightMaps = new List<float[,]>();
        }

        public void Add(Terrain terrain, float[,] heightMap)
        {
            Terrains.Add(terrain);
            HeightMaps.Add((float[,]) heightMap.Clone());
        }
    }
}
