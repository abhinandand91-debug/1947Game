// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Player preferences saved on the device: auto-fire and vibration.
/// </summary>
public static class GameSettings
{
    private const string AutoFireKey = "AutoFire";
    private const string VibrationKey = "Vibration";

    /// <summary>When on, touching the screen both steers and fires. When off, a fire button is shown on touch devices.</summary>
    public static bool AutoFire
    {
        get { return PlayerPrefs.GetInt(AutoFireKey, 1) == 1; }
        set { Save(AutoFireKey, value); }
    }

    public static bool Vibration
    {
        get { return PlayerPrefs.GetInt(VibrationKey, 1) == 1; }
        set { Save(VibrationKey, value); }
    }

    private static void Save(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
