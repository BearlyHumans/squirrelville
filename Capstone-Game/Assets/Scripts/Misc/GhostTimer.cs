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
    private List<float> allTimes = new List<float>();
    [Min(1)]
    public float RecordingFPS = 10;
    [Header("True to Start, False to Save (Toggle W/ Backspace)")]
    public bool Record = false;
    private bool recording = false;

    private float lastFrameTime = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
            }
        }

        if (RecordTarget != null && recording == true)
        {
            if (!PauseMenu.paused && Time.time > lastFrameTime + (1 / RecordingFPS))
            {
                lastFrameTime = Time.time;
                allPoints.Add(RecordTarget.position);
                allTimes.Add(Time.time);
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 50), TimeString);
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + GhostName.Replace(" ", "_") + ".pth");

        GhostData data = new GhostData();

        //Add variables here.
        data.PlayerName = GhostName;
        data.Path = new List<SerialPoint>();
        foreach (Vector3 p in allPoints)
        {
            data.Path.Add(new SerialPoint(p));
        }
        data.Times = allTimes;

        bf.Serialize(file, data);

        file.Close();
    }

    private string TimeString
    {
        get
        {
            float allTime = Time.time;
            float miliseconds = allTime - Mathf.Floor(allTime);
            miliseconds = Mathf.Floor(miliseconds * 100f);
            float minutes = Mathf.Floor(allTime / 60f);
            float seconds = Mathf.Floor(allTime - minutes);
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
public class GhostData
{
    public string PlayerName;
    public List<SerialPoint> Path;
    public List<float> Times;
}
