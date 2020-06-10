using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows flying of an enemy in a straight line towards a random base tile.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class FlyingController : MonoBehaviour
{
    private EnemyController _enemy;
    private LevelController _level;

    [Header("Waypoints")]
    public Vector3 TargetWaypoint;

    [Header("Debug visualization")]
    public float DebugSphereRadius = 0.1f;

    
    void Start()
    {
        _enemy = GetComponent<EnemyController>();
        _level = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<LevelController>();

        // pick a random target base
        Vector2Int targetTile = _level.BaseTiles[Random.Range(0, _level.BaseTiles.Count)];
        TargetWaypoint = new Vector3(targetTile.x, targetTile.y, 0);
    }

    void Update()
    {
        Vector3 waypointDirection = TargetWaypoint - transform.position;

        // turn towards the direction of movement
        float angle = Vector3.SignedAngle(transform.right, waypointDirection, Vector3.forward);
        transform.Rotate(Vector3.forward, angle * _enemy.TurnSpeed * _enemy.MovingSpeed * Time.deltaTime);

        // move towards waypoint
        float damageSpeedReduction = (_enemy.MovingSpeed - _enemy.DamagedMovingSpeed) * _enemy.DamagePercentage;
        float movingSpeed = (_enemy.MovingSpeed - damageSpeedReduction) * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetWaypoint, movingSpeed);
    }


    public void OnDrawGizmos()
    {
        // target tile
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(TargetWaypoint, DebugSphereRadius);
    }
}
