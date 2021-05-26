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

    public HomePoint homePoint;

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
    float chaseTime = 10f;

    float chaseTimer;
    Transform target;

    public void Start() 
    {
        navMesh = this.GetComponent<NavMeshAgent>();
        chaseTimer = chaseTime;
        // singleton - ask jake about game controller.
        target = GameObject.FindWithTag("Player").transform;

        currentState = HumanStates.PathFollowing;
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
                SetDest();
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
    
        // -----States------
        switch(currentState)
        {
            case HumanStates.PathFollowing:
            {
                PathFollowingBehaviour();
                
                break;
            }

            case HumanStates.Chase:
            {
                ChaseBehaviour();
                
                break;
            }

            case HumanStates.Friendly:
            {
                FriendlyBehaviour();
                break;
            }

            case HumanStates.Catch:
            {
                
                break;
            }
        }
    }

    private void ChaseBehaviour()
    {
        
                
        if(checkBoundry() == true)
        {
            SeePlayer();
            navMesh.SetDestination(target.position);

            if (chaseTimer > 0f)
            {   
                chaseTimer -= Time.deltaTime;
                    
            }
            else
            {
                chaseTimer = chaseTime;
                        
                Debug.Log("State Swap: Walking around");
                SetDest();
                currentState = HumanStates.PathFollowing;
            }
        }
        else
        {
            SetDest();
            currentState = HumanStates.PathFollowing;
        }
                
    }

    private void FriendlyBehaviour()
    {
        Debug.Log("Feed friendly squirrel");
    }

    private void PathFollowingBehaviour()
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

        SeePlayer();
    }

    private void SeePlayer()
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDir = target.position - transform.position;
        float angle = 45f;
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));

        RaycastHit hit;
 
        if ((angleToPlayer >= -angle && angleToPlayer <= angle) && (distance <= range))
        {
            if(Physics.Linecast (transform.position, target.transform.position, out hit))
            {
                if(hit.transform.tag == "Player")
                {
                    if(gameObject.tag == "Aggressive")
                    {
                        chaseTimer = chaseTime;
                        Debug.DrawLine(target.position, transform.position, Color.red);
                        currentState = HumanStates.Chase;
                    }
                    else if(gameObject.tag == "Friendly")
                    {
                        currentState = HumanStates.Friendly;
                    }
                    
                }
            }
        }
    } 

    bool checkBoundry()
    {
        float dist = Vector3.Distance(homePoint.transform.position, transform.position);
        if (dist > homePoint.boundary)
        {
            return false;
        }
        else
        {
             return true;
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
        PathFollowing,
        Chase,
        Catch,
        Friendly,
    }

}

