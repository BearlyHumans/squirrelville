using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraTransition : MonoBehaviour
{
    public Camera playerCamera;
    [Header("Will search for child transforms to use if empty. Blue arrows in local are where camera points.")]
    public List<Transform> transitionPoints = new List<Transform>();
    [Tooltip("The transition starts as soon as the script becomes active (or the scene loads).")]
    public bool playOnStart = false;
    private Vector3 startingPoint = Vector3.zero;
    private Quaternion startingRot = new Quaternion();
    private bool doingTransition = false;

    [Min(0.01f)]
    [Tooltip("Used to calculate the maximum speed between two points (but only if it is less than the overall max speed below).")]
    public float avgTimeBetweenPoints = 2f;
    [Tooltip("How quickly the camera changes speed.")]
    public float acceleration = 1f;
    [Tooltip("The absolute maximum speed of the camera.")]
    public float maxSpeed = 0f;
    private float cameraMoveSpeed = 0f;

    [Space()]
    [Tooltip("If true the camera will teleport to the end-point when the transition starts.")]
    public bool skipMoveToPoint = false;
    [Tooltip("If true the camera will cut to black instead of fading to black.")]
    public bool skipFadeOut = false;
    [Tooltip("If true the camera will cut out of black instead of fading out of black.")]
    public bool skipFadeIn = false;
    [Tooltip("If true the camera will teleport back to the player after the fade instead of moving back.")]
    public bool skipMoveBack = false;

    [Space()]
    [Tooltip("The duration of both fade effects")]
    public float fadeDuration = 2f;
    [Tooltip("The duration of the screen being black")]
    public float blackScreenDuration = 0f;
    [Tooltip("This will be added to the fade out time for the script but not for the actual fade, so you can use it to make the movement start before the fade finishes by setting it to negative")]
    public float fadeMargins = 0f;

    [Space()]
    public bool testTransition = false;
    public bool loadPointsOnStart = false;

    [Space()]
    public UnityEvent transitionStart;
    public UnityEvent screenIsBlack;
    public UnityEvent transitionEnd;

    [Header("Rarely need changing:")]
    public float startingSpeed = 0f;
    public float errorMargin = 0.1f;

    private Vector3 Pos
    {
        get { return playerCamera.transform.position; }
        set { playerCamera.transform.position = value; }
    }

    private Quaternion Rot
    {
        get { return playerCamera.transform.rotation; }
        set { playerCamera.transform.rotation = value; }
    }

    private void Start()
    {
        if (loadPointsOnStart)
        {
            transitionPoints.Clear();
            transitionPoints.AddRange(GetComponentsInChildren<Transform>());
            transitionPoints.RemoveAt(0);
        }

        if (playOnStart)
        {
            StartTransition();
        }
    }

    void Update()
    {
        if (testTransition)
        {
            testTransition = false;
            StartTransition();
        }
    }

    public void StartTransition()
    {
        print("Transition Called");
        if (doingTransition)
            return;

        doingTransition = true;

        if (transitionPoints.Count == 0)
        {
            transitionPoints.AddRange(GetComponentsInChildren<Transform>());
            transitionPoints.RemoveAt(0);
        }

        if (transitionPoints.Count == 0)
            transitionPoints.Add(transform);

        startingPoint = playerCamera.transform.position;
        startingRot = playerCamera.transform.rotation;
        cameraMoveSpeed = startingSpeed;

        transitionStart?.Invoke();

        print("Starting Transition");
        StartCoroutine(MoveCameraThoughPoints());
    }

    private IEnumerator MoveCameraThoughPoints()
    {
        print("Starting Move");
        if (skipMoveToPoint)
        {
            Pos = transitionPoints[transitionPoints.Count - 1].position;
            Rot = transitionPoints[transitionPoints.Count - 1].rotation;
            StartCoroutine(FadeOut());
            yield break;
        }
        else
        {
            float desiredMoveSpeed;
            for (int nextPoint = 0; nextPoint < transitionPoints.Count; ++nextPoint)
            {
                Vector3 nextPos = transitionPoints[nextPoint].position;
                Quaternion nextRot = transitionPoints[nextPoint].rotation;

                float remainingDist = Vector3.Distance(Pos, nextPos);
                desiredMoveSpeed = remainingDist / avgTimeBetweenPoints;

                float movementThisFrame;
                print("dist = " + remainingDist);
                while (remainingDist > errorMargin)
                {
                    remainingDist = Vector3.Distance(Pos, nextPos);
                    movementThisFrame = cameraMoveSpeed * Time.deltaTime;
                    Pos = Vector3.MoveTowards(Pos, nextPos, movementThisFrame);
                    Rot = Quaternion.Lerp(Rot, nextRot, movementThisFrame / remainingDist);

                    cameraMoveSpeed = Mathf.Min(maxSpeed, Mathf.MoveTowards(cameraMoveSpeed, desiredMoveSpeed, acceleration * Time.deltaTime));
                    yield return new WaitForEndOfFrame();
                }
            }

            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        print("Starting Fade Out");

        if (skipFadeOut)
            FadeToBlack.singleton.BecomeOpaque();
        else
        {
            yield return new WaitForSeconds(fadeMargins);
            FadeToBlack.singleton.BecomeOpaque(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        yield return new WaitForSeconds(blackScreenDuration);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        print("Starting Fade In");
        screenIsBlack?.Invoke();
        if (skipFadeIn)
            FadeToBlack.singleton.BecomeTransparent();
        else
        {
            FadeToBlack.singleton.BecomeTransparent(fadeDuration);
            yield return new WaitForSeconds(fadeDuration + fadeMargins);
        }

        StartCoroutine(MoveCameraThoughPointsBackwards());
    }

    private IEnumerator MoveCameraThoughPointsBackwards()
    {
        print("Starting Move Back");
        if (skipMoveBack)
        {
            Pos = startingPoint;
            Rot = startingRot;
            transitionEnd?.Invoke();
            doingTransition = false;
            yield break;
        }
        else
        {
            cameraMoveSpeed = startingSpeed;
            float desiredMoveSpeed;
            float specialAccel = acceleration;
            for (int nextPoint = transitionPoints.Count; nextPoint >= 0; --nextPoint)
            {
                Vector3 nextPos;
                Quaternion nextRot;
                float remainingDist;
                if (nextPoint > 0)
                {
                    nextPos = transitionPoints[nextPoint - 1].position;
                    nextRot = transitionPoints[nextPoint - 1].rotation;

                    remainingDist = Vector3.Distance(Pos, nextPos);
                    desiredMoveSpeed = remainingDist / avgTimeBetweenPoints;
                }
                else
                {
                    nextPos = startingPoint;
                    nextRot = startingRot;

                    remainingDist = Vector3.Distance(Pos, nextPos);
                    float lastPointDuration = (remainingDist * 2) / cameraMoveSpeed;
                    specialAccel = cameraMoveSpeed / lastPointDuration;
                    desiredMoveSpeed = 0;
                }

                float movementThisFrame;
                while (remainingDist > errorMargin)
                {
                    remainingDist = Vector3.Distance(Pos, nextPos);
                    movementThisFrame = cameraMoveSpeed * Time.deltaTime;
                    Pos = Vector3.MoveTowards(Pos, nextPos, movementThisFrame);
                    Rot = Quaternion.Lerp(Rot, nextRot, movementThisFrame / remainingDist);

                    cameraMoveSpeed = Mathf.Min(maxSpeed, Mathf.MoveTowards(cameraMoveSpeed, desiredMoveSpeed, specialAccel * Time.deltaTime));

                    if (cameraMoveSpeed <= 0)
                        Pos = nextPos;
                    yield return new WaitForEndOfFrame();
                }
            }
            
            Pos = startingPoint;
            Rot = startingRot;
            transitionEnd?.Invoke();
            doingTransition = false;
        }
    }

    private void OnDrawGizmosSelected()
    {

        Transform lastPoint = null;
        foreach (Transform point in transitionPoints)
        {
            if (lastPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(lastPoint.position, point.position);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(point.position, point.position + point.forward);
            }
            lastPoint = point;
        }
    }
}