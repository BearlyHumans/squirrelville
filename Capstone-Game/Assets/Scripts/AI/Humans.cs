using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Player;


//The different states each NPC can 
public enum HumanStates
{
    PathFollowing,
    Chase,
    Catch,
    Friendly,
    HandleFood,
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
    [Tooltip("Animation events")]
    [SerializeField]
    private List<ParameterChangeEvent> animationEvents = new List<ParameterChangeEvent>();
    
    private HumanStates currentState;
    private NpcModes npcMode;

    //---------Food Graber Script---------//
    private SquirrelFoodGrabber foodGraber;
    private GameObject foodController;

    private SquirrelController squirrelController;
    private GameObject sController;

    NavMeshAgent human;

    public Stun stunScript;

    public ParticleSystem exclaim;

    [Tooltip("Select the behavior for the NPC")]
    [SerializeField]
    public NpcModes npcCurrentMode;

    [SerializeField]
    private PathFollowingVarible pathFollowingVariables;

    [SerializeField]
    private FriendlyVarible friendlyVariables;

    [SerializeField]
    private CatchVarible catchVariables;

    [SerializeField]
    private ChaseVarible chaseVariables;

    // layer mask for what layer humans check spat out food on
    private LayerMask layerMask;

    // path following variables
    float distance;
    int currentPathPt;
    bool walking;
    bool waiting;
    bool walkForward;
    float waitTimer;

    // animator for controlling human animations 
    public Animator anim;

    [Tooltip("Angle in which the NPC can see player")]
    [SerializeField]
    public float detectionAngle = 70;
    [Tooltip("Detection Radius")]
    [SerializeField]
    public float range = 2f;
    
    // bool check for if the friendly human has given player food
    bool givenfood = false;

    //Timers how long friendly humans stand to watch player for
    float watchedFor = 0.0f;

    // increment timer used to check if timer is above set time for humans to give food
    float timeToFood = 0.0f;

    // used to keep tracking of how long human has chased player
    float chaseTimer;

    // bool check to make sure a check for food has been run after catching player
    bool checkBeenRun = false;

    // timer for stopping humans getting stuck walking to food if out of reach
    float walkToFoodTimer = 0f;
    float howLongCanWalkToFood = 10f;

    
    // used to determine what action "handleFood" does
    int catchChoice;

    // boolean check for if spat out food is still in area
    bool stillFood = false;
    // bool check for if human picked up any food after catching player
    bool pickedUpFood = false;

    // used for only playing exclaim once
    bool havntSpotted = false;

    //used to check if player has been caught recently 
    bool hasCaughtRecently = false;

    [Tooltip("speed of walking player")]
    [SerializeField]
    public float humanWalkSpeed = 2.0f;

    [Tooltip("speed of running player")]
    [SerializeField]
    public float humanRunSpeed = 5.0f;

    Transform target;
    GameObject squrrielTarget;
    GameObject playerController;

    /// set the nav mesh agent for humans to walk on as well as set target (player) to chase.
    public void Start() 
    {
        playerController = GameObject.FindWithTag("Player");
        
        foodController = playerController;
        foodGraber = foodController.GetComponent<SquirrelFoodGrabber>();

        sController = playerController;
        squirrelController = sController.GetComponent<SquirrelController>();

        human = this.GetComponent<NavMeshAgent>();

        chaseTimer = chaseVariables.chaseTime;

        layerMask = LayerMask.GetMask("EatenFood");

        target = playerController.transform;

        squrrielTarget = playerController;

        currentState = HumanStates.PathFollowing;
        if(human == null)
        {
            Debug.Log("No nav mesh");
        }
        else
        {
            if (pathFollowingVariables.pathPoints != null && pathFollowingVariables.pathPoints.Count >= 2)
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
            case HumanStates.HandleFood:
            {
                HandleFood();
                break;
            }
        }
    }

