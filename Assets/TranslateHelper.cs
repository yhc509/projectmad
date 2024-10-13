using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateHelper : MonoBehaviour
{
    [SerializeField] MovingObstacle movingObs;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("player is on mo, enter");

            var player = other.GetComponent<ThirdPersonController>();
            movingObs._steppedPlayerTr = player.transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("player is on mo. stay");
        
            var player = other.GetComponent<ThirdPersonController>();
            movingObs._steppedPlayerTr = player.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("player is on mo. stay");

            var player = other.GetComponent<ThirdPersonController>();
            movingObs._steppedPlayerTr = null;
        }
    }
}
