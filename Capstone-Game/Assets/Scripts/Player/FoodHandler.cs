using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FoodHandler : MonoBehaviour
{
    private Stack<GameObject> foodStack = new Stack<GameObject>();
    private Rigidbody squirrelrb;
    public Transform mouth;
    public float throwSpeed = 1.0f;
    public float pickupCooldown = 0.0f;
    private float pickupTime = 0.0f;

    private void Awake()
    {
        squirrelrb = GetComponent<Rigidbody>();
    }

    public void PickupFood(GameObject food)
    {
        // Assumes GameObject is food

        if (Time.time < pickupTime) return;

        foodStack.Push(food);
        food.SetActive(false);
        pickupTime = Time.time + pickupCooldown;
        Debug.Log($"Picked up {food.name}");
    }

    [ContextMenu("Throw food")]
    public void ThrowFood()
    {
        if (foodStack.Count == 0) return;

        GameObject food = foodStack.Pop();

        Rigidbody foodrb = food.GetComponent<Rigidbody>();
        foodrb.transform.position = mouth.position;
        foodrb.velocity = squirrelrb.velocity + mouth.forward * throwSpeed;
        foodrb.transform.rotation = Quaternion.identity;

        food.SetActive(true);

        Debug.Log($"Threw up {food.name}");
    }
}
