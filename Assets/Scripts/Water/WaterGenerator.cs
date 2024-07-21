using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WaterGenerator
{
    private const string waterParentName = "WaterParent";
    public static void GenerateWater(Transform parent, Vector3 offset)
    {
        //check if water object already exists
        int childCount = parent.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            if (parent.GetChild(i).name == waterParentName)
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        GameObject waterParent = new GameObject(waterParentName);
        waterParent.transform.SetParent(parent);

        var size = TerrainOptiones.WaterTileSize;
        var scale = TerrainOptiones.WaterTileScale;
        int tileCount = TerrainOptiones.WaterTileCount;
        var settings = TextureSettings.Load();
        var waterMaterial = settings.WaterMaterial;
        
        for(int x = 1; x <= tileCount; ++x)
        {
            for (int z = 1; z <= tileCount; ++z)
            {
                GenerateWaterTile(waterParent.transform, waterMaterial, scale, size, x, z);
            }
        }

        waterParent.transform.localPosition = offset;
    }

    private static void GenerateWaterTile(Transform parent, Material waterMaterial, 
        float scale, int size, int idxX, int idxZ)
    {
        GameObject waterTile = new GameObject("WaterTile");
        MeshRenderer meshRenderer = waterTile.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = waterTile.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        waterTile.transform.SetParent(parent);
        var offset = new Vector3(-idxX * size * scale, 0f, -idxZ * size * scale);

        Vector3[] vertices;
        vertices = new Vector3[(size + 1) * (size + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, z = 0; z <= size; z++)
        {
            for (int x = 0; x <= size; x++, i++)
            {
                vertices[i] = new Vector3(x, 0f, z);
                uv[i] = new Vector2((float)x / size, (float)z / size);
                tangents[i] = tangent;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] triangles = new int[size * size * 6];
        for (int ti = 0, vi = 0, y = 0; y < size; y++, vi++)
        {
            for (int x = 0; x < size; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + size + 1;
                triangles[ti + 5] = vi + size + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshRenderer.material = waterMaterial;

        waterTile.transform.localPosition = offset;
        waterTile.transform.localScale = new Vector3(scale, scale, scale);
    }

}
