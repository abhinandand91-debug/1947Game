// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-06

using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{

    [Header("General Properties")]
    public int HitPoints = 3;
    public float FireDelay;
    [Range(3, 15)]
    public float Speed;


    [Header("Laser Properties")]
    public GameObject LaserPrefab;
    public float LaserDistanceForward;
    public float LaserDistanceRight;
    public Vector3 LaserRotation;

    [Header("Other")]
    public GameManager GameMngr;
    protected bool CanShoot = true;
    protected EnvrionmentManager EnvironmentMngr;


    // Use this for initialization
    void Start()
    {
        EnvironmentMngr = GameObject.FindObjectOfType<EnvrionmentManager>();
    }

    /// <summary>
    /// Creates an instance of the laser game object and disables firing based on the FireDelay
    /// </summary>
    protected void ShootLaser()
    {
        FireLaser(0f, 0f);

        StartCooldown();
    }

    /// <summary>
    /// Creates one laser. Use several calls with different drift or offset values for spread and multi shots.
    /// </summary>
    /// <param name="drift">Sideways speed of the laser</param>
    /// <param name="offsetX">Sideways offset from the gun position</param>
    protected void FireLaser(float drift, float offsetX)
    {
        Vector3 position = transform.position + Vector3.forward * LaserDistanceForward + Vector3.right * (LaserDistanceRight + offsetX);
        GameObject laser = (GameObject)GameObject.Instantiate(LaserPrefab, position, Quaternion.Euler(LaserRotation));

        Laser component = laser.GetComponent<Laser>();
        if (component != null)
        {
            component.Drift = drift;
        }
    }

    /// <summary>
    /// Disables firing until FireDelay has passed.
    /// </summary>
    protected void StartCooldown()
    {
        Invoke("EnableShooting", FireDelay);
    }


    /// <summary>
    /// Enables the ability to shoot
    /// </summary>
    protected void EnableShooting()
    {
        CanShoot = true;
    }

    /// <summary>
    /// Applies the damage of the bullet/laser to the health and applies death
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    public virtual void CalculateHit(int amount)
    {
        HitPoints -= amount;
        HitFlash.Play(gameObject);
    }
}
