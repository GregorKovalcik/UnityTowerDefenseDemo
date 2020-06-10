using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

/// <summary>
/// Controls related to processing of the actual game (game logic, player state).
/// 
/// Game logic: game starts or pauses, enemies are moving, towers are built, enemies are killed, game ends.
/// Player state: money, progression, player death...
/// 
/// For level related stuff see LevelController: level settings for enemy waves, map (including tower spots), ...
/// 
/// Rendering layers:
/// 0: Background
/// 1: Background decoration
/// 
/// 2: Floor particles (blood trails, craters)
/// 3: Floor objects (coins)
/// 
/// 4: Ground objects (ground enemies)
/// 5: Projectiles
/// 6: Ground particles (explosions, smoke)
/// 
/// 7: Air objects (air enemies)
/// 8: Air particles (explosions, smoke, smoke trails)
/// 
/// </summary>
[RequireComponent(typeof(LevelController))]
public class GameController : MonoBehaviour
{
    // GameObject names
    public const string GAMECONTROLLER_STRING = "GameController";
    public const string TOWERS_STRING = "Towers";
    public const string ENEMIES_STRING = "Enemies";
    public const string PROJECTILES_STRING = "Projectiles";
    public const string COINS_STRING = "Coins";
    public const string PARTICLES_STRING = "Particles";

    // entity containers
    private GameObject _enemiesContainer;
    private GameObject _towersContainer;
    private GameObject _projectilesContainer;
    private GameObject _coinsContainer;
    private GameObject _particlesContainer;

    [Header("Game logic")]
    public float SimulationSpeed = 1;
    public TowerController PlacingTower = null;
    private bool _isPaused = false;
    private bool _isGameOver = false;
    private float _gameStartTime;
    private float GameDuration => Time.time - _gameStartTime;
    
    [Header("Player progression")]
    public int Score;
    public int Money;
    
    [Header("Level")]
    public LevelController Level;
    private LevelSettings Settings => Level.Settings;

    [Header("Tower prototypes")]
    public TowerController GunTowerPrefab;
    public TowerController RocketTowerPrefab;

    [Header("Enemy prototypes")]
    public EnemyController WalkingEnemyPrefab;
    public EnemyController DrivingEnemyPrefab;
    public EnemyController FlyingEnemyPrefab;

    [Header("Enemy spawn settings")]
    public bool SpawnWalkingEnemies = true;
    public bool SpawnDrivingEnemies = true;
    public bool SpawnFlyingEnemies = true;
    private readonly List<Coroutine> _spawningCoroutines = new List<Coroutine>();
    
    [Header("Events")]
    public UnityEvent GameOverEvent;
    public UnityEvent TowerPlacedEvent;

    public static GameObject FindOrCreateGameObject(string name)
    {
        GameObject gameObj = GameObject.Find(name);
        if (gameObj == null)
        {
            gameObj = new GameObject(name);
        }
        return gameObj;
    }

    public void Start()
    {
        Level = GetComponent<LevelController>();

        _enemiesContainer = FindOrCreateGameObject(ENEMIES_STRING);
        _towersContainer = FindOrCreateGameObject(TOWERS_STRING);
        _projectilesContainer = FindOrCreateGameObject(PROJECTILES_STRING);
        _coinsContainer = FindOrCreateGameObject(COINS_STRING);
        _particlesContainer = FindOrCreateGameObject(PARTICLES_STRING);
    }


    public void Update()
    {
        // pause and game speed for debugging
        Time.timeScale = _isPaused ? 0 : SimulationSpeed;

        ListenForPlacingTower();
    }

    public void PlayLoadedLevel()
    {
        if (Level.IsLevelLoaded)
        {
            RestartGame();
        }
    }

    public void PlayLevel(int iLevel)
    {
        Level.LoadLevel(iLevel);
        RestartGame();
    }

