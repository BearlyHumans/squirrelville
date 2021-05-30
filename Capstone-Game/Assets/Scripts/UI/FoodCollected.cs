using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FoodCollected : MonoBehaviour
{
    private TMP_Text label;
    public FoodArea foodArea;
    private string prefix;

    private void Start()
    {
        foodArea.foodEnterEvent.AddListener((_) => UpdateLabel());
        foodArea.foodExitEvent.AddListener((_) => UpdateLabel());
    }

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        prefix = label.text;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        label.text = prefix + foodArea.GetFoodCount();
    }
}
