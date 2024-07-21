using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ConvolutionNode : Node
{
    float[,] convolutionKernel;
    private const int KERNEL_SIZE = 5;

    public ConvolutionNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
        : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 210;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.convolutionNode;
        windowTitle = "Convolution Node";

        convolutionKernel = new float[KERNEL_SIZE, KERNEL_SIZE];
    }

    public override void Draw()
    {
        base.Draw();

        float startX = 15f;
        float startY = 30f;

        float xSize = 25f;
        float ySize = 25f;

        float spacing = 10f;

        float currentX = startX;
        
        for(int x = 0; x < KERNEL_SIZE; ++x)
        {
            float currentY = startY;
            for (int y = 0; y < KERNEL_SIZE; ++y)
            {
                var currentRect = new Rect(currentX, currentY, xSize, ySize);
                convolutionKernel[x, y] = EditorGUI.FloatField(currentRect, convolutionKernel[x, y]);

                currentY += (ySize + spacing);
            }
            currentX += (xSize + spacing);
        }
    }


    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 1)
        {
            return null;
        }

        float[,] heightMap = inPoint.ConnectedTo[0].Calculate(mode);

        if(heightMap == null)
        {
            return null;
        }


        float[,] normalizedKernel = NormalizeKernel();
        if (normalizedKernel == null)
        {
            Debug.LogError("Please enter a valid convolution Kernel");
            return null;
        }

        int size = GetGenerationSize(mode);

        for (int x = KERNEL_SIZE/2; x < size - KERNEL_SIZE/2; x++)
        {
            for (int z = KERNEL_SIZE/2; z < size - KERNEL_SIZE/2; z++)
            {
                float sum = 0f;

                for (int i = -KERNEL_SIZE / 2, matrixI = 0; i <= KERNEL_SIZE / 2; ++i, ++matrixI)
                {
                    for (int j = -KERNEL_SIZE / 2, matrixJ = 0; j <= KERNEL_SIZE / 2; ++j, ++matrixJ)
                    {
                        int srcX = x + i;
                        int srcY = z + j;
                        sum += (heightMap[srcX, srcY] * normalizedKernel[matrixI, matrixJ]);
                    }
                }
                heightMap[x, z] = sum;
            }
        }

        return heightMap;
    }

    private float[,] NormalizeKernel()
    {
        float sum = 0.0f;

        for (int x = 0; x < KERNEL_SIZE; ++x)
        {
            for (int y = 0; y < KERNEL_SIZE; ++y)
            {
                sum += convolutionKernel[x, y];
            }
        }

        if(Mathf.Abs(sum) < 0.1f)
        {
            return null; //we need a bigger sum than this
        }

        float[,] normalizedKernel = new float[KERNEL_SIZE, KERNEL_SIZE];

        for (int x = 0; x < KERNEL_SIZE; ++x)
        {
            for (int y = 0; y < KERNEL_SIZE; ++y)
            {
                normalizedKernel[x, y] = convolutionKernel[x, y] /  sum;
            }
        }

        return normalizedKernel;
    }
}
