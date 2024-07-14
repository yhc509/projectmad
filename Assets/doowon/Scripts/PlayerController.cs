using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float mouseSensitivity = 100f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    public Camera cam;
    float xRotation = 0f;
    float yRotation = 0f;
    bool isGrounded = false;
    public bool stunned = false;
    public Transform enemy;
    public AudioClip[] stepAudioClip;
    private AudioSource audioSource;
    [SerializeField] private bool isOnGrass = false;

    public float interactDistance = 1f;

    void Start()
    {
        // Lock cursor to center of screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (stunned)
        {
            return;
        }
        Move();
    }

    void Update()
    {
        if (stunned)
        {
            audioSource.Stop();
            return;
        }
        CameraUpdate();
       
        // Unlock cursor when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Lock cursor back when clicking on screen
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Shooting mechanics (you can extend this part for shooting logic)
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            isGrounded = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Move()
    {
        // Player movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        rb.MovePosition(rb.position + moveDirection * movementSpeed * Time.deltaTime);

        PlayFootStepSound(moveDirection);
    }

    public void PlayFootStepSound(Vector3 moveDirection)
    {
        if (isGrounded == false)
        {
            audioSource.Stop();
            return;
        }
        var hit = Physics.RaycastAll(transform.position, Vector3.down, 1.1f);
        bool onGrass = false;
        foreach (var h in hit)
        {
            if (h.collider.name == "Plane")
            {
                onGrass = true;
                break;
            }
        }
        if(isOnGrass != onGrass)
        {
            audioSource.clip = stepAudioClip[onGrass ? 0 : 1];
            isOnGrass = onGrass;
            audioSource.Stop();
        }

        if (moveDirection.magnitude < 0.5f)
        {
            audioSource.Stop();

        }
        else if (audioSource.isPlaying == false)
        {
            audioSource.Play();
        }
    }

    void CameraUpdate()
    {
        // Player rotation based on mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        yRotation += mouseX;

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void Shoot()
    {
        var hit = Physics.RaycastAll(cam.transform.position, cam.transform.forward, interactDistance);
        foreach (var h in hit)
        {
            if(h.collider.tag == "Respawn")
            {
                dGameManager.instance.CollectStatue(h.collider.gameObject);
            }
            else if(h.collider.tag == "Finish")
            {
                dGameManager.instance.Finish();
            }
        }
    }

    public void LookAt(Vector3 position)
    {
        cam.transform.LookAt(position);
        xRotation = cam.transform.rotation.eulerAngles.x;
        xRotation = Mathf.Repeat(xRotation + 90, 180f) - 90f;
        yRotation = cam.transform.rotation.eulerAngles.y;
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * interactDistance);
    }
}