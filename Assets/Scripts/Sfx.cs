// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays sound effects loaded from Resources/Sfx by name, e.g. Sfx.Play("explosion_small").
/// </summary>
public static class Sfx
{
    private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
    private static AudioSource _source;

    /// <summary>
    /// Plays a one-shot sound with a slight random pitch so repeated sounds do not feel mechanical.
    /// </summary>
    public static void Play(string clipName, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null)
        {
            return;
        }

        AudioSource source = GetSource();
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clip, volume);
    }

    private static AudioClip GetClip(string clipName)
    {
        AudioClip clip;
        if (!Clips.TryGetValue(clipName, out clip))
        {
            clip = Resources.Load<AudioClip>("Sfx/" + clipName);
            Clips[clipName] = clip;
        }
        return clip;
    }

    private static AudioSource GetSource()
    {
        if (_source == null)
        {
            GameObject go = new GameObject("Sfx");
            Object.DontDestroyOnLoad(go);
            _source = go.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }
        return _source;
    }
}
