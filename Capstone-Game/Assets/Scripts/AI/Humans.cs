using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//The different states each NPC can 
public enum HumanStates
{
    PathFollowing,
    Chase,
    Catch,
    Friendly,
}

//Modes the Npcs can be set to
// Friendly = give player food on sight
// Aggressive = Will chase player on sight
// Passive = doesnt notice the player
public enum NpcModes
{
    Friendly,
    Aggressive,
    Passive,
}

public class Humans : MonoBehaviour
{
    
    private HumanStates currentState;
    private NpcModes npcMode;

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

    [Tooltip("bin")]
    public Bin bin;

    //---sphere cast ---//
    private Vector3 origin;
    private Vector3 sDirection;
    public LayerMask layerMask;

    // --- good object for friendly humans to give---- /
    public GameObject foodToGive;

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
    [Tooltip("Select the behavior for the NPC")]
    [SerializeField]
    public NpcModes npcCurrentMode;
    
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
    GameObject squrrielTarget;

    /// set the nav mesh agent for humans to walk on as well as set target (player) to chase.
    public void Start() 
    {
        
        foodController = GameObject.FindWithTag("Player");
        foodGraber = foodController.GetComponent<SquirrelFoodGrabber>();

        navMesh = this.GetComponent<NavMeshAgent>();

        chaseTimer = chaseTime;
        

        target = GameObject.FindWithTag("Player").transform;
        squrrielTarget = GameObject.FindWithTag("Player");

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

    /// handles the main swaping of states for each person. Runs specific behaviour while in a certain state.
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

    /// functionaility for catching behaviour 
    private void CatchingState()
    {
        // To start timer for ability to catch again
        hasCaughtRecently = true;

        //currently caught
        caught = true;

        facePlayer();
        foodGraber.ThrowFood();  

        //freeze sqiurriel - to change later
        squrrielTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        
        // checks if player drops food to "pick up" delete - to change to pick and and throw out
        checkForFood();

        if(!caught)
        {
            print("play animations");

            Invoke("unFreezePlayer", unFreezeTime);  
            currentState = HumanStates.PathFollowing;
            
        }
    }
    ///functionaility for chasing behaviour. Added checks to see if the npc leaves their boundry area or chases for 'x' ammount of time 
    private void ChaseState()
    {  
        /// runs a check to see if the human is still within boundary
        if(checkBoundry() == true)
        {
            // runs a check to test if human can see the player 
            SeePlayer();
            // if can see player then target and move towards player
            navMesh.SetDestination(target.position);
            
            // if within boundary then chase while timer is above -
            if (chaseTimer > 0f)
            {   
                
                Debug.DrawLine(transform.position, target.position);

                chaseTimer -= Time.deltaTime;
                // checks to see if human is within range to "catch" player
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
        /// if the human leaves the set area they will return to following their path
        else
        {
            SetDest();
            currentState = HumanStates.PathFollowing;
        }
                
    }

    /// starts a timer to watch player food and offers 1 piece of food. ALso has internal timer to stop human from giving player too much food
    private void FriendlyState()
    {
        watchedFor += Time.deltaTime;
        facePlayer();
        navMesh.SetDestination(transform.position);
        
        if (!givenfood)
        {
           Instantiate(foodToGive, new Vector3(transform.position.x -1.0f, transform.position.y , transform.position.z ), Quaternion.identity); 
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
    ///runs checks to find player, while cant see playing iterate through list of path points and walk between them
    private void PathFollowingState()
    {
        bool canSee = SeePlayer();
        
        if(canSee)
        {
            if(npcCurrentMode == NpcModes.Friendly)
            {
                currentState = HumanStates.Friendly;
            }
            else if(npcCurrentMode == NpcModes.Aggressive)
            {
                currentState = HumanStates.Chase;
            }
            else
            {
                currentState = HumanStates.PathFollowing;
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
    /// turns to face player
    public void facePlayer()
    {

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    /// checks to see if any food is within its range after catching the player
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
    /// runs a ray cast to check if the player is within a LOS.  
    bool SeePlayer()
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDir = target.position - transform.position;

        // view angle
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
    /// checks if the humans locations is outside of a set boundary
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
    /// unfreezes player when run
    void unFreezePlayer()
    {
        squrrielTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }
    ///Start 
    void canCatchPlayer()
    {
        hasCaughtRecently = false;
    }
    /// used within path following to move human to current path point
    private void SetDest()
    {
        if (pathPoints != null)
        {
            Vector3 targetVector = pathPoints[currentPathPt].transform.position;
            navMesh.SetDestination(targetVector);
            
            walking = true;
        }
    }
    /// Used within path following to set a new path point to walk to 
    private void ChangePathPt()
    {
        // if turn around chance is true. 
        if (UnityEngine.Random.Range(0f, 1f) <= turnAroundChance)
        {
            // selects the path point they just came from
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

    // Visualize area of points
    void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}