using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GhostLeader : MonoBehaviour
{
    public string GhostName = "A_Noob";
    public Transform TutorialFollower;
    public float TutorialStopDistance = 8f;
    public float TutorialResumeDistance = 3f;
    private List<Vector3> allPoints = new List<Vector3>();
    private List<Quaternion> allRots = new List<Quaternion>();
    private List<float> allTimes = new List<float>();

    public enum GhostMode { Timed, Tutorial, Off }
    public GhostMode mode = GhostMode.Timed;
    private GhostMode lockedMode = GhostMode.Timed;
    public bool Restart = false;

    [Header("Leave blank for default path (usually .../appdata/LocalLow/...)")]
    [Tooltip("Path should not include last slash (adds '/[GhostName].pth' automatically.")]
    public string CustomPath = "";

    [SerializeField]
    [Min(0)]
    private int currentFrame = 0;
    private float startTime = 0;
    private Vector3 lastPoint = Vector3.zero;
    private Quaternion targetRot = new Quaternion();
    private float moveSpeed = 0f;
    private float rotSpeed = 0;
    private bool tutorialPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        lockedMode = mode;

        if (!Load())
            lockedMode = GhostMode.Off;
        else
        {
            float totalTime = allTimes[allTimes.Count - 1];
            print(GhostName + "'s time: " + GhostTimer.TimeString(totalTime) + "(" + totalTime + "s)");
        }
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
            if (allTimes.Count > currentFrame + 1 && allPoints.Count > currentFrame + 1 && allRots.Count > currentFrame + 1)
            {
                if (allTimes[currentFrame + 1] < Time.time - startTime)
                {
                    //Get next point if the current time is greater than its recorded time
                    lastPoint = allPoints[currentFrame];
                    float oldTime = allTimes[currentFrame];
                    currentFrame += 1;
                    float newTime = allTimes[currentFrame];
                    float timeDiff = Mathf.Max(0.001f, newTime - oldTime);
                    moveSpeed = Vector3.Distance(lastPoint, allPoints[currentFrame]) / timeDiff;

                    //Get and set rotation
                    targetRot = allRots[currentFrame];
                    float angleDiff = Quaternion.Angle(targetRot, transform.rotation);
                    rotSpeed = angleDiff / timeDiff;
                }

                //Lerp the position based on an estimate of the speed
                transform.position = Vector3.MoveTowards(transform.position, allPoints[currentFrame], moveSpeed * Time.deltaTime);

                //Lerp the rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            }
            else
                lockedMode = GhostMode.Off;
        }
        else
        {
            if (allTimes.Count > currentFrame + 1 && allPoints.Count > currentFrame + 1 && allRots.Count > currentFrame + 1)
            {
                if (tutorialPaused == true && Vector3.Distance(allPoints[currentFrame + 1], TutorialFollower.position) < TutorialResumeDistance)
                    tutorialPaused = false;
                else if (tutorialPaused == false && Vector3.Distance(allPoints[currentFrame + 1], TutorialFollower.position) > TutorialStopDistance)
                    tutorialPaused = true;

                if (tutorialPaused == false)
                {
                    //Get next point if the current time is greater than its recorded time
                    lastPoint = allPoints[currentFrame];
                    float oldTime = allTimes[currentFrame];
                    currentFrame += 1;
                    float newTime = allTimes[currentFrame];
                    float timeDiff = Mathf.Max(0.001f, newTime - oldTime);
                    moveSpeed = Vector3.Distance(lastPoint, allPoints[currentFrame]) / timeDiff;

                    //Get and set rotation
                    targetRot = allRots[currentFrame];
                    float angleDiff = Quaternion.Angle(targetRot, transform.rotation);
                    rotSpeed = angleDiff / timeDiff;
                }

                //Lerp the position based on an estimate of the speed
                transform.position = Vector3.MoveTowards(transform.position, allPoints[currentFrame], moveSpeed * Time.deltaTime);

                //Lerp the rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            }
            else
                lockedMode = GhostMode.Off;
        }
    }

    public bool Load()
    {
        string path = "/" + GhostName.Replace(" ", "_") + ".pth";
        if (CustomPath == "")
            path = Application.persistentDataPath + path;
        else
            path = CustomPath + path;
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            GhostData data = (GhostData)bf.Deserialize(file);
            file.Close();

            //Add variables here.
            allPoints.Clear();
            foreach (SerialPoint p in data.Path)
                allPoints.Add(p.ToV3());

            allRots.Clear();
            foreach (SerialRot r in data.Rotations)
                allRots.Add(r.ToQuaternion());

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
