using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/// <summary>
/// Detaches all child particles OnDisable() to allow them to finish their animations.
/// </summary>
public class ParticleDetachController : MonoBehaviour
{
    private GameObject _particleContainer;
    private ParticleSystem[] _particleSystems;

    
    void Start()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _particleContainer = GameController.FindOrCreateGameObject(GameController.PARTICLES_STRING);
    }

    private void OnDisable()
    {
        if (_particleSystems != null)
        {
            foreach (ParticleSystem particles in _particleSystems)
            {
                if (particles.gameObject.activeInHierarchy)
                {
                    particles.transform.parent = _particleContainer.transform;
                    EmissionModule emission = particles.emission;
                    emission.enabled = false;
                    Destroy(particles.gameObject, particles.main.startLifetime.constantMax);
                }
            }
        }
    }
}
