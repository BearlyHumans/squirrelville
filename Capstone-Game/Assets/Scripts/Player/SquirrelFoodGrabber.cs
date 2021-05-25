using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquirrelFoodGrabber : MonoBehaviour
{
    private Stack<GameObject> foodStack = new Stack<GameObject>();
    private Rigidbody squirrelrb;
    public Transform mouth;

    [Header("Picking up food")]

    [Tooltip("What the delay between picking up food will be (in seconds)")]
    public float pickupDelay = 0.0f;
    private float pickupTime = 0.0f;

    [Header("Throwing up food")]

    [Tooltip("What velocity the food will be spat out with")]
    public float throwVelocity = 1.0f;
    [Tooltip("What the initial delay between spitting out food will be (in seconds)")]
    public float initialThrowDelay = 1.0f;
    [Tooltip("How much the delay will be divided by after each food is spat out (in seconds)")]
    [Min(1)]
    public float throwDelayDivisor = 2.0f;
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
            GameObject food = GameObject.FindWithTag("Food");
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
        foodrb.transform.position = mouth.position;
        foodrb.velocity = squirrelrb.velocity + mouth.forward * throwVelocity;
        foodrb.transform.rotation = Quaternion.identity;

        food.SetActive(true);

        throwTime = Time.time + throwDelay;
        throwDelay = Mathf.Max(throwDelay / throwDelayDivisor, minThrowDelay);

        Debug.Log($"Threw up {food.name}");
    }
}
