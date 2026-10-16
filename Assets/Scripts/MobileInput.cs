// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Combines keyboard input with touch input so the ship can be flown on desktop and mobile.
/// Touch: the ship steers towards the finger and fires while the screen is held.
/// </summary>
public static class MobileInput
{
    private const float DeadZone = 0.25f;     // world units before the ship starts steering
    private const float FingerOffset = 0.12f; // fraction of screen height the target sits above the finger

    public static float Horizontal(Vector3 shipPosition)
    {
        float keyboard = Input.GetAxis("Horizontal");
        return Mathf.Abs(keyboard) > 0 ? keyboard : TouchDirection(shipPosition).x;
    }

    public static float Vertical(Vector3 shipPosition)
    {
        float keyboard = Input.GetAxis("Vertical");
        return Mathf.Abs(keyboard) > 0 ? keyboard : TouchDirection(shipPosition).z;
    }

    public static bool Shoot()
    {
        if (Input.GetAxis("Shoot") > 0)
        {
            return true;
        }

        // Touch: fire while the screen is held (auto-fire) or while the on-screen fire button is held
        Touch touch;
        return GameSettings.AutoFire ? TryGetGameTouch(out touch) : MobileControls.FireHeld;
    }

    /// <summary>
    /// Finds a finger that is on the play field rather than on a UI button.
    /// </summary>
    private static bool TryGetGameTouch(out Touch result)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                continue;
            }
            result = touch;
            return true;
        }

        result = default(Touch);
        return false;
    }

    /// <summary>
    /// Returns -1, 0 or 1 per axis pointing from the ship to the touch point on the play field.
    /// </summary>
    private static Vector3 TouchDirection(Vector3 shipPosition)
    {
        Camera cam = Camera.main;
        Touch touch;
        if (cam == null || !TryGetGameTouch(out touch))
        {
            return Vector3.zero;
        }

        Vector2 screenPoint = touch.position + Vector2.up * (Screen.height * FingerOffset);
        Ray ray = cam.ScreenPointToRay(screenPoint);
        Plane playField = new Plane(Vector3.up, shipPosition);

        float distance;
        if (!playField.Raycast(ray, out distance))
        {
            return Vector3.zero;
        }

        Vector3 delta = ray.GetPoint(distance) - shipPosition;
        return new Vector3(
            Mathf.Abs(delta.x) > DeadZone ? Mathf.Sign(delta.x) : 0,
            0,
            Mathf.Abs(delta.z) > DeadZone ? Mathf.Sign(delta.z) : 0);
    }
}
