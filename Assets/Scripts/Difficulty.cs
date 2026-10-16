// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// The selected difficulty (Easy, Normal or Hard) and the multipliers it applies to enemies, pickups and score.
/// The choice is saved on the device.
/// </summary>
public static class Difficulty
{
    public const int Easy = 0;
    public const int Normal = 1;
    public const int Hard = 2;

    private const string Key = "Difficulty";

    private static int _level = -1;

    public static int Level
    {
        get
        {
            if (_level < 0)
            {
                _level = Mathf.Clamp(PlayerPrefs.GetInt(Key, Normal), Easy, Hard);
            }
            return _level;
        }
        set
        {
            _level = Mathf.Clamp(value, Easy, Hard);
            PlayerPrefs.SetInt(Key, _level);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Multiplier for enemy movement speed.</summary>
    public static float EnemySpeed { get { return Pick(0.85f, 1f, 1.15f); } }

    /// <summary>Multiplier for enemy fire delay (higher means they shoot less often).</summary>
    public static float FireDelay { get { return Pick(1.4f, 1f, 0.8f); } }

    /// <summary>Multiplier for the time between enemy spawns.</summary>
    public static float SpawnInterval { get { return Pick(1.25f, 1f, 0.8f); } }

    /// <summary>Extra hit points added to every enemy.</summary>
    public static int ExtraHitPoints { get { return Level == Hard ? 1 : 0; } }

    /// <summary>Multiplier for the chance that a destroyed enemy drops a pickup.</summary>
    public static float PickupChance { get { return Pick(1.5f, 1f, 0.7f); } }

    /// <summary>Multiplier for points earned, so harder runs score higher.</summary>
    public static float ScoreMultiplier { get { return Pick(1f, 1f, 1.5f); } }

    private static float Pick(float easy, float normal, float hard)
    {
        switch (Level)
        {
            case Easy: return easy;
            case Hard: return hard;
            default: return normal;
        }
    }
}
