using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Inventory : MonoBehaviour
{
    private Stack<GameObject> foodStack = new Stack<GameObject>();
    private Rigidbody squirrelrb;
    public Transform mouth;
    public float throwSpeed = 1.0f;

    private void Awake()
    {
        squirrelrb = GetComponent<Rigidbody>();
    }

    public void PickupFood(GameObject food)
    {
        // Assumes GameObject is food
        foodStack.Push(food);
        food.SetActive(false);
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
