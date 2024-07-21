using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCustomSourceNode : ICustomSourceNode
{
    public float[,] Calculate(int size)
    {
        float[,] test = new float[size, size];

        for(int x = 0; x < size; ++x)
        {
            for(int z = 0; z < size; ++z)
            {
                test[x, z] = Random.Range(0f, 1f);
            }
        }
        return test;

    }
}
