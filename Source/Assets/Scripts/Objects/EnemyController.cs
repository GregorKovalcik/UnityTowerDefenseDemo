using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

/// <summary>
/// Enemy settings and controls.
/// (health controller, rewards)
/// </summary>
public class EnemyController : MonoBehaviour
{
    private GameController _game;
    private GameObject _particleContainer;
    private GameObject _coinContainer;

    [Header("Movement")]
    public float BaseMovingSpeed;
    public float MovingSpeed;
    public float DamagedMovingSpeed;
    public float TurnSpeed;

    [Header("Health")]
    public int MaxHealth;
    public int CurrentHealth;
    
    [Header("Damage")]
    public GameObject HitVisualization;
    public ParticleSystem DamageParticles;
    public int MaxDamageParticleCount = 10;

    [Header("Spawning")]
    public float SpawnTime = 3;


    public float DamagePercentage => (float)(MaxHealth - CurrentHealth) / MaxHealth;
    
    [Header("Rewards")]
    public int Reward;
    public float CoinDropProbability = 0.1f;
    public CoinController CoinPrefab;
    

    void Start()
    {
        _game = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<GameController>();
        DamageParticles = GetComponentInChildren<ParticleSystem>();

        _particleContainer = GameController.FindOrCreateGameObject(GameController.PARTICLES_STRING);
        _coinContainer = GameController.FindOrCreateGameObject(GameController.COINS_STRING);
    }

    void Update()
    {
        EmissionModule emissionModule = DamageParticles.emission;
        emissionModule.rateOverTime = new MinMaxCurve(DamagePercentage * MaxDamageParticleCount);
    }

    public void HitContact(ContactPoint2D contact, int damage)
    {
        // visualize
        if (HitVisualization != null)
        {
            Vector3 sourceDirection = (contact.otherCollider.transform.position - (Vector3)contact.point).normalized;
            Vector3 hitPoint = (Vector3)contact.point + sourceDirection * 0.01f;
            Quaternion hitRotation = Quaternion.LookRotation(HitVisualization.transform.forward, contact.normal);
            GameObject hitParticles = Instantiate(HitVisualization, hitPoint, hitRotation, _particleContainer.transform);
            Destroy(hitParticles, 1);
        }

        Hit(damage);
    }

    public void Hit(int damage)
    {
        // hurt
        CurrentHealth -= damage;

        // die
        if (CurrentHealth <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        // reward
        _game.AddMoney(Reward);

        // coin drop
        if (Random.value < CoinDropProbability)
        {
            Instantiate(CoinPrefab, this.transform.position, Quaternion.identity, _coinContainer.transform);
        }

        // death
        Destroy(this.gameObject);
    }

}
