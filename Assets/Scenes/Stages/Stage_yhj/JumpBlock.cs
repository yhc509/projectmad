using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class JumpBlock : MonoBehaviour
{
    [SerializeField] private float _jumpPower;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player");
            var player = other.gameObject.GetComponent<ThirdPersonController>();
            player.Grounded = false;
            player.SetVerticalVelocity(_jumpPower);
        }
        
    }
}
