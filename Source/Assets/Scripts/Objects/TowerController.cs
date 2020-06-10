using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Tower settings and controls.
/// (tracking, shooting)
/// </summary>
public class TowerController : MonoBehaviour
{
    private const string TURRET_STRING = "Turret";

    private Random _random;
    private AudioSource _audioSource;
    private GameObject _turret;
    private GameObject _enemiesContainer;
    private GameObject _projectileContainer;
    private float _timeSinceFiring = 0;

    [Header("Building")]
    public int Price = 1;

    [Header("Tracking")]
    public float EnemyDetectionRadius = 5;
    public float EnemyDetectionXCoordinateLimit = -0.5f;
    public float TurretTurningSpeed = 1;
    public float AimToleranceAngle = 5;
    public EnemyController Target;

    [Header("Reload")]
    public bool IsReloaded = true;
    public float ReloadTime = 1;
    public float ReloadTimeSpread = 0.1f;
    
    [Header("Projectile")]
    public ProjectileController ProjectilePrefab;
    
    [Header("Audio")]
    public AudioClip[] FiringAudioClips;


    void Start()
    {
        _random = new Random(gameObject.GetInstanceID());
        _turret = transform.Find(TURRET_STRING).gameObject;
        _audioSource = GetComponent<AudioSource>();

        _enemiesContainer = GameController.FindOrCreateGameObject(GameController.ENEMIES_STRING);
        _projectileContainer = GameController.FindOrCreateGameObject(GameController.PROJECTILES_STRING);
    }

    void Update()
    {
        // look for enemy to track
        if (Target == null)
        {
            Target = _enemiesContainer.GetComponentsInChildren<EnemyController>()
                .Where(enemy => enemy.transform.position.x > EnemyDetectionXCoordinateLimit)
                .OrderBy(enemy => Vector3.Distance(enemy.transform.position, this.transform.position))
                .FirstOrDefault();
        }

        // target aquired
        if (Target != null)
        {
            // attack if target is not too far
            if (Vector3.Distance(Target.transform.position, this.transform.position) < EnemyDetectionRadius)
            {
                // turn turret into target
                Vector3 enemyDirection = Target.transform.position - _turret.transform.position;
                Vector3 newLookDirection = Vector3.RotateTowards(_turret.transform.up, enemyDirection, TurretTurningSpeed * Time.deltaTime, 0);
                _turret.transform.up = newLookDirection;

                // fire if in aim tolerance angle
                float angle = Vector3.SignedAngle(_turret.transform.up, enemyDirection, Vector3.forward);
                if (Mathf.Abs(angle) < AimToleranceAngle && IsReloaded)
                {
                    Fire();
                    IsReloaded = false;
                }
            }
            else
            {
                // target is too far
                Target = null;
            }
        }

        // reload
        _timeSinceFiring += Time.deltaTime;
        if (_timeSinceFiring >= ReloadTime + (_random.NextDouble() - 0.5 * 2) * ReloadTimeSpread)
        {
            IsReloaded = true;
        }
    }

    private void Fire()
    {
        // launch a projectile
        ProjectileController projectile = Instantiate(ProjectilePrefab, _turret.transform.position, _turret.transform.rotation, _projectileContainer.transform);
        Vector2 force = new Vector2(_turret.transform.up.x, _turret.transform.up.y) * ProjectilePrefab.Speed;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        _timeSinceFiring = 0;
        Destroy(projectile.gameObject, 10);
        
        // play gunshot sound
        if (_audioSource != null 
            && FiringAudioClips != null 
            && FiringAudioClips.Length != 0)
        {
            _audioSource.clip = FiringAudioClips[_random.Next(0, FiringAudioClips.Length)];
            _audioSource.Play();
        }
    }
}
