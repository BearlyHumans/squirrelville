using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Humans : MonoBehaviour

{
    //------------States------------------//
    private HumanStates currentState;

    //----------Path Following -----------//
    [Tooltip("Do you want to npc to pause on each point?")]
    [SerializeField]
    bool walkingPause;

    [Tooltip("added change for NPC to turn around")]
    [SerializeField]
    float turnAroundChance = 0.2f;

    [Tooltip("Add ammount of path points for human to walk to")]
    [SerializeField]
    List<WayPoints> pathPoints;

    NavMeshAgent navMesh;
    int currentPathPt;
    bool walking;
    bool waiting;
    bool walkForward;
    float waitTimer;




    //--------------Chase----------------//
    [Tooltip("Detection Radius")]
    [SerializeField]
    public float range = 2f;

    // ### would a distance be better then timer
    [Tooltip("How long does the human chase the player")]
    [SerializeField]
    float chaseTime = 0f;

    float chaseTimer = 15f;
    Transform target;

    

    public void Start() 
    {
        navMesh = this.GetComponent<NavMeshAgent>();
        // ### singleton - ask jake about game controller.
        target = GameObject.FindWithTag("Player").transform;
        
        // why is this broken?
        float chaseTimer = chaseTime;
        

        if(navMesh == null)
        {
            Debug.Log("No nav mesh");
        }
        else
        {
            if (pathPoints != null && pathPoints.Count >= 2)
            {
                currentPathPt = 0;
                SetDest();
            }
            else
            {
                Debug.Log("Add more points to walk between");
            }
        }


    }

    public void Update() 
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
        // Change states into classes.

        // remember what was doing last - Stack

        // render human current task / point



        switch(currentState)
        {
            case HumanStates.WalkingAround:
            {
                if(walking && navMesh.remainingDistance <= 1.0f)
                {
                    walking = false;
                    
                    if(walkingPause)
                    {
                        waiting = true;
                        waitTimer = 0f;
                    }
                    else
                    {
                        ChangePathPt();
                        SetDest();
                    }
                }

                if(waiting)
                {
                    waitTimer += Time.deltaTime;

                    if(waitTimer >= pathPoints[currentPathPt].waitForThisLong)
                    {
                        waiting = false;

                        ChangePathPt();
                        SetDest();
                    }
                }

                FoVCheck();
                break;
            }

            case HumanStates.Chase:
            {
                
                navMesh.SetDestination(target.position);
                
                if (chaseTimer > 0f)
                {   
                    chaseTimer -= Time.deltaTime;
                    Debug.Log(chaseTimer);
                    
                }
                else
                {
                    chaseTimer = chaseTime;
                    
                    Debug.Log("State Swap: Walking around");
                    SetDest();
                    currentState = HumanStates.WalkingAround;

                }
   
    
                break;
            }

            case HumanStates.Catch:
            {
                
                break;
            }
        }
    }

    private void FoVCheck()
    {
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDir = target.position - transform.position;
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
 
        if ((angleToPlayer >= -45 && angleToPlayer <= 45) && (distance <= range))// 180° FOV
        {
            Debug.DrawLine(target.position, transform.position, Color.green);
            Debug.Log("Chasing player");
            currentState = HumanStates.Chase;
        }   
    } 

    private void SetDest()
    {
        if (pathPoints != null)
        {
            Vector3 targetVector = pathPoints[currentPathPt].transform.position;
            navMesh.SetDestination(targetVector);
            walking = true;
        }
    }

    private void ChangePathPt()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= turnAroundChance)
        {
            walkForward = !walkForward;
        }

        if (walkForward)
        {
            currentPathPt = (currentPathPt + 1) % pathPoints.Count;
            
        }
        else 
        {
            currentPathPt--;
            if (currentPathPt < 0)
            {
                currentPathPt = pathPoints.Count - 1;
                
            }
        }
    }

    void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private enum HumanStates
    {
        WalkingAround,
        Chase,
        Catch
    }

}