    public void RestartGame()
    {
        // assume level is already loaded
        ClearGame();

        // load game level settings
        // money
        Money = Settings.StartingMoney;

        // towers
        GunTowerPrefab.Price = Settings.TowerPriceGun;
        RocketTowerPrefab.Price = Settings.TowerPriceRocket;

        // enemies
        WalkingEnemyPrefab.Reward = Settings.EnemyRewardWalking;
        DrivingEnemyPrefab.Reward = Settings.EnemyRewardDriving;
        FlyingEnemyPrefab.Reward = Settings.EnemyRewardFlying;

        // coins
        WalkingEnemyPrefab.CoinPrefab.Value = Settings.CoinsWalking;
        DrivingEnemyPrefab.CoinPrefab.Value = Settings.CoinsDriving;
        FlyingEnemyPrefab.CoinPrefab.Value = Settings.CoinsFlying;

        WalkingEnemyPrefab.CoinDropProbability = Settings.CoinChanceWalking;
        DrivingEnemyPrefab.CoinDropProbability = Settings.CoinChanceDriving;
        FlyingEnemyPrefab.CoinDropProbability = Settings.CoinChanceFlying;

        // spawning
        WalkingEnemyPrefab.SpawnTime = Settings.SpawnTimeWalking;
        DrivingEnemyPrefab.SpawnTime = Settings.SpawnTimeDriving;
        FlyingEnemyPrefab.SpawnTime = Settings.SpawnTimeFlying;

        // set new spawning coroutines
        foreach (Coroutine coroutine in _spawningCoroutines)
        {
            StopCoroutine(coroutine);
        }
        _spawningCoroutines.Clear();

        // start new spawning coroutines
        if (SpawnWalkingEnemies && Settings.SpawnStartWalking >= 0)
        {
            _spawningCoroutines.Add(StartCoroutine(SpawnEnemyRepeatedly(WalkingEnemyPrefab, Settings.SpawnStartWalking)));
        }
        if (SpawnDrivingEnemies && Settings.SpawnStartDriving >= 0)
        {
            _spawningCoroutines.Add(StartCoroutine(SpawnEnemyRepeatedly(DrivingEnemyPrefab, Settings.SpawnStartDriving)));
        }
        if (SpawnFlyingEnemies && Settings.SpawnStartFlying >= 0)
        {
            _spawningCoroutines.Add(StartCoroutine(SpawnEnemyRepeatedly(FlyingEnemyPrefab, Settings.SpawnStartFlying)));
        }

        // start the game
        _isPaused = false;
        _gameStartTime = Time.time;
    }

    public void ClearGame()
    {
        // reset control variables
        Score = 0;
        Money = 0;
        _isPaused = true;
        _isGameOver = false;

        // delete all entities
        DestroyChildren(_enemiesContainer);
        DestroyChildren(_towersContainer);
        DestroyChildren(_projectilesContainer);
        DestroyChildren(_coinsContainer);
        DestroyChildren(_particlesContainer);
    }


    public void TogglePause()
    {
        if (_isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        _isPaused = true;
        MuteAudio(true);
    }
    public void Unpause()
    {
        _isPaused = false;
        MuteAudio(false);
    }


    public void SetPlacingTower(TowerController tower)
    {
        PlacingTower = tower;
    }

    public void ClearPlacingTower()
    {
        PlacingTower = null;
    }

    
    public void AddMoney(int amount)
    {
        Money += amount;
        Score += amount;
    }

    public void GameOver()
    {
        _isPaused = true;
        _isGameOver = true;
        MuteAudio(true);
        GameOverEvent?.Invoke();
    }


    private void ListenForPlacingTower()
    {
        if (!_isPaused 
            && IsInputDown()
            && PlacingTower != null 
            && PlacingTower.Price <= Money)
        {
            Vector3Int gridPosition = GetGridPosition(GetWorldPositionFromInput());

            if (IsPositionInsideMap(gridPosition) && Level.IsLandTileAtPosition(gridPosition) && !IsTowerAtPosition(gridPosition))
            {
                PlaceTower(PlacingTower, gridPosition);
                Money -= PlacingTower.Price;
                ClearPlacingTower();
                TowerPlacedEvent?.Invoke();
            }
        }
    }

    public static bool IsInputDown()
    {
        return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }
    
    public static bool IsInputActive()
    {
        return Input.GetMouseButton(0) || Input.touchCount > 0;
    }


    public static Vector3 GetWorldPositionFromInput()
    {
        return GetWorldPositionFromScreen(GetScreenPositionFromInput());
    }

    private static Vector3 GetScreenPositionFromInput()
    {
        if (Input.GetMouseButton(0))
        {
            return Input.mousePosition;
        }
        else if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        else 
        {
            return Vector3.zero;
        }
    }

    private static Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;
        return worldPosition;
    }

