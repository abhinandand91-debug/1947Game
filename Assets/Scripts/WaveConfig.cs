// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Describes one wave of enemies: how tough they are, how often they spawn and how many kills clear the wave.
/// </summary>
public class WaveDefinition
{
    public string Name;
    public int KillsToAdvance;
    public float SpawnMin;
    public float SpawnMax;
    public int HitPoints;
    public float Speed;
    public float FireDelay;
    public Color Tint;
    public bool IsBoss;
    public float ChaserChance;   // chance an enemy is a kamikaze chaser
    public float StraferChance;  // chance an enemy is a fast side-to-side strafer
    public float FormationChance; // chance a spawn is a whole formation
}

/// <summary>
/// The difficulty curve: Rookies (easy), Fighters (medium), Aces (hard), then a Boss. After the boss the
/// cycle starts again with tougher enemies, so the game is endless but always ramps back up.
/// </summary>
public static class WaveConfig
{
    public const int WavesPerRound = 4;

    private static readonly int[] WeaponKillThresholds = { 0, 4, 10, 18, 28 };

    /// <summary>
    /// Returns the definition of the given wave (0 is the first wave).
    /// </summary>
    public static WaveDefinition Get(int wave)
    {
        int round = wave / WavesPerRound;
        float spawnScale = Mathf.Max(0.6f, 1f - 0.1f * round);
        float fireScale = Mathf.Max(0.6f, 1f - 0.1f * round);

        WaveDefinition def = new WaveDefinition();
        switch (wave % WavesPerRound)
        {
            case 0: // Easy
                def.Name = "ROOKIES";
                def.KillsToAdvance = 6;
                def.SpawnMin = 3f;
                def.SpawnMax = 5f;
                def.HitPoints = 1;
                def.Speed = 2.5f;
                def.FireDelay = 2f;
                def.Tint = new Color(0.65f, 1f, 0.65f);
                break;
            case 1: // Medium
                def.Name = "FIGHTERS";
                def.StraferChance = 0.3f;
                def.FormationChance = 0.2f;
                def.KillsToAdvance = 10;
                def.SpawnMin = 2.5f;
                def.SpawnMax = 4f;
                def.HitPoints = 2;
                def.Speed = 3.5f;
                def.FireDelay = 1.3f;
                def.Tint = new Color(1f, 0.92f, 0.55f);
                break;
            case 2: // Hard
                def.Name = "ACES";
                def.ChaserChance = 0.25f;
                def.StraferChance = 0.25f;
                def.FormationChance = 0.35f;
                def.KillsToAdvance = 14;
                def.SpawnMin = 2f;
                def.SpawnMax = 3.5f;
                def.HitPoints = 3;
                def.Speed = 4.5f;
                def.FireDelay = 0.9f;
                def.Tint = new Color(1f, 0.55f, 0.45f);
                break;
            default: // Boss
                def.Name = "BOSS";
                def.KillsToAdvance = 1;
                def.SpawnMin = 5f;
                def.SpawnMax = 8f; // escorts while the boss is alive
                def.HitPoints = 25;
                def.Speed = 2.5f;
                def.FireDelay = 1.4f;
                def.Tint = new Color(0.85f, 0.55f, 1f);
                def.IsBoss = true;
                break;
        }

        // Each time the cycle repeats, enemies get tougher and fire faster
        def.KillsToAdvance += 2 * round;
        def.SpawnMin *= spawnScale;
        def.SpawnMax *= spawnScale;
        def.HitPoints += def.IsBoss ? 10 * round : round;
        def.Speed = Mathf.Min(8f, def.Speed + 0.3f * round);
        def.FireDelay *= fireScale;

        // The difficulty chosen on the main menu
        def.SpawnMin *= Difficulty.SpawnInterval;
        def.SpawnMax *= Difficulty.SpawnInterval;
        def.Speed = Mathf.Min(8f, def.Speed * Difficulty.EnemySpeed);
        def.FireDelay *= Difficulty.FireDelay;
        def.HitPoints += Difficulty.ExtraHitPoints;
        return def;
    }

    /// <summary>
    /// The regular enemies that accompany a boss (the first wave of the same round).
    /// </summary>
    public static WaveDefinition Escort(int wave)
    {
        return Get(wave - wave % WavesPerRound);
    }

    /// <summary>
    /// Player weapon level (1-5) earned after the given number of kills.
    /// </summary>
    public static int WeaponLevelForKills(int kills)
    {
        int level = 1;
        for (int i = 1; i < WeaponKillThresholds.Length; i++)
        {
            if (kills >= WeaponKillThresholds[i])
            {
                level = i + 1;
            }
        }
        return level;
    }
}
