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

    public GameObject projectilePrefab;
    public List<Transform> enemies = new List<Transform>();
    private Transform enemyTarget;
    private Vector3 coverPosition;
    public Transform waypointsParent;
    private int waypointTarget = 0;
    private Transform waypointTargetTransform;

    public float rotationSpeed;

    public bool loop = true;
    private bool increasing = true;

    private Vector3 moveDirection = Vector3.zero;


    public float health = 1f;

    void OnDrawGizmos()
    {
        // Draw a green sphere at the transform's position
        Gizmos.color = new Color(0,1,0,1f);
        if (waypointsParent != null)
        {
            for(int i = 0; i < waypointsParent.childCount; i++)
            {
                if (i > 0)
                {
                    Gizmos.DrawLine(waypointsParent.GetChild(i-1).position, waypointsParent.GetChild(i).position);
                }
                Gizmos.DrawSphere(waypointsParent.GetChild(i).position, 0.125f);
            }
            if (loop)
            {
                Gizmos.DrawLine(waypointsParent.GetChild(0).position, waypointsParent.GetChild(waypointsParent.childCount-1).position);
            }
        }
    }

    void Start()
    {
        coverPosition = transform.position;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (waypointsParent != null)
        {
            waypointTargetTransform = waypointsParent.GetChild(waypointTarget);
        }
        if (enemies.Count>0)
        {
            enemyTarget = enemies[0];
        }
    }

    private Transform ClosestEnemy()
    {
        return enemyTarget;
    }

    GameObject go;
    IEnumerator throwBall()
    {
        yield return new WaitForSeconds(0.75f);
        go = Instantiate(projectilePrefab);
        go.GetComponent<AcidBall>().aIBotController = this;
        go.transform.position = transform.position + transform.up * 1.5f + transform.right / 2;
        if(ClosestEnemy()!=null)
            go.GetComponent<Rigidbody>().AddForce((ClosestEnemy().position - transform.position + transform.forward + transform.up - transform.right / 6) * 2, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        go.GetComponent<SphereCollider>().enabled = true;
    }
    public bool HitPlayer(Transform player)
    {
        if (player.GetComponent<AIBotController>() != null)
        {            
            player.GetComponent<AIBotController>().ShotTaken(1f);
            return true;
        }
        return false;
        //TODO Damage Player
    }

    public void ShotTaken(float damage)
    {
        if (health >= 0f)
        {
            health -= damage;
            if (health <= 0f && animator != null)
            {
                if (Scoreboard.instance != null)
                    Scoreboard.instance.deathCounted();
                animator.SetTrigger("Death");
                StartCoroutine(Death());
            }
        }
    }
    IEnumerator Death()
    {        
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }

    float timer = -1.8f;
    void Update()
    {
        Vector3 target = Vector3.zero;
        switch (agentState)
        {
            case AgentState.Idle:
                animator.SetTrigger("Idle");
                break;
            case AgentState.Patrol:
                if (waypointsParent == null)
                {
                    agentState = AgentState.Idle;
                    break;
                }
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
                        }

                        waypointTarget += increasing ? 1 : -1;
                    }
                    waypointTargetTransform = waypointsParent.GetChild(waypointTarget);                    
                }
                target = new Vector3(waypointTargetTransform.position.x, transform.position.y, waypointTargetTransform.position.z) - transform.position;

                foreach (Transform enemy in enemies)
                {
                    if (enemy == null)
                    {
                        enemies.Remove(enemy);
                    }else if (Vector3.Angle(enemy.position-transform.position,transform.forward)<90f)
                    {
                        RaycastHit RHE;                        
                        Ray view = new Ray(transform.position + Vector3.up * 1.7F, enemy.position - transform.position+Vector3.down*0.25f);

                        Debug.DrawRay(view.origin, view.direction * 15f, Color.yellow);
                        if (Physics.Raycast(view, out RHE, 15f) && RHE.collider.transform==enemy )
                        {
                            Debug.DrawRay(view.origin,view.direction*15f, Color.red);
                            enemyTarget = enemy;
                            agentState = AgentState.Attack;
                        }
                    }
                }
                break;
            case AgentState.Attack:
                //if (!animator.GetCurrentAnimatorStateInfo(0).IsName(""))
                animator.SetTrigger("EngageEnemy");
                if (ClosestEnemy() != null &&  Vector3.Distance(transform.position, ClosestEnemy().position)<10f) { 
                    target = new Vector3(ClosestEnemy().position.x, transform.position.y, ClosestEnemy().position.z) - transform.position;
                    if (timer + 3f <= Time.time)
                    {
                        timer = Time.time;
                        animator.SetTrigger("AttackEnemy");
                        StartCoroutine(throwBall());
                    }
                }else if (ClosestEnemy() == null)
                {
                    agentState = AgentState.Patrol;
                }
                break;
            case AgentState.Cover:                
                if (timer + 3f <= Time.time)
                {
                    animator.SetTrigger("EngageEnemy");
                    timer = Time.time;
                    RaycastHit RH;
                    Vector3 rayHeight = transform.position + Vector3.up * 1.7f;
                    if (Physics.Raycast((rayHeight - transform.right * 2), enemyTarget.position - (rayHeight - transform.right*2), out RH, 2f))
                    {
                        target = - transform.right*2;
                        Debug.DrawRay(rayHeight, target, Color.black);
                    }
                    else if (Physics.Raycast((rayHeight + transform.right * 2), enemyTarget.position - (rayHeight + transform.right * 2), out RH, 2f))
                    {
                        target = transform.right*2;
                        Debug.DrawRay(rayHeight, target, Color.black);
                    }
                    else //(!Physics.Raycast(rayHeight - transform.forward * 2, enemyTarget.position - (rayHeight - transform.forward * 2), out RH, 2f))
                    {
                        target = -transform.forward * 2;
                        Debug.DrawRay(rayHeight, target + transform.up, Color.black);
                    }

                    coverPosition = target + transform.position;

                }
                else
                {
                    target = coverPosition- transform.position;
                    if (Vector3.Distance(transform.position, coverPosition) < 0.66f)
                    {
                        animator.SetTrigger("Idle");
                    }
                }

                

                break;            
        }

        if (agentState != AgentState.Patrol || timer + 2f <= Time.time)
        {
            var step = rotationSpeed * Time.deltaTime;
            RaycastHit RH;
            Vector3 rayHeight = transform.position + Vector3.up / 2.5f;
            if (Physics.Raycast(rayHeight, target + transform.up, out RH, 2f) || agentState == AgentState.Cover)
            {
                step *= 4f;

                if(!Physics.Raycast(rayHeight, transform.forward*2+transform.up, 2f))
                {
                    target = transform.forward*2;
                    Debug.DrawRay(rayHeight, target + transform.up, Color.cyan);
                }
                else if (!Physics.Raycast(rayHeight, transform.forward - transform.right + transform.up, out RH, 2f))
                {
                    target = transform.forward - transform.right;
                    Debug.DrawRay(rayHeight, target, Color.cyan);
                }
                else if (!Physics.Raycast(rayHeight, transform.forward + transform.right + transform.up, out RH, 2f))
                {
                    target = transform.forward + transform.right;
                    Debug.DrawRay(rayHeight, target, Color.cyan);
                }
                else if (!Physics.Raycast(rayHeight, -transform.right + transform.up, out RH, 2f))
                {
                    target = -transform.right;
                    Debug.DrawRay(rayHeight, target, Color.cyan);
                }
                else if (!Physics.Raycast(rayHeight, transform.right + transform.up, out RH, 2f))
                {                    
                    target = transform.right;
                    Debug.DrawRay(rayHeight, target, Color.cyan);
                }
            }
            else
            {
                Debug.DrawRay(rayHeight, target, Color.blue);
            }            

            // Rotate our transform a step closer to the target's.
            if(target != Vector3.zero)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target, Vector3.up), step);
            //transform.LookAt(new Vector3(waypointTargetTransform.position.x,transform.position.y,waypointTargetTransform.position.z));
        }

        if (characterController.isGrounded)
        {
            moveDirection = Vector3.zero;
            // We are grounded, so recalculate
            // move direction directly from axes

            //moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            //moveDirection *= speed;

            //if (Input.GetButton("Jump"))
            //{
            //    moveDirection.y = jumpSpeed;
            //}
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }
}