    /// functionaility for catching behaviour 
    private void CatchingState()
    {  
        human.speed = humanWalkSpeed;
        if(!hasCaughtRecently)
        {
            checkBeenRun = false;
            hasCaughtRecently = true;  

            // animation stunning (not moving)
            CallAnimationEvents(AnimTriggers.stunning);

            catchChoice = UnityEngine.Random.Range(0, 2);

            StartCoroutine(stunPlayer());
        }
        // checks to see if there is still food to pick up and if not then chose what to do with it
        if(checkBeenRun)
        {
            //stillFood = checkForFood();
            if(!stillFood)
            {
                if(pickedUpFood)
                {
                    currentState = HumanStates.HandleFood;
                }
                else
                {
                    Invoke("canCatchPlayerAgain", catchVariables.catchResetTimer);
                    returnToPath();
                }
            }
            stillFood = checkForFood();
        }
    }

    private void HandleFood()
    {
        // option 1 = go to bin
        if(catchChoice == 0)
        {
            Bin bin = pathFollowingVariables.homePoint.closestBin(transform.position);


            if(bin.radius <= Vector3.Distance(bin.transform.position, transform.position))
            {
                // animation walk (not moving)
                CallAnimationEvents(AnimTriggers.walking);
                human.SetDestination(bin.transform.position);
            }
            // put food in bin
            else
            {
                CallAnimationEvents(AnimTriggers.dropping);
                        
                human.velocity = Vector3.zero;
                Invoke("canCatchPlayerAgain", catchVariables.catchResetTimer);
                Invoke("returnToPath", 3f);
                //returnToPath();
            }
        }    
        // option 2 - eat the food
        else
        {    
            CallAnimationEvents(AnimTriggers.eating);
            Invoke("canCatchPlayerAgain", catchVariables.catchResetTimer);
            Invoke("returnToPath", 3f);
            
        }
        
    }

