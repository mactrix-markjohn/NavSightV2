using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcesuralSoundTester : MonoBehaviour
{
    public float frequency = 440f;
    public float gain = 0.05f;
    public float ringRadius = 5f;
    public int numSamples = 1024;

    private float[] samples;
    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        samples = new float[numSamples];
    }

    private void Update() {
        for (int i = 0; i < numSamples; i++) {
            float angle = (2f * Mathf.PI * i) / numSamples;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * Time.time + (x + z)) * gain;
        }
        audioSource.clip.SetData(samples, 0);
    }
}
