// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;

public enum PickupType
{
    Heart,
    Shield,
    Bomb
}

/// <summary>
/// A collectable that falls down the screen after an enemy is destroyed.
/// Heart restores health, Shield blocks the next hit and Bomb destroys everything on screen.
/// </summary>
public class Pickup : MonoBehaviour
{
    private const float FallSpeed = 1.8f;
    private const float PickupRadius = 0.9f;
    private const float Lifetime = 12f;
    private const float BaseDropChance = 0.12f;

    public PickupType Type;

    private Player _player;
    private SpriteRenderer _renderer;
    private float _age;

    /// <summary>
    /// Rolls for a drop at the given position. Heart is the most common, Bomb the rarest.
    /// </summary>
    public static void TryDrop(Vector3 position)
    {
        if (Random.value > BaseDropChance * Difficulty.PickupChance)
        {
            return;
        }

        float roll = Random.value;
        PickupType type = roll < 0.4f ? PickupType.Heart : (roll < 0.75f ? PickupType.Shield : PickupType.Bomb);
        Spawn(position, type);
    }

    public static void Spawn(Vector3 position, PickupType type)
    {
        GameObject go = new GameObject("Pickup_" + type);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.9f;

        // Face the camera so the sprite is readable from the top-down view
        Camera cam = Camera.main;
        if (cam != null)
        {
            go.transform.rotation = cam.transform.rotation;
        }

        SpriteRenderer sprite = go.AddComponent<SpriteRenderer>();
        sprite.sortingOrder = 8;
        switch (type)
        {
            case PickupType.Heart: sprite.sprite = PickupSprites.Heart; break;
            case PickupType.Shield: sprite.sprite = PickupSprites.Shield; break;
            default: sprite.sprite = PickupSprites.Bomb; break;
        }

        go.AddComponent<Pickup>().Type = type;
    }

    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _age += Time.deltaTime;

        // Pickups vanish when the run ends (the player is switched off) or after a while
        if (_age > Lifetime || _player == null || !_player.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += Vector3.back * FallSpeed * Time.deltaTime;

        // Gentle pulse, and blink during the last few seconds
        transform.localScale = Vector3.one * (0.9f + 0.1f * Mathf.Sin(_age * 6f));
        _renderer.enabled = _age < Lifetime - 3f || Mathf.Repeat(_age, 0.3f) > 0.15f;

        Vector3 toPlayer = _player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.magnitude < PickupRadius)
        {
            Collect();
        }
    }

    private void Collect()
    {
        Vector3 position = transform.position;
        Sfx.Play("pickup");

        switch (Type)
        {
            case PickupType.Heart:
                CollectHeart(position);
                break;
            case PickupType.Shield:
                _player.ActivateShield(8f);
                ScreenFx.Popup(position, "SHIELD", new Color(0.5f, 0.85f, 1f), 1.1f);
                break;
            default:
                Detonate();
                break;
        }

        Destroy(gameObject);
    }

    private void CollectHeart(Vector3 position)
    {
        if (_player.Heal())
        {
            UserInterfaceManager ui = FindObjectOfType<UserInterfaceManager>();
            if (ui != null)
            {
                ui.RestoreHeart();
            }
            ScreenFx.Popup(position, "+1 HEART", new Color(1f, 0.4f, 0.5f), 1.1f);
        }
        else
        {
            // Already at full health, so the heart turns into a shield instead
            _player.ActivateShield(8f);
            ScreenFx.Popup(position, "SHIELD", new Color(0.5f, 0.85f, 1f), 1.1f);
        }
    }

    /// <summary>
    /// Destroys every enemy on screen (the boss only takes heavy damage) and clears enemy lasers.
    /// </summary>
    private void Detonate()
    {
        ScreenFx.Flash(Color.white, 0.8f);
        CameraShake.Shake(0.5f, 0.5f);
        Sfx.Play("explosion_big");

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.CalculateHit(enemy.IsBoss ? 5 : 999);
        }

        foreach (Laser laser in FindObjectsOfType<Laser>())
        {
            if (laser.EnemyWeapon)
            {
                Destroy(laser.gameObject);
            }
        }
    }
}
