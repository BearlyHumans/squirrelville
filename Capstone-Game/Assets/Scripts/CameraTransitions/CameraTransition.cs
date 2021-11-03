using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraTransition : MonoBehaviour
{
    public Camera playerCamera;
    [Header("Will search for child transforms to use if empty. Blue arrows in local are where camera points.")]
    public List<Transform> transitionPoints = new List<Transform>();
    public bool playOnStart = false;
    public bool startBlackIfSkipping = true;
    private Vector3 startingPoint = Vector3.zero;
    private Quaternion startingRot = new Quaternion();
    private bool doingTransition = false;

    [Min(0.01f)]
    public float avgTimeBetweenPoints = 2f;
    public float acceleration = 1f;
    public float startingSpeed = 0f;
    public float maxSpeed = 0f;
    public float errorMargin = 0.1f;
    public float cameraMoveSpeed = 0f;

    [Space()]
    public bool skipMoveToPoint = false;
    public bool skipFadeOut = false;
    public bool skipFadeIn = false;
    public bool skipMoveBack = false;

    [Space()]
    public float fadeDuration = 2f;
    public float fadeMargins = 0f;

    [Space()]
    public bool testTransition = false;
    public bool loadPoints = false;

    [Space()]
    public UnityEvent transitionStart;
    public UnityEvent screenIsBlack;
    public UnityEvent transitionEnd;

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
        if (playOnStart)
        {
            StartTransition();
            if (skipMoveToPoint && startBlackIfSkipping)
                FadeToBlack.singleton.BecomeOpaque();
        }
    }

    void Update()
    {
        if (testTransition)
        {
            testTransition = false;
            StartTransition();
        }

        if (loadPoints)
        {
            transitionPoints.Clear();
            transitionPoints.AddRange(GetComponentsInChildren<Transform>());
            transitionPoints.RemoveAt(0);
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