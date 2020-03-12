using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBotController : MonoBehaviour
{
    public enum AgentState
    {
        Idle,
        Patrol,
        Attack,
        Cover
    }
    public AgentState agentState = AgentState.Patrol; 

    CharacterController characterController;

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 9.8f;

    public Transform waypoints;
    private int waypointTarget = 0;
    private Transform waypointTargetTransform;

    public float rotationSpeed;

    public bool loop = true;
    private bool increasing = true;

    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        waypointTargetTransform = waypoints.GetChild(waypointTarget);
    }

    void Update()
    {
        Vector3 target = Vector3.zero;
        switch (agentState)
        {
            case AgentState.Idle:
                break;
            case AgentState.Patrol:
                if (Vector3.Distance(waypointTargetTransform.position, new Vector3(transform.position.x, waypointTargetTransform.position.y, transform.position.z)) < 0.66f)
                {
                    if (loop)
                    {
                        waypointTarget = waypointTarget + 1 >= waypoints.childCount ? 0 : waypointTarget + 1;
                    }
                    else
                    {
                        if ((increasing && waypointTarget + 1 >= waypoints.childCount) || (!increasing && waypointTarget - 1 <= 0))
                        {
                            increasing = !increasing;
                            transform.rotation *= Quaternion.Euler(0, 35, 0);
                        }

                        waypointTarget += increasing ? 1 : -1;
                    }
                    waypointTargetTransform = waypoints.GetChild(waypointTarget);
                }

                target = new Vector3(waypointTargetTransform.position.x, transform.position.y, waypointTargetTransform.position.z) - transform.position;
                break;
            case AgentState.Attack:

                break;
            case AgentState.Cover:

                break;            
        }


        var step = rotationSpeed * Time.deltaTime;
        RaycastHit RH;
        Vector3 rayHeight = transform.position + Vector3.up / 1.5f;
        if (Physics.Raycast(rayHeight, target, out RH, 1.5f) || Physics.Raycast(transform.position + Vector3.up / 1.5f, transform.forward, 1.5f))
        {
            step *= 4f;
            if (!Physics.Raycast(rayHeight, transform.forward - transform.right, out RH, 3f))
            {
                target = Quaternion.Euler(0, -45, 0) * target;
            }
            else if (!Physics.Raycast(rayHeight, transform.forward + transform.right, out RH, 3f))
            {
                target = Quaternion.Euler(0, 45, 0) * target;
            }
        }
        Debug.DrawRay(rayHeight, target);

        // Rotate our transform a step closer to the target's.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target, Vector3.up), step);
        //transform.LookAt(new Vector3(waypointTargetTransform.position.x,transform.position.y,waypointTargetTransform.position.z));

        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
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
}