    ///functionaility for chasing behaviour. Added checks to see if the npc leaves their boundry area or chases for 'x' ammount of time 
    private void ChaseState()
    {  
        human.speed = humanRunSpeed;
        CallAnimationEvents(AnimTriggers.running);
        
        if(!havntSpotted)
        {
            havntSpotted = true;
            exclaim.Play();
        }

        /// runs a check to see if the human is still within boundary
        if(checkBoundry() == true)
        {
            // runs a check to test if human can see the player 
            SeePlayer();
            // if can see player then target and move towards player
            human.SetDestination(target.position);
            
            // if within boundary then chase while timer is above -
            if (chaseTimer > 0f)
            {   
                
                Debug.DrawLine(transform.position, target.position);

                chaseTimer -= Time.deltaTime;
                // checks to see if human is within range to "catch" player
                if (distance < 1.5f)
                {
                    havntSpotted = false;
                    currentState = HumanStates.Catch;
                }
                     
            }
            else
            {
                chaseTimer = chaseVariables.chaseTime;
                        
                SetDest();
                havntSpotted = false;
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
        anim.SetInteger("HumanMove", 0);
        watchedFor += Time.deltaTime;
        facePlayer();
        human.SetDestination(transform.position);
        
        if (!givenfood)
        {
           Instantiate(friendlyVariables.foodToGive, new Vector3(transform.position.x -1.0f, transform.position.y , transform.position.z ), Quaternion.identity); 
           givenfood = true;
           timeToFood = 0.0f;
        }

        if(watchedFor > friendlyVariables.watchTimer)
        {
            currentState = HumanStates.PathFollowing;
            watchedFor = 0.0f;
        }

        if(timeToFood > friendlyVariables.foodTimer)
        {
            givenfood = false;
        }
    }
    ///runs checks to find player, while cant see playing iterate through list of path points and walk between them
    private void PathFollowingState()
    {
        human.speed = humanWalkSpeed;
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
        if(walking && human.remainingDistance <= 1.0f)
        {
            walking = false;
                    
            if(pathFollowingVariables.walkingPause)
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
            CallAnimationEvents(AnimTriggers.idle);
            waitTimer += Time.deltaTime;
            if(waitTimer >= pathFollowingVariables.pathPoints[currentPathPt].waitForThisLong)
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
            Collider bestCollider = null;
            float bestDistance = Mathf.Infinity;
            
            foreach (Collider hitCollider in hitColliders)
            {
                Vector3 distToFood = hitCollider.transform.position - transform.position;
                float distSq = distToFood.sqrMagnitude;

                if (distSq < bestDistance)
                {
                   bestDistance = distSq;
                   bestCollider = hitCollider;
                }
            }
            walkToFoodTimer += Time.deltaTime;
            CallAnimationEvents(AnimTriggers.walking);
            human.SetDestination(bestCollider.transform.position);
        
            if(walkToFoodTimer > howLongCanWalkToFood)
            {
                bestCollider.gameObject.layer = LayerMask.NameToLayer("Food");
                
                walkToFoodTimer = 0f;
            }
          
            if(bestDistance < 1f)
            {
                human.velocity = Vector3.zero;
                
                CallAnimationEvents(AnimTriggers.pickup);
                StartCoroutine(pickUpFood(bestCollider));

                walkToFoodTimer = 0f;
            
            } 
            pickedUpFood = true;
            return true;
        }
        return false;
    }

    IEnumerator stunPlayer()
    {
        yield return new WaitForSeconds(1.1f);
        
        stunScript.stompEffect(squirrelController, foodGraber, catchVariables.takeFoodAmmount);

        yield return new WaitForSeconds(1.2f);
        stillFood = checkForFood();
        
        //yield return new WaitForSeconds(1.22f);
        checkBeenRun = true;
    }
    
    IEnumerator pickUpFood(Collider food)
    {
        yield return new WaitForSeconds(2.8f);
    
        food.gameObject.SetActive(false);
        food.GetComponent<Food>().respawn();
 
    }

    void canCatchPlayerAgain()
    {
        hasCaughtRecently = false;
    }


    void returnToPath()
    {
        CallAnimationEvents(AnimTriggers.walking);
        pickedUpFood = false;
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
        float dist = Vector3.Distance(pathFollowingVariables.homePoint.transform.position, transform.position);
        if (dist > pathFollowingVariables.homePoint.boundary)
        {
            return false;
        }
        else
        {
            return true;
        } 
    }
     
    /// used within path following to move human to current path point
    private void SetDest()
    {
        if (pathFollowingVariables.pathPoints != null)
        {
            CallAnimationEvents(AnimTriggers.walking);

            Vector3 targetVector = pathFollowingVariables.pathPoints[currentPathPt].transform.position;
            human.SetDestination(targetVector);
            
            walking = true;
        }
    }
    /// Used within path following to set a new path point to walk to 
    private void ChangePathPt()
    {
        // if turn around chance is true. 
        if (UnityEngine.Random.Range(0f, 1f) <= pathFollowingVariables.turnAroundChance)
        {
            // selects the path point they just came from
            walkForward = !walkForward;
        }
        if (walkForward)
        {
            currentPathPt = (currentPathPt + 1) % pathFollowingVariables.pathPoints.Count;
            
        }
        else 
        {
            currentPathPt--;
            if (currentPathPt < 0)
            {
                currentPathPt = pathFollowingVariables.pathPoints.Count - 1;
                
            }
        }
    }

    public void CallAnimationEvents(AnimTriggers trigger)
    {
        foreach (ParameterChangeEvent PCE in animationEvents)
        {
            if (PCE.trigger == trigger)
                ChangeParameter(PCE);
        }
    }

    private void ChangeParameter(ParameterChangeEvent PCE)
    {
        anim.SetInteger(PCE.animsParameter.paramName, (int)PCE.animsParameter.setToValue);
    }
    // Visualize area of points
    void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public enum AnimTriggers
    {
        idle,
        walking,
        running,
        stunning,
        pickup,
        eating,
        dropping

    }

    [System.Serializable]
    private class ParameterChangeEvent
    {
        public AnimTriggers trigger;
        public AnimationParameter animsParameter;
    }

    [System.Serializable]
    private class AnimationParameter
    {
        [Tooltip("Set this to the name of the parameter you want to change when the trigger occurs.")]
        public string paramName;
        [Tooltip("Use 0/1 for false/true.")]
        public float setToValue;

    }

    [System.Serializable]
    private class PathFollowingVarible
    {
        
        public bool walkingPause;
        
        public float turnAroundChance = 0.2f;
        
        public List<WayPoints> pathPoints;

        public HomePoint homePoint;
    }

    [System.Serializable]
    private class FriendlyVarible
    {
        public GameObject foodToGive;

        public float foodTimer;

        public float watchTimer = 5;
    }

    [System.Serializable]
    private class ChaseVarible
    {
        public float chaseTime;
    }

    [System.Serializable]
    private class CatchVarible
    {
        public float catchResetTimer;
        public int takeFoodAmmount;
    }
}