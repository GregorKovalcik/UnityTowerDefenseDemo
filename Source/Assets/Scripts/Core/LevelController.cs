using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

/// <summary>
/// Controls related to game level processing.
/// (loading maps, map building, map interaction, level difficulty settings of enemy waves, starting money, etc.)
/// </summary>
public class LevelController : MonoBehaviour
{
    public const int MAP_WIDTH = 12;
    public const int MAP_HEIGHT = 6;

    private const char PATH_CHAR = ' ';
    private const char LAND_CHAR = 'O';
    private const char OBSTACLE_CHAR = '#';
    //private const char BASE_CHAR = '@';

    private static readonly char[][] _emptyMapDefinition = Enumerable.Repeat(Enumerable.Repeat(PATH_CHAR, MAP_WIDTH).ToArray(), MAP_HEIGHT).ToArray();


    [Header("Map boundaries definition")]
    public Vector2Int BottomLeftMapPosition = new Vector2Int(0, 0);
    public Vector2 SpawnPoint = new Vector2(-4, 3);
    public float SpawnWidth = 4f;
    public float SpawnHeight = 6f;

    [Header("Tilemaps for map building")]
    public Tilemap Map;
    
    [Header("Tile prototypes for map building")]
    public Tile PathTile;
    public Tile LandTile;
    public Tile ObstacleTile;
    public Tile BaseTile;

    [Header("Level loading")]
    public string LevelFolder = "Levels";
    public string[] LevelFiles = new string[] { "Level1", "Level2", "Level3" };
    public string DevelopmentLevelName = "Level000";
    public string LoadedLevel = "Level";
    public bool IsLevelLoaded = false;
    public UnityEvent OnLevelLoaded;
    public UnityEvent OnLevelCleared;

    [Header("Level settings")]
    public List<Vector2Int> BaseTiles = new List<Vector2Int>();
    public LevelSettings Settings = new LevelSettings();


    void Start()
    {
        // store currently loaded map - used for easy creation of new maps
        //StoreLevel(Path.Combine(LevelFolder, DevelopmentLevelName));
        ClearMap();
    }


    public void ClearMap()
    {
        BuildMap(_emptyMapDefinition);
        UpdateBaseTiles();
        IsLevelLoaded = false;
        OnLevelCleared?.Invoke();
    }

    public void LoadLevel(int iLevel)
    {
        LoadLevel(Path.Combine(LevelFolder, LevelFiles[iLevel]));
        LoadedLevel = $"Level {iLevel + 1}";
    }

