using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomTransformNode
{
    float[,] Calculate(int size, float[,] input);
}
