using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FoodCollected : MonoBehaviour
{
    private TMP_Text label;
    public FoodArea foodArea;

    private void Start()
    {
        foodArea.foodEnterEvent.AddListener((_) => UpdateLabel());
        foodArea.foodExitEvent.AddListener((_) => UpdateLabel());
    }

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        label.text = $"Food collected: {foodArea.GetFoodCount()}";
    }
}
