using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomSourceNode
{
    float[,] Calculate(int size);
}
