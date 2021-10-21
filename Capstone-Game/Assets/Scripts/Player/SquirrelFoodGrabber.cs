using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Player;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NPCInteractionManager))]
[RequireComponent(typeof(SquirrelController))]
public class SquirrelFoodGrabber : MonoBehaviour
{
    private Stack<GameObject> foodStack = new Stack<GameObject>();
    private Rigidbody squirrelrb;
    private SquirrelController controller;
    private NPCInteractionManager npcInteractionManager;

    public ParticleSystem foodEaten;

    [Header("Mouth")]

    [Tooltip("The position of the mouth for the normal state rotated in the direction food will be spat out towards")]
    public Transform normalMouth;
    [Tooltip("The position of the mouth for the ball state rotated in the direction food will be spat out towards")]
    public Transform ballMouth;

    [Header("Picking up food")]

    [Tooltip("What the delay between picking up food will be (in seconds)")]
    public float pickupDelay = 0.5f;
    [Tooltip("How far away food can be picked up from")]
    public float pickupRadius = 1.0f;
    private float pickupTime = 0.0f;
    [Tooltip("The maximum number of food that can be in the player's inventory at once (-1 to disable)")]
    [Min(-1)]
    public int maxFoodInInventory = 10; // -1 to disable
    [Tooltip("How much food does the squirrel need to swallow to be able to turn into a ball")]
    [Min(0)]
    public int foodCountBallForm = 0;
    public Material highlightMaterial;

    [Header("Throwing up food")]

    [Tooltip("What velocity the food will be spat out with")]
    public float throwVelocity = 1.0f;
    [Tooltip("What the initial delay between spitting out food will be (in seconds)")]
    public float initialThrowDelay = 1.0f;
    [Tooltip("How much the delay will be divided by after each food is spat out (in seconds)")]
    [Min(1)]
    public float throwDelayDivisor = 1.5f;
    [Tooltip("What the shortest delay can be between spitting out food (in seconds)")]
    public float minThrowDelay = 0.2f;
    private float throwTime = 0.0f;
    private float throwDelay;

    [Header("Events")]
    public UnityEvent<GameObject> pickupEvent;
    public UnityEvent<GameObject> throwEvent;

    private GameObject nearestFood = null;

    private void Awake()
    {
        squirrelrb = GetComponent<Rigidbody>();
        npcInteractionManager = GetComponent<NPCInteractionManager>();
        controller = GetComponent<SquirrelController>();
    }

    private void Update()
    {
        if (PauseMenu.paused) return;

        GameObject prevNearestFood = nearestFood;
        nearestFood = CanEatFood() ? GetNearestFood() : null;

        if (prevNearestFood != nearestFood)
        {
            // Remove highlight material from previous nearest food
            if (prevNearestFood != null)
            {
                MeshRenderer prevFoodMeshRenderer = prevNearestFood.GetComponent<MeshRenderer>();
                Material[] materials = prevFoodMeshRenderer.materials;
                materials[1] = null;
                prevFoodMeshRenderer.materials = materials;
            }

            // Add highlight material to nearest food
            if (nearestFood != null)
            {
                MeshRenderer foodMeshRenderer = nearestFood.GetComponent<MeshRenderer>();
                Material[] materials = foodMeshRenderer.materials;

                // Make room for the highlight material
                if (materials.Length == 1)
                {
                    materials = new Material[2]
                    {
                        materials[0],
                        highlightMaterial
                    };
                }
                else
                {
                    materials[1] = highlightMaterial;
                }

                foodMeshRenderer.materials = materials;
            }
        }

        if (nearestFood != null && Input.GetButton("Eat") && CanEatFood())
        {
            PickupFood(nearestFood);
        }

        if (CanThrowFood())
        {
            if (Input.GetButtonDown("Spit"))
            {
                throwDelay = initialThrowDelay;
            }

            if (Input.GetButton("Spit"))
            {
                if (Time.time < throwTime) return;

                ThrowFood(0);
            }
        }
    }

    public void PickupFood(GameObject food)
    {
        // Assumes GameObject is food

        foodStack.Push(food);
        food.SetActive(false);
        pickupTime = Time.time + pickupDelay;
        food.layer = LayerMask.NameToLayer("EatenFood");
        controller.CallEvents(SquirrelController.EventTrigger.eat);
        pickupEvent.Invoke(food);
        food.GetComponent<Food>().pickupEvent.Invoke();
        Instantiate(foodEaten, food.transform.position, food.transform.rotation);
    }

    [ContextMenu("Throw food")]
    public void ThrowFood(int mode)
    {
        GameObject food = foodStack.Pop();
        Rigidbody foodrb = food.GetComponent<Rigidbody>();
        Transform activeMouth = normalMouth.gameObject.activeInHierarchy ? normalMouth : ballMouth;

        foodrb.transform.position = activeMouth.position;
        foodrb.velocity = squirrelrb.velocity + activeMouth.forward * throwVelocity;
        foodrb.transform.rotation = Quaternion.identity;

        food.SetActive(true);

        if (mode == 0)
        {
            food.layer = LayerMask.NameToLayer("Food");
        }
        else
        {
            food.layer = LayerMask.NameToLayer("EatenFood");
        }

        throwTime = Time.time + throwDelay;
        throwDelay = Mathf.Max(throwDelay / throwDelayDivisor, minThrowDelay);

        controller.CallEvents(SquirrelController.EventTrigger.spit);
        throwEvent.Invoke(food);
        food.GetComponent<Food>().throwEvent.Invoke();
    }

    public bool CanEatFood()
    {
        return (
            // Player has room for more food
            (maxFoodInInventory < 0 || GetFoodCount() < maxFoodInInventory) &&
            // Player isn't interacting with an NPC
            !npcInteractionManager.isInteracting &&
            // Pickup cooldown has ended
            Time.time >= pickupTime &&
            // In normal squirrel form
            normalMouth.gameObject.activeInHierarchy
        );
    }

    public bool CanThrowFood()
    {
        return (
            // Player isn't interacting with an NPC
            !npcInteractionManager.isInteracting &&
            // Player has food to spit
            foodStack.Count > 0
        );
    }

    public int GetFoodCount()
    {
        return foodStack.Count;
    }

    private GameObject[] GetFoodInRange()
    {
        List<GameObject> food = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(squirrelrb.position, pickupRadius, LayerMask.GetMask("Food", "EatenFood"));
        foreach (Collider collider in colliders)
        {
            food.Add(collider.gameObject);
        }

        return food.ToArray();
    }

    private GameObject GetNearestFood()
    {
        GameObject closestObj = null;
        float closestDistSqr = Mathf.Infinity;

        GameObject[] food = GetFoodInRange();
        foreach (GameObject obj in food)
        {
            Vector3 deltaPos = obj.transform.position - squirrelrb.position;
            float distSqr = deltaPos.sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestObj = obj;
            }
        }

        return closestObj;
    }
}
