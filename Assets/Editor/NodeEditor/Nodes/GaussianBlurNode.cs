using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GaussianBlurNode : Node
{
    private static int FILTER_SIZE = 21;
    public GaussianBlurNode(Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
         : base(position, OnClickInPoint, OnClickOutPoint)
    {
        baseHeight = 60;
        windowRect = new Rect(position.x, position.y, baseWidth, baseHeight);

        nodeType = NodeType.gaussianBlurNode;
        windowTitle = "Gaussian Blur Node";
    }

    public override void Draw()
    {
        base.Draw();
        if (inPoint.ConnectedTo.Count != 1)
        {
            EditorGUI.LabelField(new Rect(20, 20, windowRect.width - 40, 20), "One entry node required");
        }
    }

    public override float[,] Calculate(GenerationMode mode)
    {
        if (inPoint.ConnectedTo.Count != 1)
        {
            return null;
        }

        return ApplyGaussianBlurr(inPoint.ConnectedTo[0].Calculate(mode), mode);
    }

    private float[,] ApplyGaussianBlurr(float[,] input, GenerationMode mode)
    {
        if (input == null)
        {
            return null;
        }

        int size = GetGenerationSize(mode);

        float[,] filter = GenerateFilter();

        
        for (int x = FILTER_SIZE/2; x < size - FILTER_SIZE/2; ++x)
        {
            for (int y = FILTER_SIZE/2; y < size - FILTER_SIZE/2; ++y)
            {
                float sum = 0f;
                
                for(int i = - FILTER_SIZE/2, matrixI = 0; i <= FILTER_SIZE/2; ++i, ++matrixI)
                {
                    for (int j = -FILTER_SIZE / 2, matrixJ = 0; j <= FILTER_SIZE / 2; ++j, ++matrixJ)
                    {
                        int srcX = x + i;
                        int srcY = y + j;
                        sum += (input[srcX, srcY] * filter[matrixI, matrixJ]);
                    }
                }

                input[x, y] = sum;
            }
        }

        return input;
    }

    private float[,] GenerateFilter(float stdv = 1.0f)
    {
        float[,] filter = new float[FILTER_SIZE, FILTER_SIZE];

        float r, s = 2.0f * stdv * stdv;
        float sum = 0.0f;

        for(int x = -FILTER_SIZE/2; x <= FILTER_SIZE/2; ++x)
        {
            for(int y = -FILTER_SIZE / 2; y <= FILTER_SIZE / 2; ++y)
            {
                r = (float) Math.Sqrt(x * x + y * y);
                filter[x + FILTER_SIZE / 2, y + FILTER_SIZE / 2] = (float) (Math.Exp(-(r * r) / s)) / (Mathf.PI * s);
                sum += filter[x + FILTER_SIZE / 2, y + FILTER_SIZE / 2];
            }
        }

        for(int i = 0; i < FILTER_SIZE; ++i)
        {
            for(int j = 0; j < FILTER_SIZE; ++j)
            {
                filter[i, j] /= sum;
            }
        }

        return filter;
    }
}
