// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-06

using UnityEngine;
using System.Collections;

/// <summary>
/// Handles enemy AI specific behaviors such as moving, shooting and taking damage
/// </summary>
public class Enemy : Ship
{

    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private GameObject _player;
    private float _distanceToPlayer = 4f;
    private int _points = 1;
    private int _maxHitPoints;
    private bool _enraged;
    private int _shotCount;

    public bool IsBoss { get; private set; }

    // Use this for initialization
    void Start()
    {

        //Get references to components
        _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _player = GameObject.FindObjectOfType<Player>().gameObject;
        GameMngr = GameObject.FindObjectOfType<GameManager>();

        // Set destination for navigation
        _navMeshAgent.SetDestination(_player.transform.position + Vector3.forward * _distanceToPlayer);
        _navMeshAgent.speed = Speed;

        //Start navigating
        _navMeshAgent.Resume();

        GetNextPosition();

    }

    // Update is called once per frame
    void Update()
    {

        //Continue to update destination based on player position
        transform.rotation = Quaternion.identity;

        if (CanShoot)
        {
            CanShoot = false;

            Fire();
        }
    }

    /// <summary>
    /// Gets the a random position within a set range of the player every half a second
    /// </summary>
    private void GetNextPosition()
    {
        Vector3 destination = _player.transform.position + Vector3.forward * _distanceToPlayer + Random.insideUnitSphere * 1;
        destination.y = transform.position.y;

        _navMeshAgent.SetDestination(destination);

        Invoke("GetNextPosition", .5f);
    }

    /// <summary>
    /// Applies the stats of the current wave. Call right after spawning, before the first frame.
    /// </summary>
    public void ApplyTier(WaveDefinition wave, bool boss)
    {
        HitPoints = wave.HitPoints;
        Speed = wave.Speed;
        FireDelay = wave.FireDelay;
        IsBoss = boss;
        _maxHitPoints = HitPoints;

        if (boss)
        {
            _points = 10;
            ScreenFx.SetBossBar(1f, true);
            _distanceToPlayer = 6f;
            transform.localScale *= 2.2f;
        }

        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.color = wave.Tint;
        }
    }

    /// <summary>
    /// Regular enemies fire one laser; the boss fires a five-way fan.
    /// </summary>
    private void Fire()
    {
        if (!IsBoss)
        {
            ShootLaser();
            return;
        }

        if (_enraged)
        {
            // Phase two: a fast stream of lasers that sweeps from side to side
            _shotCount++;
            FireLaser(Mathf.Sin(_shotCount * 0.5f) * 4f, 0f);
            StartCooldown();
            return;
        }

        float[] drifts = { -3f, -1.5f, 0f, 1.5f, 3f };
        foreach (float drift in drifts)
        {
            FireLaser(drift, drift * 0.1f);
        }
        StartCooldown();
    }

    /// <summary>
    /// The boss enters its second phase at half health: faster movement and a sweeping barrage.
    /// </summary>
    private void Enrage()
    {
        _enraged = true;
        FireDelay = 0.22f;
        Speed *= 1.3f;
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = Speed;
        }

        ScreenFx.Banner("ENRAGED!", new Color(1f, 0.3f, 0.3f));
        ScreenFx.Flash(new Color(1f, 0.1f, 0.1f), 0.4f);
        CameraShake.Shake(0.4f, 0.4f);
        Sfx.Play("boss_alarm");
    }

    private void OnDestroy()
    {
        // Covers every way a boss can disappear, including the run ending
        if (IsBoss)
        {
            ScreenFx.SetBossBar(0f, false);
        }
    }

    public override void CalculateHit(int amount)
    {
        base.CalculateHit(amount);

        if (IsBoss)
        {
            ScreenFx.SetBossBar((float)HitPoints / _maxHitPoints, HitPoints > 0);
            if (!_enraged && HitPoints > 0 && HitPoints * 2 <= _maxHitPoints)
            {
                Enrage();
            }
        }

        if (HitPoints <= 0)
        {
            Explosion.Spawn(transform.position, new Color(1f, 0.6f, 0.2f), 1f);
            CameraShake.Shake(0.12f, 0.15f);
            Sfx.Play("explosion_small");
            if (!IsBoss)
            {
                Pickup.TryDrop(transform.position);
            }
            if (IsBoss)
            {
                Explosion.Spawn(transform.position, new Color(0.85f, 0.55f, 1f), 3f);
                CameraShake.Shake(0.5f, 0.5f);
                Sfx.Play("explosion_big");
            }
            int awarded = GameMngr.IncrementScore(_points, IsBoss);
            ScreenFx.Popup(transform.position, "+" + awarded, new Color(1f, 0.85f, 0.3f), IsBoss ? 1.6f : 1f);
            Destroy(this.gameObject);
        }
    }
}
