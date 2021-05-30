using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FoodArea : MonoBehaviour
{
    private Collider trigger;
    private Food[] oldFoodInArea;

    public UnityEvent<GameObject> foodEnterEvent;
    public UnityEvent<GameObject> foodExitEvent;

    private void Start()
    {
        trigger = GetComponent<Collider>();
    }

    private void Update()
    {
        // Detect when food is eaten from within the area
        if (oldFoodInArea != null)
        {
            foreach (Food food in oldFoodInArea)
            {
                if (food.isEaten() && foodExitEvent != null)
                {
                    foodExitEvent.Invoke(food.gameObject);
                }
            }
        }

        oldFoodInArea = GetFoodInArea();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Food.IsFood(other.gameObject) && foodEnterEvent != null)
        {
            foodEnterEvent.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Food.IsFood(other.gameObject) && foodExitEvent != null)
        {
            foodExitEvent.Invoke(other.gameObject);
        }
    }

    public Food[] GetFoodInArea()
    {
        Food[] foodArr = Food.GetFood();
        List<Food> foodInArea = new List<Food>();

        foreach (Food food in foodArr)
        {
            Bounds foodBounds = food.GetComponent<Collider>().bounds;
            if (!food.isEaten() && trigger.bounds.Intersects(foodBounds))
            {
                foodInArea.Add(food);
            }
        }

        return foodInArea.ToArray();
    }

    public int GetFoodCount()
    {
        return GetFoodInArea().Length;
    }
}
