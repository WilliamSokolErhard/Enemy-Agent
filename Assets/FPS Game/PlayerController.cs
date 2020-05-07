using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    public float speed = 6.0f;
    public float jumpSpeed = 6.0f;
    public float gravity = 20f;

    private Vector3 moveDirection = Vector3.zero;


    public GameObject projectilePrefab;

    private Transform CameraCache;
    public float rotationSpeed = 1.0f;
    private float mouseX=0f,mouseY=0f;

    private bool canThrow = true;

    public AIBotController iBotController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        CameraCache = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && canThrow)
        {
            canThrow = false;
            StartCoroutine(throwBall());
        }

        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;

        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;

        mouseY = Mathf.Clamp(mouseY, -45, 45);

        CameraCache.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        transform.rotation = Quaternion.Euler(0, mouseX, 0);

        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = transform.right* Input.GetAxis("Horizontal")+ Input.GetAxis("Vertical")*transform.forward;
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    GameObject go;
    IEnumerator throwBall()
    {
        //yield return new WaitForSeconds(0.75f);
        go = Instantiate(projectilePrefab);
        go.GetComponent<AcidBall>().aIBotController = iBotController;
        go.transform.position = CameraCache.position;
        go.GetComponent<Rigidbody>().AddForce((CameraCache.forward*8f + CameraCache.up/4f) * 2, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        go.GetComponent<SphereCollider>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        canThrow = true;
    }
}
