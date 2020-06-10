using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class ExplosionController : MonoBehaviour
{
    private bool _ranUpdatedOnce = false;

    public int Damage = 5;


    public void FixedUpdate()
    {
        // run for a single PHYSICS frame, hit all enemies inside the AOE and visualize the explosion
        if (_ranUpdatedOnce)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _ranUpdatedOnce = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // hit all the targets in the area of effect
        EnemyController enemy;
        if ((enemy = collider.gameObject.GetComponent<EnemyController>()) != null)
        {
            enemy.Hit(Damage);
        }
    }
}
