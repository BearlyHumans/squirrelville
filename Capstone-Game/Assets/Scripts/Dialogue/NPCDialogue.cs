using UnityEngine;
using UnityEngine.Events;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]

    public Dialogue initialDialogue;
    public Dialogue preFoodDialogue;
    public Dialogue foodDialogue;
    public Dialogue postFoodDialogue;

    [Header("Food Area")]

    [Tooltip("The food area the NPC checks for food")]
    public ObjectArea foodArea;

    [Tooltip("The food required to complete the objective")]
    [Min(0)]
    public int foodRequired;

    [Header("Particles")]

    [Tooltip("The particle system to emit when the NPC has something important to say")]
    public ParticleSystem dialogueParticle;

    [Header("Events")]

    public UnityEvent dialogueStart;
    public UnityEvent dialogueNext;
    public UnityEvent dialogueFinish;
    public UnityEvent initialDialogueFinished;
    public UnityEvent foodCollected;

    private bool firstTimeSpeaking = true;
    private bool foodTaken = false;
    private bool isSpeaking = false;

    private void Start()
    {
        dialogueStart.AddListener(() => OnDialogueStart());
        dialogueFinish.AddListener(() => OnDialogueFinish());
    }

    private void Update()
    {
        dialogueParticle.gameObject.SetActive(IsIndicatorVisible());
    }

    void OnDialogueStart()
    {
        isSpeaking = true;
    }

    void OnDialogueFinish()
    {
        isSpeaking = false;

        if (GetDialogue() == foodDialogue)
        {
            foodCollected.Invoke();

            GameObject[] objects = foodArea.GetObjectsInArea();
            int objectsToRemove = Mathf.Min(foodRequired, objects.Length);

            for (int i = 0; i < objectsToRemove; i++)
            {
                objects[i].SetActive(false);
            }

            GameObject.Destroy(foodArea.gameObject);

            foodTaken = true;
        }

        if (firstTimeSpeaking)
        {
            initialDialogueFinished.Invoke();
            firstTimeSpeaking = false;
        }
    }

    private bool CollectedEnoughFood()
    {
        return foodArea.GetObjectCount() >= foodRequired;
    }

    public Dialogue GetDialogue()
    {
        if (firstTimeSpeaking && initialDialogue != null)
        {
            return initialDialogue;
        }
        else if (foodTaken)
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

    public bool IsIndicatorVisible()
    {
        Dialogue dialogue = GetDialogue();
        return (dialogue == initialDialogue || dialogue == foodDialogue) && !isSpeaking;
    }
}
