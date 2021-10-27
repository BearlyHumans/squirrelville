using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GhostTimer : MonoBehaviour
{
    public Transform RecordTarget;
    public string GhostName = "A_Noob";
    private List<Vector3> allPoints = new List<Vector3>();
    private List<Quaternion> allRots = new List<Quaternion>();
    private List<float> allTimes = new List<float>();
    [Min(1)]
    public float RecordingFPS = 10;
    [Header("True to Start, False to Save (Toggle W/ Backspace)")]
    public bool Record = false;
    private bool recording = false;
    private float startTime = 0f;
    private float endTime = 0f;

    private float lastFrameTime = 0f;

    [Header("Leave blank for default path (usually .../appdata/LocalLow/...)")]
    [Tooltip("Path should not include last slash (adds '/[GhostName].pth' automatically.")]
    public string CustomPath = "";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((Record == true || recording == true) && RecordTarget == null)
        {
            Record = false;
            recording = false;
            Debug.LogError("Select a target before recording!");
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Record = !Record;
        }

        if (Record != recording)
        {
            if (recording == true)
            {
                //If currently recording and the toggle bool is now different, stop recording and save.
                recording = false;
                Save();
            }
            else
            {
                //If not recording and the toggle bool is now different, start recording.
                recording = true;
                startTime = Time.time;
            }
        }

        if (RecordTarget != null && recording == true)
        {
            if (!PauseMenu.paused && Time.time > lastFrameTime + (1 / RecordingFPS))
            {
                lastFrameTime = Time.time;
                allPoints.Add(RecordTarget.position);
                allTimes.Add(Time.time - startTime);
                allRots.Add(RecordTarget.rotation);
                endTime = Time.time;
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, -3, 100, 50), "Game Time");
        GUI.Label(new Rect(0, 10, 100, 50), TimeString(Time.time));

        GUI.Label(new Rect(80, -3, 100, 50), "Recording Time");
        GUI.Label(new Rect(80, 10, 100, 50), TimeString(endTime - startTime));
    }

    public void Save()
    {
        string path = "/" + GhostName.Replace(" ", "_") + ".pth";
        if (CustomPath == "")
            path = Application.persistentDataPath + path;
        else
            path = CustomPath + path;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path);

        GhostData data = new GhostData();

        //Add variables here.
        data.PlayerName = GhostName;

        data.Path = new List<SerialPoint>();
        foreach (Vector3 p in allPoints)
            data.Path.Add(new SerialPoint(p));

        data.Rotations = new List<SerialRot>();
        foreach (Quaternion r in allRots)
            data.Rotations.Add(new SerialRot(r));

        data.Times = allTimes;

        bf.Serialize(file, data);

        file.Close();
        float timeInS = allTimes[allTimes.Count - 1];
        print("Saved recording to '" + path + "' with time: " + TimeString(timeInS) + " (" + timeInS + "s)");
    }

    public static string TimeString (float timeInSeconds)
    {
        float allTime = timeInSeconds;
        float miliseconds = allTime - Mathf.Floor(allTime);
        miliseconds = Mathf.Floor(miliseconds * 100f);
        float minutes = Mathf.Floor(allTime / 60f);
        float seconds = Mathf.Floor(allTime - (minutes * 60));
        float hours = Mathf.Floor(minutes / 60f);
        minutes = minutes % 60f;

        string sMili;
        if (miliseconds == 0)
            sMili = "00";
        else if (miliseconds < 10f)
            sMili = "0" + miliseconds;
        else
            sMili = miliseconds.ToString();

        string sSeconds;
        if (seconds == 0)
            sSeconds = "00";
        else if (seconds < 10f)
            sSeconds = "0" + seconds;
        else
            sSeconds = seconds.ToString();

        string time = sSeconds + ":" + sMili;

        if (minutes == 0f)
            time = "00" + ":" + time;
        else if (minutes < 10f)
            time = "0" + minutes + ":" + time;
        else
            time = minutes + ":" + time;

        if (hours != 0f)
            time = hours + ":" + time;


        return time;
    }
}

[System.Serializable]
public class SerialPoint
{
    public float _x;
    public float _y;
    public float _z;

    public SerialPoint(float x, float y, float z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public SerialPoint(Vector3 V3)
    {
        _x = V3.x;
        _y = V3.y;
        _z = V3.z;
    }

    public Vector3 ToV3()
    {
        return new Vector3(_x, _y, _z);
    }
}

[System.Serializable]
public class SerialRot
{
    public float _w;
    public float _x;
    public float _y;
    public float _z;

    public SerialRot(float x, float y, float z, float w)
    {
        _w = w;
        _x = x;
        _y = y;
        _z = z;
    }

    public SerialRot(Quaternion Rot)
    {
        _w = Rot.w;
        _x = Rot.x;
        _y = Rot.y;
        _z = Rot.z;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(_x, _y, _z, _w);
    }
}

[System.Serializable]
public class GhostData
{
    public string PlayerName;
    public List<SerialPoint> Path;
    public List<SerialRot> Rotations;
    public List<float> Times;
}
