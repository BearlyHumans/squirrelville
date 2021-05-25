using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquirrelFoodGrabber : MonoBehaviour
{
    private Stack<GameObject> foodStack = new Stack<GameObject>();
    private Rigidbody squirrelrb;

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

    private void Awake()
    {
        squirrelrb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            GameObject food = GetNearestFood();
            if (food != null)
            {
                PickupFood(food);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            throwDelay = initialThrowDelay;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Time.time < throwTime) return;

            ThrowFood();
        }
    }

    public void PickupFood(GameObject food)
    {
        // Assumes GameObject is food

        if (Time.time < pickupTime) return;
        if (!normalMouth.gameObject.activeInHierarchy) return;

        foodStack.Push(food);
        food.SetActive(false);
        pickupTime = Time.time + pickupDelay;
        Debug.Log($"Picked up {food.name}");
    }

    [ContextMenu("Throw food")]
    public void ThrowFood()
    {
        if (foodStack.Count == 0) return;

        GameObject food = foodStack.Pop();
        Rigidbody foodrb = food.GetComponent<Rigidbody>();
        Transform activeMouth = normalMouth.gameObject.activeInHierarchy ? normalMouth : ballMouth;

        foodrb.transform.position = activeMouth.position;
        foodrb.velocity = squirrelrb.velocity + activeMouth.forward * throwVelocity;
        foodrb.transform.rotation = Quaternion.identity;

        food.SetActive(true);

        throwTime = Time.time + throwDelay;
        throwDelay = Mathf.Max(throwDelay / throwDelayDivisor, minThrowDelay);

        Debug.Log($"Threw up {food.name}");
    }

    int GetFoodCount()
    {
        return foodStack.Count;
    }

    private GameObject[] GetFoodInRange()
    {
        List<GameObject> food = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(squirrelrb.position, pickupRadius);
        foreach (Collider collider in colliders)
        {
            GameObject obj = collider.gameObject;
            if (obj.tag == "Food")
            {
                food.Add(obj);
            }
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
