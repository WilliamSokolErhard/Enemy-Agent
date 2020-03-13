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
    Animator animator;

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 9.8f;

    public List<Transform> enemies = new List<Transform>();
    //private Transform enemyTarget;
    public Transform waypointsParent;
    private int waypointTarget = 0;
    private Transform waypointTargetTransform;

    public float rotationSpeed;

    public bool loop = true;
    private bool increasing = true;

    private Vector3 moveDirection = Vector3.zero;

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = new Color(0,1,0,0.2f);
        foreach(Transform waypoint in waypointsParent)
        {
            Gizmos.DrawSphere(waypoint.position, 0.25f);
        }        
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        waypointTargetTransform = waypointsParent.GetChild(waypointTarget);
    }

    private Transform ClosestEnemy()
    {
        return enemies[0]; //TODO
    }

    float timer = -1.8f;
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
                        waypointTarget = waypointTarget + 1 >= waypointsParent.childCount ? 0 : waypointTarget + 1;
                    }
                    else
                    {
                        if ((increasing && waypointTarget + 1 >= waypointsParent.childCount) || (!increasing && waypointTarget - 1 <= 0))
                        {
                            increasing = !increasing;
                            animator.SetTrigger("AboutFace");
                            timer = Time.time;
                            print("AboutFace");
                            //transform.rotation *= Quaternion.Euler(0, 35, 0);
                        }

                        waypointTarget += increasing ? 1 : -1;
                    }
                    waypointTargetTransform = waypointsParent.GetChild(waypointTarget);
                }

                target = new Vector3(waypointTargetTransform.position.x, transform.position.y, waypointTargetTransform.position.z) - transform.position;
                break;
            case AgentState.Attack:
                animator.SetTrigger("EngageEnemy");
                if (Vector3.Distance(transform.position, ClosestEnemy().position)<10f) { 
                    target = new Vector3(ClosestEnemy().position.x, transform.position.y, ClosestEnemy().position.z) - transform.position;
                    animator.SetTrigger("AttackEnemy"); 
                }
                break;
            case AgentState.Cover:

                break;            
        }

        if (timer + 2f <= Time.time)
        {
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
        }

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
