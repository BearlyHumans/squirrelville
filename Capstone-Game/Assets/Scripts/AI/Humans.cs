using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//------------States------------------//
public enum HumanStates
{
    PathFollowing,
    Chase,
    Catch,
    Friendly,
}
public class Humans : MonoBehaviour
{
    
    private HumanStates currentState;

    //---------Food Graber Script---------//
    private SquirrelFoodGrabber foodGraber;
    private GameObject foodController;

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
    [Tooltip("adds a home area that acts as boundary")]
    public HomePoint homePoint;


    //---sphere cast ---//
    private Vector3 origin;
    private Vector3 sDirection;
    public LayerMask layerMask;

    // --- burger---- /
    public GameObject burger;
    

    NavMeshAgent navMesh;
    float distance;
    int currentPathPt;
    bool walking;
    bool waiting;
    bool walkForward;
    float waitTimer;

    public Animator anim;

    private void UpdAnimator()
    {
        if (walking)
            anim.SetInteger("HumanMove", 1);
        else
            anim.SetInteger("HumanMove", 0);
    }

    [Tooltip("Angle in which the NPC can see player")]
    [SerializeField]
    public float detectionAngle = 70;

    //--------------Friendly----------------//
    [Tooltip("Is the NPC friendly?")]
    [SerializeField]
    public bool isFriendly;
    
    bool givenfood = false;
    float watchedFor = 0.0f;
    float watchTimer = 5.0f;

    float timeToFood = 0.0f;
    [Tooltip("How long until humans give more food")]
    [SerializeField]
    public float foodTimer = 10.0f;

    //--------------Chase----------------//
    [Tooltip("Detection Radius")]
    [SerializeField]
    public float range = 2f;
    [Tooltip("How long does the human chase the player")]
    [SerializeField]
    float chaseTime = 10f;
    float chaseTimer;

    // used to check if player is currently caught
    bool caught = false;
    //used to check if player has been caught recently 
    bool hasCaughtRecently = false;
    //caught timer

    [Tooltip("Time the Player is frozen")]
    [SerializeField]
    public float unFreezeTime = 5.0f;

    [Tooltip("Time until can chase player again")]
    [SerializeField]
    public float deAggroTimer = 10.0f;

    

    Transform target;
    Rigidbody squrrielBody;

    

    public void Start() 
    {
        foodController = GameObject.FindWithTag("Player");
        foodGraber = foodController.GetComponent<SquirrelFoodGrabber>();

        navMesh = this.GetComponent<NavMeshAgent>();

        chaseTimer = chaseTime;
        // singleton - ask jake about game controller.

        target = GameObject.FindWithTag("Player").transform;
        squrrielBody = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();

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
        
        distance = Vector3.Distance(target.position, transform.position);
        timeToFood += Time.deltaTime;
        
        
        // -----States------
        switch(currentState)
        {
            case HumanStates.PathFollowing:
            {
                PathFollowingState();
                break;
            }
            case HumanStates.Chase:
            {
                ChaseState();
                break;
            }
            case HumanStates.Friendly:
            {
                FriendlyState();
                break;
            }
            case HumanStates.Catch:
            {
                CatchingState();
                break;
            }
        }
        UpdAnimator();
    }

    private void CatchingState()
    {
        Debug.Log("catching");
        // To start timer for ability to catch again
        hasCaughtRecently = true;

        //currently caught
        caught = true;

        facePlayer();
        foodGraber.ThrowFood();  

        //freeze sqiurriel - to change later
        if(caught)
        {
            Debug.Log("caught");
            squrrielBody.constraints = RigidbodyConstraints.FreezePosition;
            
        }
        // checks if player drops food to "pick up" delete - to change to pick and and throw out
    
        checkForFood();
        if(!caught)
        {
            Invoke("unFreezePlayer", unFreezeTime);  
            currentState = HumanStates.PathFollowing;
            
        }
    }

    private void ChaseState()
    {  
        if(checkBoundry() == true)
        {
            SeePlayer();
            navMesh.SetDestination(target.position);
            
            if (chaseTimer > 0f)
            {   
                
                Debug.DrawLine(transform.position, target.position);

                chaseTimer -= Time.deltaTime;
                if(distance < 1.0f)
                {
                    currentState = HumanStates.Catch;
                }
                     
            }
            else
            {
                chaseTimer = chaseTime;
                        
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

    private void FriendlyState()
    {
        watchedFor += Time.deltaTime;
        facePlayer();
        navMesh.SetDestination(transform.position);
        
        if (!givenfood)
        {
           Instantiate(burger, new Vector3(transform.position.x -1.0f, transform.position.y , transform.position.z ), Quaternion.identity); 
           givenfood = true;
           timeToFood = 0.0f;
        }

        if(watchedFor > watchTimer)
        {
            currentState = HumanStates.PathFollowing;
            watchedFor = 0.0f;
        }

        if(timeToFood > foodTimer)
        {
            givenfood = false;
        }
    }

    private void PathFollowingState()
    {
        bool canSee = SeePlayer();
        
        if(canSee)
        {
            if(isFriendly)
            {
                currentState = HumanStates.Friendly;
            }
            else
            {
                currentState = HumanStates.Chase;
            }
        }
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
        
    }

    public void facePlayer()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void checkForFood()
    {
        float radius = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        if(hitColliders.Length == 0)
        {
            caught = false;
            Invoke("canCatchPlayer", deAggroTimer);
        }
        else
        {
           
            foreach (var hitCollider in hitColliders)
            {
                Destroy(hitCollider);
            } 
        }
    }

    bool SeePlayer()
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDir = target.position - transform.position;
    
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
        RaycastHit hit;
 
        if ((angleToPlayer >= -detectionAngle && angleToPlayer <= detectionAngle) && (distance <= range))
        {
            if(Physics.Linecast (transform.position, target.transform.position, out hit))
            {
                if(hit.transform.tag == "Player")
                {
                    if(!hasCaughtRecently)
                        return true;
                }
                
            }
        }
        return false;
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

    void unFreezePlayer()
    {
        squrrielBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void canCatchPlayer()
    {
        hasCaughtRecently = false;
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
}