using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FoodCollected : MonoBehaviour
{
    private TMP_Text label;
    public ObjectArea foodStockpileArea;
    private string prefix;

    private void Start()
    {
        foodStockpileArea.enterEvent.AddListener((_) => UpdateLabel());
        foodStockpileArea.exitEvent.AddListener((_) => UpdateLabel());
    }

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        prefix = label.text;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        label.text = prefix + foodStockpileArea.GetObjectCount();
    }
}
