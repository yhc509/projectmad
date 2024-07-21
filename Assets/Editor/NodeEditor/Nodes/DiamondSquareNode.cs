using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class DiamondSquareNode : Node
{
    private int seed = 0;
    private float roughness = 5f;

    public DiamondSquareNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 180;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.diamondSquareNode;
        windowTitle = "Diamond Square Node";
    }

    public override void Draw()
    {
        EditorGUI.LabelField(new Rect(15, 20, windowRect.width - 30, 20), "Seed");
        seed = EditorGUI.IntSlider(new Rect(15, 40, windowRect.width - 30, 20), seed, 1, 100);

        EditorGUI.LabelField(new Rect(15, 70, windowRect.width - 30, 20), "Roughness");
        roughness = EditorGUI.Slider(new Rect(15, 90, windowRect.width - 30, 20), roughness, 1f, 10f);

        outPoint.Draw();
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }

    private int GetNextPowerOfTwo(int x)
    {
        int nextPowerOfTwo = 1;

        for(int i = 1; i < 15; ++i)
        {
            int tmp = (int) Math.Pow(2, i);
            if(tmp >= x)
            {
                nextPowerOfTwo = tmp;
                break;
            }
        }
        return nextPowerOfTwo;
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        int size = GetGenerationSize(GenerationMode.fullMap); //we need to generate the full map example to get a accurate representation

        //if the size is not 2^x +1 ... just make it work on the next best size and downscale it afterwards
        if (!IsPowerOfTwo(size - 1))
        {
            //update size to a power of 2
            size = GetNextPowerOfTwo(size) + 1;
        }

        float[,] data = new float[size, size];
        data[0, 0] = data[0, size - 1] = data[size - 1, 0] = data[size - 1, size - 1] = seed;

        double h = roughness;
        System.Random r = new System.Random(seed);

        for (int sideLength = size - 1; sideLength >= 2; sideLength /= 2, h /= 2.0)
        {
            int halfSide = sideLength / 2;

            //generate the new square values
            for (int x = 0; x < size - 1; x += sideLength)
            {
                for (int y = 0; y < size - 1; y += sideLength)
                {
                    double avg = data[x, y] + //top left
                    data[x + sideLength, y] +//top right
                    data[x, y + sideLength] + //lower left
                    data[x + sideLength, y + sideLength];//lower right
                    avg /= 4.0;

                    //center is average plus random offset
                    data[x + halfSide, y + halfSide] = (float) (avg + (r.NextDouble() * 2.0 * h) - h);
                }
            }
            for (int x = 0; x < size - 1; x += halfSide)
            {
                for (int y = (x + halfSide) % sideLength; y < size - 1; y += sideLength)
                {
                    double avg =
                      data[(x - halfSide + size) % size, y] + //left of center
                      data[(x + halfSide) % size, y] + //right of center
                      data[x, (y + halfSide) % size] + //below center
                      data[x, (y - halfSide + size) % size]; //above center
                    avg /= 4.0;

                    avg = avg + (r.NextDouble() * 2 * h) - h;
                    data[x, y] = (float) avg;

                    if (x == 0) data[size - 1, y] = (float)avg;
                    if (y == 0) data[x, size - 1] = (float)avg;
                }
            }
        }

        //cut off the exess and normalize the map
        size = GetGenerationSize(GenerationMode.fullMap);
        float[,] tmpData = data;
        data = new float[size, size];
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                data[i, j] = Mathf.Clamp01(1f - Mathf.Clamp01(tmpData[i, j]));
            }
        }

        //check if we have to downsample the whole map for our purpose only
        int actualSize = GetGenerationSize(mode);
        if(actualSize != size)
        {
            float[,] previewData = new float[actualSize, actualSize];

            int idxAddAmt = size / actualSize;
            //we are generating the preview ... downsize to the actualsize
            int x = 0, y;
            for(int i = 0; i < actualSize; ++i)
            {
                y = 0;
                for(int j = 0; j < actualSize; ++j)
                {
                    previewData[i, j] = data[x, y];
                    y += idxAddAmt;
                }
                x += idxAddAmt;
            }

            return previewData;
        }
        else
        {
            return data;
        }
    }
}
