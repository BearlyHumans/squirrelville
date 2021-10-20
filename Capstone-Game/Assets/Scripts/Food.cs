using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Food : MonoBehaviour
{
    private Vector3 spawnPoint;
    private Quaternion spawnRotation;
    private static List<Food> foodList = new List<Food>();

    public float respawnTimer;
    private Rigidbody rb;

    [Header("Events")]
    public UnityEvent pickupEvent;
    public UnityEvent throwEvent;


    private void Awake()
    {
        foodList.Add(this);

        spawnPoint = transform.position;
        spawnRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }


    public bool isEaten()
    {
        return !gameObject.activeInHierarchy;
    }

    public static Food[] GetFood()
    {
        return foodList.ToArray();
    }

    public static bool IsFood(GameObject obj)
    {
        int foodLayer = LayerMask.NameToLayer("Food");
        return obj.layer == foodLayer;
    }
    public void respawn()
    {
        Invoke("respawnSelf", respawnTimer);
    }

    public void respawnSelf()
    {
        gameObject.SetActive(true);
        gameObject.layer = LayerMask.NameToLayer("Food");
        gameObject.transform.position = spawnPoint;
        gameObject.transform.rotation = spawnRotation;
        rb.velocity = Vector3.zero;
    }

}
