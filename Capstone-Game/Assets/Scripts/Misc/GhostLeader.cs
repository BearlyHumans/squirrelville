using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GhostLeader : MonoBehaviour
{
    public string GhostName = "A_Noob";
    public Transform TutorialFollower;
    public float FollowerDistance = 3f;
    private List<Vector3> allPoints = new List<Vector3>();
    private List<float> allTimes = new List<float>();

    public enum GhostMode { Timed, Tutorial, Off }
    public GhostMode mode = GhostMode.Timed;
    private GhostMode lockedMode = GhostMode.Timed;
    public bool Restart = false;

    [SerializeField]
    private int currentFrame = 0;
    private float startTime = 0;
    private Vector3 lastPoint = Vector3.zero;
    private float timeDiff = 0.001f;

    // Start is called before the first frame update
    void Start()
    {
        lockedMode = mode;

        if (!Load())
            lockedMode = GhostMode.Off;
    }

    // Update is called once per frame
    void Update()
    {
        if (Restart)
        {
            Restart = false;
            currentFrame = 0;
            startTime = Time.time;
        }

        if (lockedMode == GhostMode.Off)
        {
            lockedMode = mode;
            startTime = Time.time;
        }

        if (lockedMode == GhostMode.Off || PauseMenu.paused)
            return;

        if (lockedMode == GhostMode.Timed || TutorialFollower == null)
        {
            if (allTimes.Count > currentFrame + 1 && allPoints.Count > currentFrame + 1)
            {
                if (allTimes[currentFrame + 1] < Time.time - startTime)
                {
                    lastPoint = allPoints[currentFrame];
                    float oldTime = allTimes[currentFrame];
                    currentFrame += 1;
                    float newTime = allTimes[currentFrame];
                    timeDiff = Mathf.Max(0.001f, newTime - oldTime);
                }
                float speed = Vector3.Distance(lastPoint, allPoints[currentFrame]) / timeDiff;
                transform.position = Vector3.MoveTowards(transform.position, allPoints[currentFrame], speed * Time.deltaTime);
            }
            else
                lockedMode = GhostMode.Off;
        }
        else
        {
            if (allTimes.Count > currentFrame + 1 && allPoints.Count > currentFrame + 1)
            {
                if (Vector3.Distance(allPoints[currentFrame + 1], TutorialFollower.position) < FollowerDistance)
                {
                    lastPoint = allPoints[currentFrame];
                    float oldTime = allTimes[currentFrame];
                    currentFrame += 1;
                    float newTime = allTimes[currentFrame];
                    timeDiff = Mathf.Max(0.001f, newTime - oldTime);
                }
                float speed = Vector3.Distance(lastPoint, allPoints[currentFrame]) / timeDiff;
                transform.position = Vector3.MoveTowards(transform.position, allPoints[currentFrame], speed * Time.deltaTime);
            }
            else
                lockedMode = GhostMode.Off;
        }
    }

    public bool Load()
    {
        string path = Application.persistentDataPath + "/" + GhostName.Replace(" ", "_") + ".pth";
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            GhostData data = (GhostData)bf.Deserialize(file);
            file.Close();

            //Add variables here.
            foreach (SerialPoint p in data.Path)
            {
                allPoints.Add(p.ToV3());
            }
            allTimes = data.Times;

            if (allPoints.Count > 0)
            {
                transform.position = allPoints[0];
                lastPoint = allPoints[0];
            }
            return true;
        }
        return false;
    }
}
