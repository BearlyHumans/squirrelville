using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class SquirrelFoodGrabber : MonoBehaviour
{
    private Inventory inventory;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject food = GameObject.FindWithTag("Food");
            if (food != null)
            {
                inventory.PickupFood(food);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            inventory.ThrowFood();
        }
    }
}
