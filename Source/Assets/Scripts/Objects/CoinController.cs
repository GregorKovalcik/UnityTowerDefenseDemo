using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Coin settings and controls.
/// (value, mouse pointer attraction and collecting)
/// </summary>
public class CoinController : MonoBehaviour
{
    private GameController _game;

    public int Value = 1;
    public float CollectableRange = 0.5f;
    public float AttractableRange = 2f;
    public float AttractionSpeed = 1;
    public float DecayTime = 30;

    void Start()
    {
        _game = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<GameController>();

        if (DecayTime > 0)
        {
            Destroy(this.gameObject, DecayTime);
        }
    }

    void Update()
    {
        // collect or attract towards pointer
        if (GameController.IsInputActive())
        {
            Vector3 pointerPosition = GameController.GetWorldPositionFromInput();

            float distance = Vector3.Distance(transform.position, pointerPosition);

            // collect 
            if (distance < CollectableRange)
            {
                Collect();
            }

            // attract
            else if (distance < AttractableRange)
            {
                AttractTowards(pointerPosition);
            }
        }
    }


    private void Collect()
    {
        _game.AddMoney(Value);
        Destroy(this.gameObject);
    }

    private void AttractTowards(Vector3 target)
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, target, AttractionSpeed * Time.deltaTime);
    }
}
