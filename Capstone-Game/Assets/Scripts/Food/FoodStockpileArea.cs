using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class FoodStockpileArea : MonoBehaviour
{
    private MeshRenderer mesh;
    private Collider trigger;
    public TMP_Text label;
    private Food[] oldFoodInArea;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        trigger = GetComponent<Collider>();
        // mesh.enabled = false;
    }

    private void Update()
    {
        label.text = $"Food collected: {GetFoodCount()}";

        if (oldFoodInArea != null)
        {
            foreach (Food food in oldFoodInArea)
            {
                if (food.isEaten())
                {
                    Debug.Log($"{food.name} was eaten");
                }
            }
        }

        oldFoodInArea = GetFoodInArea();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Food.IsFood(other.gameObject))
        {
            Debug.Log($"{other.name} entered");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Food.IsFood(other.gameObject))
        {
            Debug.Log($"{other.name} rolled out");
        }
    }

    public Food[] GetFoodInArea()
    {
        Food[] foodArr = Food.GetFood();
        List<Food> foodInArea = new List<Food>();

        foreach (Food food in foodArr)
        {
            if (!food.isEaten() && mesh.bounds.Contains(food.transform.position))
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
