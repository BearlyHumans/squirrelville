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
    Ending
    
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

public enum IdleMode
{
    Standing,
    WSittingTalk,
    MSittingTalk,
    WSitting,
    MSitting,
    Lyingdown
}

public class Humans : MonoBehaviour
{
    [Tooltip("Animation events")]
    [SerializeField]
    private List<ParameterChangeEvent> animationEvents = new List<ParameterChangeEvent>();
    
    private HumanStates currentState;

    NavMeshAgent human;

    public Stun stunScript;

    public ParticleSystem exclaim;

    [Tooltip("Select the behavior for the NPC")]
    [SerializeField]
    public NpcModes npcCurrentMode;

    [Tooltip("Select the behavior for the NPC")]
    [SerializeField]
    public IdleMode npcIdleAnim;

    [SerializeField]
    private PathFollowingVarible pathFollowingVariables;

    [SerializeField]
    private FriendlyVarible friendlyVariables;

    [SerializeField]
    private CatchVarible catchVariables;

    [SerializeField]
    private ChaseVarible chaseVariables;

    [SerializeField]
    private EndingVariable endVariables;



    // layer mask for what layer humans check spat out food on
    private LayerMask layerMask;

    // path following variables
    float distanceToPlayer;
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

    // bool used to check if stun hit player
    [HideInInspector]
    public bool hitPlayerStun = false;

    private bool pickUpFoodOnce = false;

    [Tooltip("speed of walking player")]
    [SerializeField]
    public float humanWalkSpeed = 2.0f;

    [Tooltip("speed of running player")]
    [SerializeField]
    public float humanRunSpeed = 5.0f;

    private int howManyAcornsLeft;

    // walk to path wait times
    float returnToPathWaitTime = 3.0f;
    float returnToPathNoWaitTime = 0f;

    // timer delay for acorns disappearing on pick up
    float pickUpTimer = 3f;

    //handle food single use check
    bool returnedToPath = false;

    //Transform target;
    //GameObject squrrielTarget;
    //GameObject playerController;
    public GameObject acornHolder;

    public float NPCVolume = .1f;

    //flee positions
    private Vector3 startingPos;

    private AudioSource audio;
    [SerializeField]
    private SoundEffects SoundEffectClips;

    public SquirrelController Player
    {
        get { return AIManager.singleton.sController; }
    }
  

