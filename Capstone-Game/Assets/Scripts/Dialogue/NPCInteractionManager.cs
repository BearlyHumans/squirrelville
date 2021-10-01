using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Player.SquirrelController))]
public class NPCInteractionManager : MonoBehaviour
{
    [Tooltip("Reference to the dialogue box element")]
    public DialogueBox dialogueBox;

    [Tooltip("How far away NPCs can be interacted from")]
    public float interactionRadius = 1.0f;

    [Tooltip("Event evoked when an interaction with an NPC starts")]
    public UnityEvent interactionStart;

    [Tooltip("Event evoked when an interaction with an NPC finishes")]
    public UnityEvent interactionFinish;

    [Tooltip("Whether the squirrel is currently interacting with an NPC")]
    [HideInInspector]
    public bool isInteracting = false;

    private Rigidbody squirrelrb;
    private Player.SquirrelController squirrelController;

    void Awake()
    {
        squirrelrb = GetComponent<Rigidbody>();
        squirrelController = GetComponent<Player.SquirrelController>();
    }

    void Update()
    {
        if (PauseMenu.paused) return;

        if (CanInteract())
        {
            if (isInteracting)
            {
                isInteracting = false;
                squirrelController.UnfreezeMovement();
                interactionFinish?.Invoke();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    GameObject npc = GetNearestNPC();
                    if (npc != null)
                    {
                        isInteracting = true;
                        squirrelController.FreezeMovement();
                        interactionStart?.Invoke();
                        if (npc.TryGetComponent<NPCDialogue>(out NPCDialogue npcDialogue) && npcDialogue.HasDialogue())
                        {
                            dialogueBox.SetDialogue(npcDialogue);
                        }
                    }
                }
            }
        }
    }

    private GameObject[] GetNPCsInRange()
    {
        List<GameObject> npcs = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(squirrelrb.position, interactionRadius, LayerMask.GetMask("NPC"));
        foreach (Collider collider in colliders)
        {
            npcs.Add(collider.gameObject);
        }

        return npcs.ToArray();
    }

    private GameObject GetNearestNPC()
    {
        GameObject closestObj = null;
        float closestDistSqr = Mathf.Infinity;

        GameObject[] npcs = GetNPCsInRange();
        foreach (GameObject obj in npcs)
        {
            Vector3 deltaPos = obj.transform.position - squirrelrb.position;
            float distSqr = deltaPos.sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestObj = obj;
            }
        }

        return closestObj;
    }

    public bool CanInteract()
    {
        return !dialogueBox.isDialogueOpen;
    }
}
