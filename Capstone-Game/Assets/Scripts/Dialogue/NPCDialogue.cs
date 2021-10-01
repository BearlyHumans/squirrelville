using UnityEngine;
using UnityEngine.Events;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]

    public Dialogue preFoodDialogue;
    public Dialogue foodDialogue;
    public Dialogue postFoodDialogue;

    [Header("Food Area")]

    [Tooltip("The food area the NPC checks for food")]
    public ObjectArea foodArea;

    [Tooltip("The food required to complete the objective")]
    [Min(0)]
    public int foodRequired;

    [Header("Events")]

    public UnityEvent dialogueStart;
    public UnityEvent dialogueNext;
    public UnityEvent dialogueFinish;
    public UnityEvent foodCollected;

    private bool foodTaken = false;

    private void Start()
    {
        dialogueFinish.AddListener(() => OnDialogueFinish());
    }

    void OnDialogueFinish()
    {
        if (GetDialogue() == foodDialogue)
        {
            foodCollected.Invoke();

            GameObject[] objects = foodArea.GetObjectsInArea();
            int objectsToRemove = Mathf.Min(foodRequired, objects.Length);

            for (int i = 0; i < objectsToRemove; i++)
            {
                GameObject.Destroy(objects[i]);
            }

            GameObject.Destroy(foodArea.gameObject);

            foodTaken = true;
        }
    }

    private bool CollectedEnoughFood()
    {
        return foodArea.GetObjectCount() >= foodRequired;
    }

    public Dialogue GetDialogue()
    {
        if (foodTaken)
        {
            return postFoodDialogue;
        }
        else if (CollectedEnoughFood())
        {
            return foodDialogue;
        }
        else
        {
            return preFoodDialogue;
        }
    }

    public bool HasDialogue()
    {
        return GetDialogue() != null;
    }
}
