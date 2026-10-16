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

            ShootLaser();
        }
    }


    public override void CalculateHit(int amount)
    {
        //Remove heart from UI
        UserInterface.DecreasePlayerHealth();
        base.CalculateHit(amount);
        CameraShake.Shake(0.25f, 0.25f);

        if (HitPoints <= 0)
        {
            Explosion.Spawn(transform.position, new Color(0.4f, 0.7f, 1f), 2f);
            CameraShake.Shake(0.6f, 0.5f);
            GameMngr.EndGame(); 
        }
    }

}
