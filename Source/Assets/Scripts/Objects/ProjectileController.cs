using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile settings and controls.
/// (hitting enemies, spawning explosions)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : MonoBehaviour
{
    private GameObject _particleContainer;
    
    public int Damage = 1;
    public float Speed = 1;
    public float SelfDestructTime = -1;
    public ParticleSystem ImpactParticles;
    public GameObject SecondaryProjectile;
    

    public void Start()
    {
        _particleContainer = GameController.FindOrCreateGameObject(GameController.PARTICLES_STRING);

        if (SelfDestructTime > 0)
        {
            StartCoroutine(SelfDestructDelayed(SelfDestructTime));
        }
    }

    private IEnumerator SelfDestructDelayed(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        SelfDestruct(transform.position);
    }

    private void SelfDestruct(Vector3 impactPosition)
    {
        // visualize impact
        if (ImpactParticles != null)
        {
            ParticleSystem impactParticles = Instantiate(ImpactParticles, impactPosition, Quaternion.identity, _particleContainer.transform);
            Destroy(impactParticles.gameObject, impactParticles.main.duration);
        }

        // spawn secondary projectile
        if (SecondaryProjectile != null)
        {
            Instantiate(SecondaryProjectile, impactPosition, Quaternion.identity, _particleContainer.transform);
        }

        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ignore other projectiles
        //if (collision.gameObject.GetComponent<ProjectileController>() != null)
        //{
        //    return;
        //}

        ContactPoint2D contact = collision.GetContact(0);
        Vector3 impactPosition = contact.point;
        
        // hit the target
        EnemyController enemy;
        if ((enemy = collision.gameObject.GetComponent<EnemyController>()) != null)
        {
            enemy.HitContact(contact, Damage);
        }

        SelfDestruct(impactPosition);
    }
}
