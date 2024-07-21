using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextureGenerator : Generator
{
    private Terrain terrain;
    private TextureSettings textureSettings;
    private float[,,] alphaMap;
    public TextureGenerator(Terrain terrain)
    {
        this.terrain = terrain;
        this.textureSettings = TextureSettings.Load();
        alphaMap = new float[TerrainOptiones.alphaMapSize, TerrainOptiones.alphaMapSize, textureSettings.LayerCount];
    }

    public override void Generate()
    {
        CreateTextureMap();
        terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    private void CreateTextureMap()
    {
        /*
         * [0] = water
         * [1] = sand
         * [2] = forst
         * [3] = rock _ lower
         * [4] = rock _ higher
         * [5] = snow
         */
        //float snowHeight = terrainSettings.snowHeight;
        
        float snowHeight = textureSettings.Layers[textureSettings.LayerCount - 1].StartHeight;
        float beachHeight = 80;
        float rockHeight = 250;
        int alphaMapSize = TerrainOptiones.alphaMapSize;
        float waterHeight = textureSettings.Layers[1].StartHeight;
        int splatCount = textureSettings.LayerCount;

        float relMaxHeight = textureSettings.MaxTerrainHeight * TerrainOptiones.TerrainHeight;

        for (int x = 0; x < alphaMapSize; x++)
        {
            for (int z = 0; z < alphaMapSize; z++)
            {
                float normX = x * 1.0f / (alphaMapSize - 1);
                float normZ = z * 1.0f / (alphaMapSize - 1);

                float angle = terrain.terrainData.GetSteepness(normX, normZ) / 45f;
                float height = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
                float relativeHeight = height / relMaxHeight;

                float[] splatWeights = new float[splatCount];
                if (relativeHeight < waterHeight + 0.001f)
                {
                    splatWeights[0] = 1f;
                    splatWeights[1] = 0f;
                    splatWeights[2] = 0f;
                    splatWeights[3] = 0f;
                    splatWeights[4] = 0f;
                }
                else if(relativeHeight > snowHeight + Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 0.2f )
                {
                    //we are above tree height
                    splatWeights[0] = 0f;
                    splatWeights[1] = 0f;
                    splatWeights[2] = 0f;
                    splatWeights[3] = 0f;


                    if(angle < 0.5f)
                    {
                        splatWeights[4] = 0.3f;
                        splatWeights[5] = 0.7f;
                    }
                    else
                    {
                        splatWeights[4] = 0.7f;
                        splatWeights[5] = 0.3f;
                    }

                    splatWeights[4] = 0.3f * angle;
                    splatWeights[5] = 0.7f * (1f - angle);

                }
                else
                {
                    
                    
                    float relLowerHeight = Mathf.Clamp01(Mathf.Pow(height / beachHeight, 3) + Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 0.1f);
                    float relRockHeight = Mathf.Clamp01(Mathf.Pow(height / rockHeight, 5));

                    splatWeights[0] = splatWeights[5] = 0f;                         //no snow or water
                    splatWeights[1] = 1 - angle - (1 - angle) * relLowerHeight;     //sandy areas
                    if(height > rockHeight + Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 0.2f)
                    {
                        splatWeights[2] = (1 - angle) * 0.2f;                //forst on low and flat areas
                    }
                    else
                    {
                        splatWeights[2] = (1 - angle) * relLowerHeight;                //forst on low and flat areas
                    }
                    splatWeights[3] = angle * (1 - angle) * relLowerHeight;
                    splatWeights[4] = (1 - angle) * relRockHeight;                  //rocky planes higher up
                    //splatWeights[4] = angle * (1 - angle) * relRockHeight;  // for more forsty planes everywhere
                }

                //normalize data
                float sum = splatWeights.Sum();
                for (int i = 0; i < splatCount; ++i)
                {
                    alphaMap[z, x, i] = splatWeights[i] / sum;
                }
            }
        }
    }
}
