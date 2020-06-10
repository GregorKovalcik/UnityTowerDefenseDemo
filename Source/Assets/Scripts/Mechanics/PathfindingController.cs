using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

/// <summary>
/// Allows pathfinding of an enemy across path tiles towards the closest base tile.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class PathfindingController : MonoBehaviour
{
    private EnemyController _enemy;
    private LevelController _level;

    [Header("Waypoints")]
    public float WaypointWobble = 0;
    public Vector3 NextWaypoint;
    public Stack<Vector3> Waypoints;
    
    [Header("Debug visualization")]
    public float DebugSphereRadius = 0.1f;


    void Start()
    {
        _enemy = GetComponent<EnemyController>();
        _level = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<LevelController>();
        
        Waypoints = FindPath();
        NextWaypoint = Waypoints.Pop();
    }

    void Update()
    {
        Vector3 waypointDirection = NextWaypoint - transform.position;
        
        // turn towards the direction of movement
        float angle = Vector3.SignedAngle(transform.right, waypointDirection, Vector3.forward);
        transform.Rotate(Vector3.forward, angle * _enemy.TurnSpeed * _enemy.MovingSpeed * Time.deltaTime);

        // move towards waypoint
        float damageSpeedReduction = (_enemy.MovingSpeed - _enemy.DamagedMovingSpeed) * _enemy.DamagePercentage;
        float movingSpeed = (_enemy.MovingSpeed - damageSpeedReduction) * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, NextWaypoint, movingSpeed);

        // move to the next waypoint if needed
        if (transform.position == NextWaypoint && Waypoints.Count > 0)
        {
            NextWaypoint = Waypoints.Pop() + new Vector3(
                Random.Range(-0.5f, 0.5f) * WaypointWobble, 
                Random.Range(-0.5f, 0.5f) * WaypointWobble, 
                0);
        }
    }


    public void OnDrawGizmos()
    {
        // next waypoint
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(NextWaypoint, DebugSphereRadius);

        // path waypoints
        Gizmos.color = Color.green;
        foreach (Vector3 waypoint in Waypoints)
        {
            Gizmos.DrawSphere(waypoint, DebugSphereRadius);
        }
    }


    private Stack<Vector3> FindPath()
    {
        // prepare helper structures
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        // load starting position
        Vector2Int startingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        queue.Enqueue(startingPosition);
        distances[startingPosition] = 0;

        // locate the closest base and produce waypoints to it
        Vector2Int closestBase = ForwardSearchClosestBase(startingPosition, queue, distances);
        Stack<Vector3> waypoints = BacktrackShortestPath(distances, closestBase);
        return waypoints;
    }

    private Vector2Int ForwardSearchClosestBase(Vector2Int startingPosition, Queue<Vector2Int> queue, Dictionary<Vector2Int, int> distances)
    {
        while (queue.Count > 0)
        {
            Vector2Int currentPosition = queue.Dequeue();
            int currentDistance = distances[currentPosition];
            Tile currentTile = _level.Map.GetTile<Tile>(new Vector3Int(currentPosition.x, currentPosition.y, 0));

            // found target
            if (currentTile == _level.BaseTile)
            {
                return currentPosition;
            }

            // spread to nearby paths using 4-connectivity
            foreach (Vector2Int offset in new Vector2Int[]
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0)
                })
            {
                Vector2Int nearbyPosition = currentPosition + offset;

                // skip already traversed paths
                if (distances.ContainsKey(nearbyPosition))
                {
                    continue;
                }

                // add only path and base tiles
                Tile nearbyTile = _level.Map.GetTile<Tile>(new Vector3Int(nearbyPosition.x, nearbyPosition.y, 0));
                if (nearbyTile == _level.PathTile || nearbyTile == _level.BaseTile)
                {
                    queue.Enqueue(nearbyPosition);
                    distances[nearbyPosition] = currentDistance + 1;
                }
            }
        }

        // fallback when no base was found
        return startingPosition;
    }

    private Stack<Vector3> BacktrackShortestPath(Dictionary<Vector2Int, int> distances, Vector2Int closestBase)
    {
        Stack<Vector3> waypoints = new Stack<Vector3>();
        waypoints.Push(new Vector3(closestBase.x, closestBase.y, 0));

        Vector2Int currentPosition = closestBase;
        int currentDistance = distances[closestBase];
        while (currentDistance > 0)
        {
            // find a neighbor with decremented distance
            foreach (Vector2Int offset in new Vector2Int[]
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0)
                })
            {
                Vector2Int nearbyPosition = currentPosition + offset;

                if (distances.TryGetValue(nearbyPosition, out int nearbyDistance)
                    && nearbyDistance == currentDistance - 1)
                {
                    // add waypoint
                    Vector3 waypoint = new Vector3(nearbyPosition.x, nearbyPosition.y, 0);
                    waypoints.Push(waypoint);
                    // backtrack
                    currentPosition = nearbyPosition;
                    currentDistance--;
                }
            }
        }

        return waypoints;
    }

}
