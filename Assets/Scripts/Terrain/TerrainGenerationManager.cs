using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationManager
{
    public static Queue<Generator> patchQueue = new Queue<Generator>();

    public static void Execute()
    {
        int patchCount = patchQueue.Count;
        for (int i = 0; i < patchCount; i++)
            patchQueue.Dequeue().Generate();
    }
    
    public static void CreateMap(Terrain t, Vector3 newPos, float[,] heightMap)
    {
        patchQueue.Enqueue(new TerrainGenerator(t, newPos, heightMap));
        patchQueue.Enqueue(new TextureGenerator(t));
    }

    public static void RetextureTerrain(Terrain t)
    {
        patchQueue.Enqueue(new TextureGenerator(t));
    }

    public static void SimulateErosion(Terrain t, Vector3 newPos)
    {
        patchQueue.Enqueue(new TerrainGenerator(t, newPos));

        int patchCount = patchQueue.Count;
        for (int i = 0; i < patchCount; i++)
            patchQueue.Dequeue().HydrolicErosion();
    }


}