    /// set the nav mesh agent for humans to walk on as well as set target (player) to chase.
    public void Start() 
    {
        
        layerMask = LayerMask.GetMask("EatenFood");

        howManyAcornsLeft = friendlyVariables.howManyAcorns;

        audio = GetComponent<AudioSource>();

        human = this.GetComponent<NavMeshAgent>();

        chaseTimer = chaseVariables.chaseTime;

        if(human == null)
        {
            Debug.LogWarning("No nav mesh");
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
                Debug.LogWarning("Add more points to walk between");
            }
        }
    }

    /// handles the main swaping of states for each person. Runs specific behaviour while in a certain state.
    public void Update() 
    {
        distanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);
        // -----States------
        if(Player.giantSettings.inGiantMode)
        {
            print("big ballmode");
            currentState = HumanStates.Ending;
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            print("little ballmode");
            GetComponent<Collider>().enabled = true;
        }
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
                HandleFoodState();
                break;
            }
            case HumanStates.Ending:
            {
                EndingState();
                break;
            }
        }
    }

    private void EndingState()
    {
        //walking along path
        Vector3 targetPos = endVariables.endPathPoints[0].transform.position;
        human.SetDestination(targetPos);

        // if near player run away
        if(distanceToPlayer < 15.0f)
        {
            startingPos = transform.position;
            RunFromPlayer(startingPos);
        }
    }
    /// functionaility for catching behaviour 
    private void CatchingState()
    {  
        human.speed = humanWalkSpeed; 
        if(!hasCaughtRecently)
        {
            
            hasCaughtRecently = true;  
            checkBeenRun = false;
            hitPlayerStun = false;
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
                    if(hitPlayerStun)
                    {
                        StartCoroutine(returnToPath(returnToPathNoWaitTime));
                    }
                    else
                    {
                        //CallAnimationEvents(AnimTriggers.running);
                        currentState = HumanStates.Chase;
                    }
                    // if didnt get player enter chase state again
                    
                }
            }
            stillFood = checkForFood();
        }
    }

    private void HandleFoodState()
    {
        // option 1 = go to bin
        catchChoice = 1;
        if(catchChoice == 0)
        {
            
            Bin bin = pathFollowingVariables.homePoint.closestBin(transform.position);


            if(bin.radius <= Vector3.Distance(bin.transform.position, transform.position))
            {
                // animation walk (not moving)
                CallAnimationEvents(AnimTriggers.walking);
                acornHolder.SetActive(true);
                human.SetDestination(bin.transform.position);
            }
            // put food in bin
            else
            {
                
                CallAnimationEvents(AnimTriggers.dropping);
                acornHolder.SetActive(true); 
                human.velocity = Vector3.zero;
                if(!returnedToPath)
                {
                    returnedToPath = true;
                    StartCoroutine(returnToPath(2.5f));
                }
                
            }
        }    
        else
        {    
            
            CallAnimationEvents(AnimTriggers.eating);
            acornHolder.SetActive(true);
            if(!returnedToPath)
            {
                audio.PlayOneShot (SoundEffectClips.eating, 1f);
                returnedToPath = true;
                StartCoroutine(returnToPath(2.5f));
            }
            
        }
        
    }

    ///functionaility for chasing behaviour. Added checks to see if the npc leaves their boundry area or chases for 'x' ammount of time 
    private void ChaseState()
    { 
        
        human.speed = humanRunSpeed;
        CallAnimationEvents(AnimTriggers.running);
        if(!havntSpotted)
        {
            int alertSound =  UnityEngine.Random.Range(0, 2);
            if(alertSound == 0)
            {
                audio.PlayOneShot (SoundEffectClips.alert, 1f);
            }
            else
            {
                audio.PlayOneShot (SoundEffectClips.alert2, 1f);
            }
            
            havntSpotted = true;
            exclaim.Play();
        }
        
        /// runs a check to see if the human is still within boundary
        if(checkBoundry() == true)
        {
            // runs a check to test if human can see the player 
            SeePlayer();
            // if can see player then target and move towards player
            human.SetDestination(Player.transform.position);
            
            // if within boundary then chase while timer is above -
            if (chaseTimer > 0f)
            {   
                
                Debug.DrawLine(transform.position, Player.transform.position);

                chaseTimer -= Time.deltaTime;
                // checks to see if human is within range to "catch" player
                if (distanceToPlayer < 1.5f)
                {
                    havntSpotted = false;

                    hasCaughtRecently = false;

                    currentState = HumanStates.Catch;
                }
                     
            }
            else
            {
                chaseTimer = chaseVariables.chaseTime;
                SetDest();
                havntSpotted = false;
                StartCoroutine(returnToPath(returnToPathNoWaitTime));
            }
        }
        /// if the human leaves the set area they will return to following their path
        else
        {
            CallAnimationEvents(AnimTriggers.walking);
            SetDest();
            StartCoroutine(returnToPath(returnToPathNoWaitTime));
        }
                
    }

    /// starts a timer to watch player food and offers 1 piece of food. ALso has internal timer to stop human from giving player too much food
    private void FriendlyState()
    {
        watchedFor += Time.deltaTime;
        
        //facePlayer();
        //human.SetDestination(transform.position);
        
        if(!givenfood)
        {
            if(howManyAcornsLeft != 0)
            {
                audio.PlayOneShot (SoundEffectClips.friendly, 1f);
                Instantiate(friendlyVariables.foodToGive, new Vector3(transform.position.x -1.0f, transform.position.y , transform.position.z ), Quaternion.identity); 
                howManyAcornsLeft -= 1;
                timeToFood = 0.0f;
                givenfood = true;
            }

        }
        else
        {
            timeToFood += Time.deltaTime;
        }

        if(watchedFor > friendlyVariables.watchTimer)
        {
            givenfood = false;
            //howManyAcornsLeft = friendlyVariables.howManyAcorns;
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
        print(canSee);
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
        SetDest();
        
        if(walking && human.remainingDistance <= 1.0f)
        {
            CallAnimationEvents(AnimTriggers.idle);
            if(pathFollowingVariables.walkingPause)
            {

                if(npcIdleAnim == IdleMode.Lyingdown)
                {
                    CallAnimationEvents(AnimTriggers.lying);
                }
                else if(npcIdleAnim == IdleMode.WSitting)
                {
                    anim.SetInteger("HumanSitting", 1);
                }
                else if(npcIdleAnim == IdleMode.MSitting)
                {
                    anim.SetInteger("HumanSitting", 2);
                }
                else if(npcIdleAnim == IdleMode.WSittingTalk)
                {
                    anim.SetInteger("HumanSitting", 3);
                }
                else if(npcIdleAnim == IdleMode.MSittingTalk)
                {
                    anim.SetInteger("HumanSitting", 4);
                }
                
                waitTimer += Time.deltaTime;
                
                if(waitTimer >= pathFollowingVariables.pathPoints[currentPathPt].waitForThisLong)
                {
                    waitTimer = 0;
                    CallAnimationEvents(AnimTriggers.walking);
                    ChangePathPt();
                    SetDest();
                }
            }
            else
            {
                CallAnimationEvents(AnimTriggers.walking);
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
            
            CallAnimationEvents(AnimTriggers.walking);
            human.SetDestination(bestCollider.transform.position);
            if (bestDistance < 5f)
            {
                walkToFoodTimer += Time.deltaTime;
            }
            
            if(walkToFoodTimer > howLongCanWalkToFood)
            {
                bestCollider.gameObject.layer = LayerMask.NameToLayer("Food");
                
                walkToFoodTimer = 0f;
            }
            
            if(bestDistance < 1f)
            {
                human.velocity = Vector3.zero;
                
                CallAnimationEvents(AnimTriggers.pickup);
                if(!pickUpFoodOnce)
                {
                    StartCoroutine(pickUpFood(bestCollider, pickUpTimer));
                    pickUpFoodOnce = true;
                }
                
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
        
        stunScript.stompEffect(Player, Player.behaviourScripts.foodGrabber, catchVariables.takeFoodAmmount);
        audio.PlayOneShot(SoundEffectClips.stomp, 1f);
        //yield return new WaitForSeconds(1.2f);
        stillFood = checkForFood();
        
        checkBeenRun = true;
    }

    
    IEnumerator pickUpFood(Collider food, float timer)
    {
        yield return new WaitForSeconds(timer);
        pickUpTimer -= .15f;
        food.gameObject.SetActive(false);
        acornHolder.SetActive(true);
        pickUpFoodOnce = false;
        food.GetComponent<Food>().respawn();

        //yield return new WaitForSeconds(1.2f);
        //acornHolder.SetActive(false);
    }

    IEnumerator returnToPath(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        acornHolder.SetActive(false);
        CallAnimationEvents(AnimTriggers.walking);
        pickedUpFood = false;
        currentState = HumanStates.PathFollowing;

        yield return new WaitForSeconds(catchVariables.catchResetTimer);
        hasCaughtRecently = false;
        returnedToPath = false;
    }

    public void RunFromPlayer(Vector3 startPos)
    {
        CallAnimationEvents(AnimTriggers.running);
        //transform.rotation = Quaternion.LookRotation((transform.position - Player.transform.position), Vector3.up);
        Quaternion LookAtRotation = Quaternion.LookRotation(transform.position - Player.transform.position);

        Quaternion LookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, LookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        transform.rotation = LookAtRotationOnly_Y;
        
        

        Vector3 dirToPlaayer = transform.position - Player.transform.position;
        Vector3 newPos = transform.position + dirToPlaayer;

        human.SetDestination(newPos);
        // look away from player
    }

    /// turns to face player
    public void facePlayer()
    {

        Vector3 direction = (Player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    /// checks to see if any food is within its range after catching the player
    
    /// runs a ray cast to check if the player is within a LOS.  
    bool SeePlayer()
    {
        Vector3 targetDir = Player.transform.position - transform.position;

        // view angle
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
        RaycastHit hit;

        if ((angleToPlayer >= -detectionAngle && angleToPlayer <= detectionAngle) && (distanceToPlayer <= range))
        {
            if(Physics.Linecast (transform.position, Player.transform.transform.position, out hit))
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
    
    public void humanSquash(bool bigSquirrel)
    {
        if(bigSquirrel)
            print("test");
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
        dropping,
        sitting,
        lying

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

        public int howManyAcorns = 1;
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

    [System.Serializable]
    private class SoundEffects
    {
        public AudioClip eating;
        public AudioClip alert;
        public AudioClip alert2;
        public AudioClip stomp;
        public AudioClip friendly;
    }

    [System.Serializable]
    private class EndingVariable
    {
        public List<WayPoints> endPathPoints;
    }

}