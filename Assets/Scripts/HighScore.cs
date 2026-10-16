// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Stores the best score on the device using PlayerPrefs.
/// </summary>
public static class HighScore
{
    private const string Key = "HighScore";

    public static int Best
    {
        get { return PlayerPrefs.GetInt(Key, 0); }
    }

    /// <summary>
    /// Saves the score if it beats the current best.
    /// </summary>
    /// <returns>True when a new best score was set</returns>
    public static bool Submit(int score)
    {
        if (score <= Best)
        {
            return false;
        }

        PlayerPrefs.SetInt(Key, score);
        PlayerPrefs.Save();
        return true;
    }
}
