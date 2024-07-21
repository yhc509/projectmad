/***
 * This class is used to for placing trees on the already generated terrain
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VegetationGenerator
{
    private static string vegetationParent = "Vegetation Container";
    private static string treeParentName = "Tree Parent";
    private static string detailsParentName = "Details Parent";
    private static string bushParentName = "Bush Parent";

    public static void PlaceTrees(List<Terrain> terrains)
    {
        var vegetationParent = GameObject.Find(VegetationGenerator.vegetationParent);
        if(vegetationParent == null)
        {
            //create new tree parent
            vegetationParent = new GameObject(VegetationGenerator.vegetationParent);
        }
        else
        {
            //remove all children
            var transform = vegetationParent.transform;
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        var textureSettings = VegetationSettings.Load();
        foreach(var terrain in terrains)
        {
            PlaceTreesOnTerrain(terrain, textureSettings, vegetationParent.transform);
        }
    }

    private static void PlaceTreesOnTerrain(Terrain terrain, VegetationSettings vegetationSettings, Transform vegetationParent)
    {
        var terrainParent = new GameObject(terrain.gameObject.name);
        var treeParent = new GameObject(treeParentName);
        var detailsParent = new GameObject(detailsParentName);
        var bushParent = new GameObject(bushParentName);

        terrainParent.transform.SetParent(vegetationParent);
        treeParent.transform.SetParent(terrainParent.transform);
        detailsParent.transform.SetParent(terrainParent.transform);
        bushParent.transform.SetParent(terrainParent.transform);

        //go through textures
        for (int layerIdx = 0; layerIdx < vegetationSettings.LayerCount; ++layerIdx) 
        {
            var layer = vegetationSettings.Layers[layerIdx];
            if (layer.TreePrefab != null && layer.TreeCount > 0)
            {
                //trees
                var newTreePositions = GetTreePositions(terrain, layerIdx, layer.TreeCount / TerrainOptiones.TerrainChunkCount, vegetationSettings.TreePlacingCoeffitient);
                PlaceObjects(newTreePositions, layer.TreePrefab, treeParent.transform, 0.1f);

                //details
                var newDetailsPosition = GetDetailPositions(terrain, layerIdx, layer.DetailCount / TerrainOptiones.TerrainChunkCount, vegetationSettings.TreePlacingCoeffitient, 0.025f);
                PlaceObjects(newDetailsPosition, layer.DetailPrefabs, detailsParent.transform, scaleOffsetMin: 0.01f, scaleOffsetMax: 0.05f);

                //bushes
                var newBushPositions = GetDetailPositions(terrain, layerIdx, layer.BushCount/ TerrainOptiones.TerrainChunkCount, vegetationSettings.TreePlacingCoeffitient, 0.05f);
                PlaceObjects(newBushPositions, layer.BushPrefabs, bushParent.transform, scaleOffsetMax : 1.4f);
            }
        }
    }

    private static void PlaceObjects(List<Vector3> positions, GameObject[] prefabs, Transform parent, float yOffset = 0.0f, float scaleOffsetMin = 0.9f, float scaleOffsetMax = 1.1f)
    {
        foreach(var pos in positions)
        {
            GameObject newTree = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, Quaternion.identity);
            float scaleOffset = Random.Range(scaleOffsetMin, scaleOffsetMax);
            newTree.transform.localScale = new Vector3(scaleOffset, scaleOffset, scaleOffset);
            newTree.transform.SetParent(parent, true);
            newTree.transform.localRotation = Quaternion.Euler(Random.Range(0f, 2f), Random.Range(0f, 360f), Random.Range(0f, 2f));
            newTree.transform.position = new Vector3(newTree.transform.position.x, newTree.transform.position.y - yOffset, newTree.transform.position.z);
        }
    }

    private static List<Vector3> GetTreePositions(Terrain terrain, int layerIdx, int treeCount, float perlinThreshold)
    {
        List<Vector3> treePositions = new List<Vector3>();
        List<Vector3> possibleTreeSites = new List<Vector3>();
        int terrainPosX = (int)terrain.transform.position.x;
        int terrainPosZ = (int)terrain.transform.position.z;
        
        int terrainSize = (int)terrain.terrainData.size.z;

        int alphaMapSize = terrain.terrainData.alphamapWidth;

        float worldPosScale = (float)terrainSize / (float)alphaMapSize;

        var textureSettings = TextureSettings.Load();
        var splatMap = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        int splatCount = textureSettings.LayerCount;

        for (int x = 0; x < alphaMapSize; x += 2)
        {
            for (int z = 0; z < alphaMapSize; z += 2)
            {
                //steepness / slope limits
                //float normX = x * 1.0f / (alphaMapSize - 1);
                //float normZ = z * 1.0f / (alphaMapSize - 1);
                //float angle = terrain.terrainData.GetSteepness(normX, normZ);

                //if (angle > maxAngle || angle < minAngle)
                //    continue;

                //get the grouping togather thing going
                float perlinValue = Mathf.PerlinNoise(x*0.025f , z * 0.025f);
                if (perlinValue < perlinThreshold)
                    continue;
                
                int textureIndexAtCheckPos = GetMainTextureIdx(new Vector3(x, 0, z), splatMap, splatCount);

                if (textureIndexAtCheckPos == layerIdx)
                {
                    Vector3 checkPos = new Vector3(terrainPosX + x * worldPosScale, 0, terrainPosZ + z * worldPosScale);

                    possibleTreeSites.Add(checkPos);
                }
                
            }
        }


        if (treeCount > possibleTreeSites.Count)
        {
            treeCount = possibleTreeSites.Count;
        }


        for (int i = 0; i < treeCount; ++i)
        {
            int randomIndex = Random.Range(0, possibleTreeSites.Count);
            //get Y-s
            Vector3 treeSite = possibleTreeSites[randomIndex];
            float yPos = terrain.SampleHeight(treeSite);
            treeSite.y = yPos + terrain.GetPosition().y - 0.65f; //remove a small offset so we do not see the tree bottoms on sloped terrain

            possibleTreeSites.RemoveAt(randomIndex);

            treePositions.Add(treeSite);
        }

        return treePositions;
    }


    private static List<Vector3> GetDetailPositions(Terrain terrain, int layerIdx, int detailCount, float perlinThreshold, float perlinMultiplier)
    {

        List<Vector3> finalPositions = new List<Vector3>();
        List<Vector3> possiblePossitions = new List<Vector3>();
        int terrainPosX = (int)terrain.transform.position.x;
        int terrainPosZ = (int)terrain.transform.position.z;
        int terrainSize = (int)terrain.terrainData.size.z;
        int alphaMapSize = terrain.terrainData.alphamapWidth;
        float worldPosScale = (float)terrainSize / (float)alphaMapSize;

        var textureSettings = TextureSettings.Load();
        var splatMap = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        int splatCount = textureSettings.LayerCount;

        for (int x = 0; x < alphaMapSize; x += 1)
        {
            for (int z = 0; z < alphaMapSize; z += 1)
            {
                //get the grouping togather thing going
                float perlinValue = Mathf.PerlinNoise(x * perlinMultiplier, z * perlinMultiplier);
                if (perlinValue < perlinThreshold)
                    continue;

                int textureIndexAtCheckPos = GetMainTextureIdx(new Vector3(x, 0, z), splatMap, splatCount);
                if (textureIndexAtCheckPos == layerIdx)
                {
                    Vector3 checkPos = new Vector3(terrainPosX + x * worldPosScale, 0, terrainPosZ + z * worldPosScale);
                    possiblePossitions.Add(checkPos);
                }

            }
        }

        if (detailCount > possiblePossitions.Count)
        {
            detailCount = possiblePossitions.Count;
        }


        for (int i = 0; i < detailCount; ++i)
        {
            int randomIndex = Random.Range(0, possiblePossitions.Count);
            Vector3 detailSite = possiblePossitions[randomIndex];
            float yPos = terrain.SampleHeight(detailSite);
            detailSite.y = yPos + terrain.GetPosition().y;

            possiblePossitions.RemoveAt(randomIndex);
            finalPositions.Add(detailSite);
        }

        return finalPositions;
    }

    private static int GetMainTextureIdx(Vector3 pos, float[,,] splatMap, int splatCount)
    {
        int maxIdx = 0;
        float maxValue = float.MinValue;
        for(int i = 0; i < splatCount; ++i)
        {
            var value = splatMap[Mathf.FloorToInt(pos.z), Mathf.FloorToInt(pos.x), i];
            if(value > maxValue)
            {
                maxValue = value;
                maxIdx = i;
            }
        }

        return maxIdx;
    }

    private static float[] GetTextureMix(TerrainData terrainData, Vector3 pos)
    {
        var splatMap = terrainData.GetAlphamaps((int)pos.x, (int)pos.z, 1, 1);

        var cellMix = new float[splatMap.GetUpperBound(2) + 1];
        for(int i = 0; i < cellMix.Length; ++i)
        {
            cellMix[i] = splatMap[0, 0, i];
        }

        return cellMix;
    }
}
