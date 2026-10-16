// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

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
        return Input.GetAxis("Shoot") > 0 || Input.touchCount > 0;
    }

    /// <summary>
    /// Returns -1, 0 or 1 per axis pointing from the ship to the touch point on the play field.
    /// </summary>
    private static Vector3 TouchDirection(Vector3 shipPosition)
    {
        Camera cam = Camera.main;
        if (Input.touchCount == 0 || cam == null)
        {
            return Vector3.zero;
        }

        Vector2 screenPoint = Input.GetTouch(0).position + Vector2.up * (Screen.height * FingerOffset);
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
