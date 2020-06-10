using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

[RequireComponent(typeof(Tilemap))]
public class DecorationController : MonoBehaviour
{
    private Tilemap _map;
    private Random _random;
    
    [Header("Base level")]
    public LevelController Level;

    [Header("Decoration density")]
    public float PathDecorationDensity = 0.1f;
    public float LandDecorationDensity = 0.3f;
    public float ObstacleDecorationDensity = 0.5f;


    [Header("Tile types")]
    public Tile PathTile;
    public Tile LandTile;
    public Tile ObstacleTile;
    
    [Header("Decorations")]
    public Tile[] PathDecorations;
    public Tile[] LandDecorations;
    public Tile[] ObstacleDecorations;
    
    // Start is called before the first frame update
    void Start()
    {
        _map = GetComponent<Tilemap>();
    }


    public void Decorate()
    {
        _map.ClearAllTiles();
        _random = new Random(42);

        for (int iRow = 0; iRow < LevelController.MAP_HEIGHT; iRow++)
        {
            for (int iCol = 0; iCol < LevelController.MAP_WIDTH; iCol++)
            {
                int mapX = Level.BottomLeftMapPosition.x + iCol;
                int mapY = Level.BottomLeftMapPosition.y + LevelController.MAP_HEIGHT - 1 - iRow;
                Vector3Int position = new Vector3Int(mapX, mapY, 0);

                HandleDecoration(position, PathTile, PathDecorations, PathDecorationDensity);
                HandleDecoration(position, LandTile, LandDecorations, LandDecorationDensity);
                HandleDecoration(position, ObstacleTile, ObstacleDecorations, ObstacleDecorationDensity);
            }
        }
    }

    private void HandleDecoration(Vector3Int position, Tile decorationType, Tile[] decorations, float decorationDensity)
    {
        TileBase mapTile = Level.Map.GetTile(position);
        
        if (mapTile == decorationType && decorations != null && decorations.Length > 0)
        {
            if (_random.NextDouble() < decorationDensity)
            {
                Tile decorationTile = GetRandomDecoration(decorations);
                _map.SetTile(position, decorationTile);
            }
        }
    }

    private Tile GetRandomDecoration(Tile[] decorations)
    {
        Tile tile = decorations[_random.Next(decorations.Length)];
        // TODO: consider adding rotation
        return tile;
    }

    public void ClearDecorations()
    {
        if (_map != null)
        {
            _map.ClearAllTiles();
        }
    }
}
