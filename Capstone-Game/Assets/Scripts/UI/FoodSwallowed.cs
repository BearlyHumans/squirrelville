using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FoodSwallowed : MonoBehaviour
{
    private TMP_Text label;
    public SquirrelFoodGrabber squirrelFoodGrabber;
    private string prefix;

    private void Start()
    {
        squirrelFoodGrabber.pickupEvent.AddListener((_) => UpdateLabel());
        squirrelFoodGrabber.throwEvent.AddListener((_) => UpdateLabel());
    }

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        prefix = label.text;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        label.text = prefix + squirrelFoodGrabber.GetFoodCount();
    }
}