    private static Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), 0);
    }

    private bool IsPositionInsideMap(Vector3Int gridPositionInt)
    {
        return gridPositionInt.x >= Level.BottomLeftMapPosition.x
            && gridPositionInt.y >= Level.BottomLeftMapPosition.y
            && gridPositionInt.x < Level.BottomLeftMapPosition.x + LevelController.MAP_WIDTH
            && gridPositionInt.y < Level.BottomLeftMapPosition.y + LevelController.MAP_HEIGHT;
    }

    private bool IsTowerAtPosition(Vector3 gridPosition)
    {
        return _towersContainer.transform.GetComponentsInChildren<TowerController>()
            .Where(tower => tower.transform.position == gridPosition)
            .Any();
    }

    private void PlaceTower(TowerController towerPrefab, Vector3Int gridPosition)
    {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.left);
        Instantiate(towerPrefab, gridPosition, rotation, _towersContainer.transform);
    }


    private IEnumerator SpawnEnemyRepeatedly(EnemyController enemyPrefab, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        while (!_isGameOver)
        {
            SpawnEnemy(enemyPrefab);
            //yield return new WaitForSeconds(enemyPrefab.SpawnTime);

            // adjust spawn rate by current game difficulty
            //enemyPrefab.SpawnTime = enemyPrefab.BaseSpawnTime / (1 + _gameDuration * (_settings.SpawnRateEvolutionPerMinute - 1) / 60);
            float spawnNextIn = enemyPrefab.SpawnTime / (1 + (GameDuration - delay) * (Settings.SpawnRateEvolutionPerMinute - 1) / 60);
            yield return new WaitForSeconds(spawnNextIn);
        }
    }

    private void SpawnEnemy(EnemyController enemyPrefab)
    {
        float randomSpawnX = Random.Range(Level.SpawnPoint.x - Level.SpawnWidth / 2, Level.SpawnPoint.x + Level.SpawnWidth / 2);
        float randomSpawnY = Random.Range(Level.SpawnPoint.y - Level.SpawnHeight / 2, Level.SpawnPoint.y + Level.SpawnHeight / 2);
        Vector3 spawnPoint = new Vector3(randomSpawnX, randomSpawnY, 0);
        EnemyController enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity, _enemiesContainer.transform);
        
        // adjust moving speed by current game difficulty
        enemy.MovingSpeed = enemy.BaseMovingSpeed * ( 1 + GameDuration * (Settings.MovementSpeedEvolutionPerMinute - 1) / 60);
    }


    private void MuteAudio(bool doMute = true)
    {
        MuteAudioInChildren<EnemyController>(_enemiesContainer, doMute);
        MuteAudioInChildren<TowerController>(_towersContainer, doMute);
        MuteAudioInChildren<ProjectileController>(_projectilesContainer, doMute);
        MuteAudioInChildren<Component>(_particlesContainer, doMute);
    }

    private void MuteAudioInChildren<T>(GameObject parent, bool doMute) where T : Component
    {
        foreach (T child in parent.GetComponentsInChildren<T>())
        {
            AudioSource audio = child.GetComponent<AudioSource>();
            if (audio != null)
            {
                if (doMute)
                {
                    audio.Pause();
                }
                else
                {
                    audio.Play();
                }
            }
        }
    }

    private void DestroyChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