    public bool IsLandTileAtPosition(Vector3Int gridPosition)
    {
        return Map.GetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0)) == LandTile;
    }


    private void LoadLevel(string levelPath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(levelPath);
        using (StringReader reader = new StringReader(textAsset.text))
        {
            // 12x6 map top-down in row-first order (Path, Land, Obstacle)
            char[][] mapDefinition = new char[MAP_HEIGHT][];
            for (int iRow = 0; iRow < MAP_HEIGHT; iRow++)
            {
                mapDefinition[iRow] = reader.ReadLine().ToCharArray();
            }
            BuildMap(mapDefinition);

            Settings = JsonUtility.FromJson<LevelSettings>(reader.ReadToEnd());
        }

        UpdateBaseTiles();
        IsLevelLoaded = true;
        OnLevelLoaded?.Invoke();
        // TODO: check map for consistency
    }

    private void StoreLevel(string levelPath)
    {
        levelPath = Path.Combine(Path.Combine("Assets", "Resources"), levelPath);
        Directory.CreateDirectory(Path.GetDirectoryName(levelPath));
        using (StreamWriter writer = new StreamWriter(levelPath))
        {
            // 12x6 map top-down in row-first order (Path, Land, Obstacle)
            for (int iRow = 0; iRow < MAP_HEIGHT; iRow++)
            {
                for (int iCol = 0; iCol < MAP_WIDTH; iCol++)
                {
                    int mapX = BottomLeftMapPosition.x + iCol;
                    int mapY = BottomLeftMapPosition.y + MAP_HEIGHT - 1 - iRow;
                    try
                    {
                        Tile tile = Map.GetTile<Tile>(new Vector3Int(mapX, mapY, 0));
                        char tileChar = EncodeTile(tile, mapX, mapY);
                        writer.Write(tileChar);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                writer.WriteLine();
            }

            writer.WriteLine(JsonUtility.ToJson(Settings, true));
        }
    }



    private void BuildMap(char[][] mapDefinition)
    {
        // 12x6 map top-down in row-first order (Path, Land, Obstacle)
        for (int iRow = 0; iRow < MAP_HEIGHT; iRow++)
        {
            for (int iCol = 0; iCol < MAP_WIDTH; iCol++)
            {
                int mapX = BottomLeftMapPosition.x + iCol;
                int mapY = BottomLeftMapPosition.y + MAP_HEIGHT - 1 - iRow;
                try
                {
                    char tileChar = mapDefinition[iRow][iCol];
                    Tile tile = DecodeTile(tileChar);
                    Map.SetTile(new Vector3Int(mapX, mapY, 0), tile);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }


    /// <summary>
    /// Updates list of base tiles. Used because map definition could contain base tiles.
    /// TODO: currently unused
    /// </summary>
    private void UpdateBaseTiles()
    {
        BaseTiles.Clear();
        for (int iRow = BottomLeftMapPosition.y; iRow < MAP_HEIGHT; iRow++)
        {
            // include right-most part behind map border (default base location)
            for (int iCol = BottomLeftMapPosition.x; iCol <= MAP_WIDTH; iCol++)
            {
                if (Map.GetTile(new Vector3Int(iCol, iRow, 0)) == BaseTile)
                {
                    BaseTiles.Add(new Vector2Int(iCol, iRow));
                }
            }
        }
    }

    private Tile DecodeTile(char tileChar)
    {
        switch (tileChar)
        {
            case PATH_CHAR:
                return PathTile;
            case LAND_CHAR:
                return LandTile;
            case OBSTACLE_CHAR:
                return ObstacleTile;
            //case BASE_CHAR:
            //    return BaseTile;
            default:
                Debug.LogError($"Unknown tile definition: '{tileChar}'");
                return null;
        }
    }
    
    private char EncodeTile(Tile tile, int mapX, int mapY)
    {
        if (tile == PathTile)
        {
            return PATH_CHAR;
        }
        else if (tile == LandTile)
        {
            return LAND_CHAR;
        }
        else if (tile == ObstacleTile)
        {
            return OBSTACLE_CHAR;
        }
        //else if (tile == BaseTile)
        //{
        //    return BASE_CHAR;
        //}
        else if (tile == null)
        {
            Debug.LogError($"Empty tile at [{mapX}, {mapY}]");
            return PATH_CHAR;
        }
        else
        {
            Debug.LogError($"Unsupported tile {tile.name} at [{mapX}, {mapY}]");
            return PATH_CHAR;
        }
    }

    
}


public class LevelSettings
{
    // money
    public int StartingMoney = 500;

    // towers
    public int TowerPriceGun = 200;
    public int TowerPriceRocket = 1000;

    // enemies
    public int EnemyRewardWalking = 5;
    public int EnemyRewardDriving = 10;
    public int EnemyRewardFlying = 20;

    // coins
    public int CoinsWalking = 5;
    public int CoinsDriving = 10;
    public int CoinsFlying = 20;

    public float CoinChanceWalking = 0.1f;
    public float CoinChanceDriving = 0.2f;
    public float CoinChanceFlying = 0.2f;

    public int CoinDecayTime = 10;

    // spawning
    public int SpawnStartWalking = 0;
    public int SpawnStartDriving = 15;
    public int SpawnStartFlying = 60;

    public int SpawnTimeWalking = 3;
    public int SpawnTimeDriving = 5;
    public int SpawnTimeFlying = 15;

    public float SpawnRateEvolutionPerMinute = 10f;

    // movement
    public float MovementSpeedEvolutionPerMinute = 4f;
}
