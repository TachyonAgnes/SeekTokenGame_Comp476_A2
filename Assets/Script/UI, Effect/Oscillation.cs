using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oscillation is for topdown items like lights and cameras perform a realistic rotation.
/// </summary>
public class Oscillation : MonoBehaviour {
    [SerializeField] 
    public float shakeIntensity = 5.0f;
    public float frequency = 0.1f;
    public bool isXRotation = false;
    public bool isYRotation = false;

    private Quaternion originalRotation;
    private float elapsedTime;

    private void Start() {
        originalRotation = transform.rotation;
    }


    private void Update() {
        elapsedTime += Time.deltaTime;
        float xRotation = 0;
        float yRotation = 0;
        if (isXRotation) {
            xRotation = Mathf.Sin(elapsedTime * frequency) * shakeIntensity;
        }
        if (isYRotation) {
            yRotation = Mathf.Cos(elapsedTime * frequency) * shakeIntensity;
        }
        transform.localRotation = originalRotation * Quaternion.Euler(xRotation, yRotation, 0);
    }
}