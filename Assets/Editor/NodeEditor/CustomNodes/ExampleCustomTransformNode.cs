using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCustomTransformNode : ICustomTransformNode
{
    public float[,] Calculate(int size, float[,] input)
    {
        for(int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                input[x, y] *= 2.0f;
            }
        }

        return input;
    }
}
