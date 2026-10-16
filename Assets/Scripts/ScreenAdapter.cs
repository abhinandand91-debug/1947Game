// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Adapts the game to the device: keeps the whole play field visible on any aspect ratio,
/// caps the frame rate and stops the screen from dimming while playing.
/// </summary>
public class ScreenAdapter : MonoBehaviour
{
    private const float DesignHalfWidth = 6.67f; // half of the 4:3 play field at orthographic size 5
    private const float MinOrthographicSize = 5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (Camera.main != null && Camera.main.GetComponent<ScreenAdapter>() == null)
        {
            Camera.main.gameObject.AddComponent<ScreenAdapter>();
        }
    }

    private void Start()
    {
        Fit();
    }

    private void Update()
    {
        // Handles rotation and window resizing
        Fit();
    }

    private void Fit()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        float sizeToFitWidth = DesignHalfWidth / cam.aspect;
        cam.orthographicSize = Mathf.Max(MinOrthographicSize, sizeToFitWidth);
    }
}
