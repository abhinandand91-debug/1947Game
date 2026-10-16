// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Shakes the main camera for impact feedback. Call CameraShake.Shake from anywhere.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake _instance;

    private float _magnitude;
    private float _duration;
    private float _elapsed;
    private Vector3 _lastOffset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<CameraShake>() == null)
        {
            cam.gameObject.AddComponent<CameraShake>();
        }
    }

    /// <summary>
    /// Starts a shake. A weaker shake never interrupts a stronger one that is still running.
    /// </summary>
    /// <param name="magnitude">Maximum offset in world units</param>
    /// <param name="duration">Length of the shake in seconds</param>
    public static void Shake(float magnitude, float duration)
    {
        if (_instance == null)
        {
            return;
        }

        float remaining = _instance._magnitude * (1f - _instance._elapsed / Mathf.Max(_instance._duration, 0.0001f));
        if (_instance._elapsed < _instance._duration && remaining > magnitude)
        {
            return;
        }

        _instance._magnitude = magnitude;
        _instance._duration = duration;
        _instance._elapsed = 0f;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void LateUpdate()
    {
        // Remove last frame's offset so other scripts can still move the camera
        Vector3 basePosition = transform.position - _lastOffset;
        _lastOffset = Vector3.zero;

        if (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(_elapsed / _duration);
            Vector2 random = Random.insideUnitCircle * _magnitude * fade;
            _lastOffset = transform.right * random.x + transform.up * random.y;
        }

        transform.position = basePosition + _lastOffset;
    }
}
