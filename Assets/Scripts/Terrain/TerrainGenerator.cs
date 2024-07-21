using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : Generator
{
    private Vector3 pos;
    private Terrain terrain;
    float[,] heightMap;
    float[,] erosionMap;
    
    int size => heightMap.GetLength(0);

    float erosionRate;
    float depositRate;
    float maxErosionAmt;
    int iterations;
    int windSimulationInterval;

    public TerrainGenerator(Terrain terrain, Vector3 pos, float[,] heightMap)
    {
        this.terrain = terrain;
        this.pos = pos;
        this.heightMap = heightMap;
    }

    public override void Generate()
    {
        terrain.terrainData.SetHeights(0, 0, heightMap);  //SetHeights calculates terrain collider
        terrain.transform.position = pos;
    }


    public TerrainGenerator(Terrain terrain, Vector3 pos,
        float erosionRate = 0.01f, float depositRate = 0.001f, float maxErosionAmt = 0.01f,
        int iterations = 50000, int windSimulationCount = 10)
    {
        this.terrain = terrain;
        this.pos = pos;
        this.erosionRate = erosionRate;
        this.depositRate = depositRate;
        this.maxErosionAmt = maxErosionAmt;
        this.iterations = iterations;
        this.windSimulationInterval = iterations / windSimulationCount;
    }

    public override void HydrolicErosion()
    {
        var heightMapSize = terrain.terrainData.heightmapResolution;

        heightMap = terrain.terrainData.GetHeights(0, 0, heightMapSize, heightMapSize);

        PerformErosion();

        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    private void PerformErosion()
    {
        erosionMap = new float[size, size];

        for(int i = 0; i <= iterations; ++i)
        {
            int posX = UnityEngine.Random.Range(1, size - 2);
            int posY = UnityEngine.Random.Range(1, size - 2);
            SimulateDroplet(posX, posY);
            if(i % windSimulationInterval == 0)
            {
                //run blurr filter through height field
                SimulateWindBlur();
            }
        }

        //apply erosion map to height map
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                heightMap[i, j] = Mathf.Clamp01(heightMap[i, j] - erosionMap[i, j]);
            }
        }
    }

    private void SimulateWindBlur()
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

        float[,] erosiomMapCopy = (float[,]) erosionMap.Clone();

        const int kernelSize = 3;

        for(int x = 1; x < size - 1; ++x)
        {
            for (int y = 1; y < size - 1; ++y)
            {
                float sum = 0.0f;
                //apply kernel
                for(int i = -kernelSize/2, kernelX = 0 ; i <= kernelSize/2; ++i, ++kernelX)
                {
                    for (int j = -kernelSize/2, kernelY = 0; j <= kernelSize/2; ++j, ++ kernelY)
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

    private void SimulateDroplet(int x, int z)
    {

        //spawn a droplet at a random position
        float totalSoilAmt = 0f;

        int maxIterations = 20;
        int iterations = 0;

        //we are getting heights from 0 to 1 ey?
        while(iterations < maxIterations)
        {

            //find lowest neighbour
            var newPos = GetLowestNeighbour(x, z);
            if(newPos == Vector2Int.zero)
            {
                //no lower neighbor, deposit carried soil
                //DepositeSoil(x, z, depositRate); //make this a constant for now
                return;
            }

            //pick up soil from current point
            totalSoilAmt += RemoveSoil(x, z);

            //update point
            x = newPos.x;
            z = newPos.y;

            //totalSoilAmt = Mathf.Clamp01(totalSoilAmt - RemoveCarriedSoil(x, z, totalSoilAmt));

            iterations++;
        }
    }

    private float RemoveCarriedSoil(int x, int z, float totalSoilAmt)
    {
        float normX = x * 1.0f / (size - 1);
        float normZ = z * 1.0f / (size - 1);
        float angle = terrain.terrainData.GetSteepness(normX, normZ) / 360f;

        float deposit = totalSoilAmt * depositRate * angle;
        heightMap[x, z] = Mathf.Clamp01(heightMap[x, z] + deposit);
        return deposit;
    }

    private float RemoveSoil(int x, int z)
    {
        float normX = x * 1.0f / (size - 1);
        float normZ = z * 1.0f / (size - 1);
        float angle = terrain.terrainData.GetSteepness(normX, normZ) / 360f;

        float soilAmt = erosionRate * angle;
        erosionMap[x, z] = Mathf.Min(erosionMap[x, z] + soilAmt, maxErosionAmt);

        return soilAmt;
    }

    private void DepositeSoil(int x, int z, float amt)
    {
        heightMap[x, z] = Mathf.Clamp01(heightMap[x, z] + amt);
    }

    private Vector2Int GetLowestNeighbour(int posX, int posY)
    {
        //check weather we can move in all 8 directions
        if (posX - 1 < 0 || posX + 1 >= size || posY + 1 >= size || posY - 1 < 0)
            return Vector2Int.zero;

        Vector2Int returnPos = Vector2Int.zero;

        for(int x = posX - 1; x <= posX + 1; ++x)
        {
            for (int y = posY - 1; y <= posY + 1; ++y)
            {
                if(heightMap[x,y] < heightMap[returnPos.x, returnPos.y])
                {
                    returnPos = new Vector2Int(x, y);
                }
            }
        }

        return returnPos;
    }

}
