using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    Rigidbody rb;
    Transform _cameraTr;
    [SerializeField] float BOUNCE_POWER = 300.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _cameraTr = GameObject.Find("Main Camera").transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Mathf.Abs(collision.impulse.z) < 0.01f && 
            Mathf.Abs(collision.impulse.x) < 0.01f)
        {

            rb.AddForce(0.0f, 8.0f, 0.0f, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0.0f;

        if (isMove)
        {
            //Vector3 lookForward = new Vector3(_cameraTr.forward.x, 0.0f, _cameraTr.forward.z).normalized;
            //Vector3 lookRight = new Vector3(_cameraTr.right.x, 0.0f, _cameraTr.right.z).normalized;
            //Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;
            Vector3 moveDir = new Vector3(moveInput.x, 0.0f, moveInput.y);

            transform.forward = moveDir;
            transform.position += moveDir * Time.deltaTime * 5.0f;            
        }
    }
}
