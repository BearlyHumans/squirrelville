using UnityEngine;

[RequireComponent(typeof(FoodHandler))]
public class SquirrelFoodGrabber : MonoBehaviour
{
    private FoodHandler foodHandler;

    private void Awake()
    {
        foodHandler = GetComponent<FoodHandler>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject food = GameObject.FindWithTag("Food");
            if (food != null)
            {
                foodHandler.PickupFood(food);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            foodHandler.ThrowFood();
        }
    }
}
