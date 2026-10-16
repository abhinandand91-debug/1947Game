// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

/// <summary>
/// Briefly tints every sprite on a game object red to show it took damage.
/// </summary>
public class HitFlash : MonoBehaviour
{
    private const float Duration = 0.12f;

    private SpriteRenderer[] _renderers;
    private Color[] _original;
    private float _timer;

    /// <summary>
    /// Plays the flash on the target, adding the component the first time it is needed.
    /// </summary>
    public static void Play(GameObject target)
    {
        HitFlash flash = target.GetComponent<HitFlash>();
        if (flash == null)
        {
            flash = target.AddComponent<HitFlash>();
        }
        flash.Begin();
    }

    private void Begin()
    {
        if (_renderers == null)
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _original = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _original[i] = _renderers[i].color;
            }
        }

        _timer = Duration;
        foreach (SpriteRenderer sprite in _renderers)
        {
            sprite.color = new Color(1f, 0.35f, 0.35f, sprite.color.a);
        }
    }

    private void Update()
    {
        if (_timer <= 0f)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Restore();
        }
    }

    private void OnDisable()
    {
        // The player is toggled off when the game ends, so never leave it tinted
        if (_timer > 0f)
        {
            _timer = 0f;
            Restore();
        }
    }

    private void Restore()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].color = _original[i];
            }
        }
    }
}
