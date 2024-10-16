using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public Transform _spawnPosition;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = _spawnPosition.position;
    }
}
