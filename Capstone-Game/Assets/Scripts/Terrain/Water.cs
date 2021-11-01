using UnityEngine;

public class Water : MonoBehaviour
{
    public AnimationCurve heightCurve;
    public float noiseAmplitude = 1.0f;
    public float noiseFrequency = 1.0f;

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * noiseFrequency, 0.0f) * noiseAmplitude;
        transform.localPosition = new Vector3(0, heightCurve.Evaluate(Time.time) + noise, 0);
    }
}
