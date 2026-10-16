// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-06

using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player specific behaviors such as moving, shooting and taking damage
/// </summary>
public class Player : Ship
{


    public float BackgroundMultiplyer = 0.008f;
    public UserInterfaceManager UserInterface;

    // Update is called once per frame
    void Update()
    {
	
        if (MobileInput.Horizontal(transform.position) < 0) //Move left
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.left, Speed * Time.deltaTime);
            EnvironmentMngr.SetMovingHorizontalMultiplyer(-BackgroundMultiplyer);
        }
        else if (MobileInput.Horizontal(transform.position) > 0) //Move Right
        {
            transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.left, Speed * Time.deltaTime);
            EnvironmentMngr.SetMovingHorizontalMultiplyer(BackgroundMultiplyer);
        }
        else
        {
            EnvironmentMngr.SetMovingHorizontalMultiplyer(0);
        }

        if (MobileInput.Vertical(transform.position) > 0) //Move up
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.forward, Speed * Time.deltaTime);
            EnvironmentMngr.SetMovingVerticallyMultiplyer(BackgroundMultiplyer);

        }
        else if (MobileInput.Vertical(transform.position) < 0) //Move down
        {
            transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.forward, Speed * Time.deltaTime);
            EnvironmentMngr.SetMovingVerticallyMultiplyer(-BackgroundMultiplyer);
        }
        else
        {
            EnvironmentMngr.SetMovingVerticallyMultiplyer(0);
        }

        if (MobileInput.Shoot() && CanShoot)
        {
            CanShoot = false;

            FireWeapon();
        }
    }


    public int WeaponLevel { get; private set; }

    private static readonly float[] FireDelayScale = { 1f, 0.8f, 0.8f, 0.8f, 0.65f };

    private float _baseFireDelay;
    private Vector3 _startPosition;

    void Awake()
    {
        _startPosition = transform.position;
    }

    void OnEnable()
    {
        // A pending cooldown is cancelled when the player is disabled, so reset for every new run
        transform.position = _startPosition;
        CanShoot = true;
    }

    /// <summary>
    /// Sets the weapon level (1-5). Higher levels shoot faster and add more lasers.
    /// </summary>
    public void SetWeaponLevel(int level, bool announce = true)
    {
        level = Mathf.Clamp(level, 1, FireDelayScale.Length);
        if (_baseFireDelay <= 0f)
        {
            _baseFireDelay = FireDelay;
        }

        int previous = WeaponLevel;
        bool upgraded = level > previous;
        WeaponLevel = level;
        FireDelay = _baseFireDelay * FireDelayScale[level - 1];

        if (!announce)
        {
            return;
        }

        if (previous > 0 && level < previous)
        {
            ScreenFx.Popup(transform.position, "WEAPON LV " + level, new Color(1f, 0.4f, 0.4f), 1.2f);
        }
        else if (upgraded && level > 1)
        {
            Sfx.Play("weapon_up");
            ScreenFx.Popup(transform.position, "WEAPON LV " + level, new Color(0.5f, 0.9f, 1f), 1.2f);
        }
    }

    /// <summary>
    /// Fires the pattern for the current weapon level: single, double, triple spread or five-way.
    /// </summary>
    private void FireWeapon()
    {
        switch (WeaponLevel)
        {
            case 3:
                FireLaser(0f, -0.3f);
                FireLaser(0f, 0.3f);
                break;
            case 4:
                FireLaser(0f, 0f);
                FireLaser(-1.5f, -0.2f);
                FireLaser(1.5f, 0.2f);
                break;
            case 5:
                FireLaser(0f, 0f);
                FireLaser(0f, -0.4f);
                FireLaser(0f, 0.4f);
                FireLaser(-2.5f, -0.5f);
                FireLaser(2.5f, 0.5f);
                break;
            default:
                FireLaser(0f, 0f);
                break;
        }
        StartCooldown();
    }

    // Kills needed at each level to reach the next one (level 1 to 2, 2 to 3, ...)
    private static readonly int[] KillsPerUpgrade = { 4, 6, 8, 10 };

    private int _killsAtLevel;

    /// <summary>
    /// Counts a kill towards the next weapon upgrade.
    /// </summary>
    public void RegisterKill()
    {
        if (WeaponLevel < 1 || WeaponLevel >= FireDelayScale.Length)
        {
            return;
        }

        _killsAtLevel++;
        if (_killsAtLevel >= KillsPerUpgrade[WeaponLevel - 1])
        {
            _killsAtLevel = 0;
            SetWeaponLevel(WeaponLevel + 1);
        }
    }

    /// <summary>
    /// Taking a hit costs one weapon level and the progress towards the next one.
    /// </summary>
    private void DowngradeWeapon()
    {
        _killsAtLevel = 0;
        if (WeaponLevel > 1)
        {
            SetWeaponLevel(WeaponLevel - 1);
        }
    }

    public override void CalculateHit(int amount)
    {
        //Remove heart from UI
        UserInterface.DecreasePlayerHealth();
        base.CalculateHit(amount);
        CameraShake.Shake(0.25f, 0.25f);
        Sfx.Play("player_hit");
        DowngradeWeapon();
        ScreenFx.Flash(new Color(1f, 0.1f, 0.1f), 0.4f);
        if (HitPoints == 1)
        {
            ScreenFx.SetPressure(true); // last life: pulse the screen edges red
        }

        if (HitPoints <= 0)
        {
            Explosion.Spawn(transform.position, new Color(0.4f, 0.7f, 1f), 2f);
            CameraShake.Shake(0.6f, 0.5f);
            Sfx.Play("explosion_big");
            GameMngr.EndGame(); 
        }
    }

}
