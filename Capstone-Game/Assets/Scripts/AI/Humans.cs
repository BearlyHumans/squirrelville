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


    public int takeFoodAmmount;

    public Animator anim;

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


    float pickUpTimer;

    // -----catching variables ------//
    int catchChoice;
    bool stillFood = false;
    bool hasFood = false;
    string aniChoice = "Stun";

    //used to check if player has been caught recently 
    bool hasCaughtRecently = false;

    [Tooltip("Time the Player is frozen")]
    [SerializeField]
    public float unFreezeTime = 5.0f;

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

                anim.Play("Walk");
                break;
            }
            case HumanStates.Chase:
            {
                ChaseState();
                anim.Play("Running");
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
        //UpdAnimator();
    }

    /// functionaility for catching behaviour 
    private void CatchingState()
    {  
        anim.Play(aniChoice);
        if(!hasCaughtRecently)
        {

            hasCaughtRecently = true;  
            // takes x ammount of food from the player when caught
            takeFood(takeFoodAmmount);

            stillFood = checkForFood();

            squrrielTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Invoke("unFreezePlayer", unFreezeTime);
            catchChoice = Random.Range(0,2);
        }

        // checks to see if there is still food to pick up and if not then chose what to do with it
        if(!stillFood)
        {
            // if food was picked up
            if(hasFood)
            {
                // option 1 = go to bin
                if(catchChoice == 0)
                {
                    Bin bin = homePoint.closestBin(transform.position);


                    if(bin.radius <= Vector3.Distance(bin.transform.position, transform.position))
                    {
                        aniChoice = "Walk";
                        navMesh.SetDestination(bin.transform.position);
                    }
                    // put food in bin
                    else
                    {
                        aniChoice = "Drop";
                        navMesh.velocity = Vector3.zero;
                        Invoke("canCatchAgain", 5);
                        Invoke("returnToPath", 1.8f);
                    }
                }
                // option 2 - eat the food
                else
                {
                    aniChoice = "Eating";
                    Invoke("canCatchAgain", 5);
                    Invoke("returnToPath", 3);
                }
            }
            else
            {
                
                Invoke("canCatchAgain", 5);
                Invoke("returnToPath", 1.5f);
            }
        }
        else
        {
            stillFood = checkForFood();
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
                if (distance < 1.0f)
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
            // if friendly and see player then enter friendly state
            if(npcCurrentMode == NpcModes.Friendly)
            {
                currentState = HumanStates.Friendly;
            }
            // if aggressive and see playing then chase
            else if(npcCurrentMode == NpcModes.Aggressive)
            {
                // only if havnt caught recently 
                if(!hasCaughtRecently)
                {
                    currentState = HumanStates.Chase;
                }
            }

            // if passive then keep walking around
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
                waitTimer = 2f;
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

    // checks for any food taken from player and attempts to pick it all up
    private bool checkForFood()
    {
        float radius = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        
        
        if(hitColliders.Length != 0)
        {
            
            float bestDistance = 9999.0f;
            Collider bestCollider = null;

            foreach (Collider hitCollider in hitColliders)
            {
                
                float distToFood = Vector3.Distance(hitCollider.transform.position, transform.position);
                
                if (distToFood < bestDistance)
                {
                   bestDistance = distToFood;
                   bestCollider = hitCollider;
                }
            }
            
            navMesh.SetDestination(bestCollider.transform.position);

            if(bestDistance < 1f)
            {
                navMesh.velocity = Vector3.zero;
                
                StartCoroutine(pickUpFood(bestCollider));

                pickUpTimer += Time.deltaTime;
                Invoke("pickUp", 0.8f);
            
            } 
            hasFood = true;
            return true;
        }
        return false;
    }
    
    IEnumerator pickUpFood(Collider food)
    {
        
        yield return new WaitForSeconds(2.8f);
        food.enabled = false;
    }

    // when caught forces the player to spit up an ammont of food
    private void takeFood(int takeFoodAmmount)
    {
        int i = 0; 
        while(i < takeFoodAmmount)
        {
            foodGraber.ThrowFood(); 
            i++;
        }
    }

    void pickUp()
    {
        pickUpTimer = 0f;
        aniChoice = "Pick Up";
    }

    void returnToPath()
    {
        hasFood = false;
    
        currentState = HumanStates.PathFollowing;
    }

    /// turns to face player
    public void facePlayer()
    {

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    /// checks to see if any food is within its range after catching the player
    
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

    void canCatchAgain()
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

    void resetAnimation()
    {
        anim.enabled = false;
        anim.enabled = true;
    }

    // Visualize area of points
    void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}