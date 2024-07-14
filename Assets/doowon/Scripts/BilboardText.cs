using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilboardText : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void LateUpdate()
    {
        if(target.gameObject.activeSelf == false)
        {
            return;
        }
        transform.position = target.position + offset;
        transform.LookAt(Camera.main.transform);
    }
}
