using System.Collections.Generic;
using UnityEngine;

public class AcornCounter : MonoBehaviour
{
    public SquirrelFoodGrabber squirrelFoodGrabber;
    public AcornCounterAcorn acornPrefab;

    private List<AcornCounterAcorn> acorns = new List<AcornCounterAcorn>();

    private void Start()
    {
        squirrelFoodGrabber.pickupEvent.AddListener((_) => UpdateDisplay());
        squirrelFoodGrabber.throwEvent.AddListener((_) => UpdateDisplay());
    }

    private void Awake()
    {
        for (int i = 0; i < squirrelFoodGrabber.maxFoodInInventory; i++)
        {
            AcornCounterAcorn acorn = GameObject.Instantiate<AcornCounterAcorn>(acornPrefab, new Vector3(i * 80, 0, 0), Quaternion.identity);
            acorn.transform.SetParent(gameObject.transform, false);
            acorns.Add(acorn);
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < acorns.Count; i++)
        {
            AcornCounterAcorn acorn = acorns[i];

            acorn.circle.enabled = i == squirrelFoodGrabber.foodCountBallForm - 1;

            bool hasAcorn = i < squirrelFoodGrabber.GetFoodCount();
            acorn.transparentAcorn.enabled = !hasAcorn;
            acorn.glowingAcorn.enabled = hasAcorn;
        }
    }
}
