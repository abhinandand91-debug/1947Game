// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-06

using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{

    public int Damage = 10;
    public bool EnemyWeapon;
    public float Drift; // sideways speed, used for spread shots

    void Start()
    {
        // Safety net so lasers that drift off the sides never pile up
        Destroy(this.gameObject, 6f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * Drift * Time.deltaTime;
        if (EnemyWeapon)
        {
            transform.position = Vector3.Lerp(transform.position, transform.position -= Vector3.forward, 5f * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, transform.position += Vector3.forward, 8f * Time.deltaTime);
        }
    }



    public void OnCollisionEnter(Collision collision)
    {
        if (!EnemyWeapon && collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<Enemy>().CalculateHit(Damage);
        }
        else if (EnemyWeapon && collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Player>().CalculateHit(Damage);
        }

        Explosion.Spawn(transform.position, EnemyWeapon ? new Color(1f, 0.3f, 0.3f) : new Color(0.4f, 0.8f, 1f), 0.3f);
        Sfx.Play("laser_hit", 0.4f);
        Destroy(this.gameObject);
    }
}
