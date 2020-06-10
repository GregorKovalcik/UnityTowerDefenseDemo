using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handles toggling and visualization of "build tower" buttons.
/// </summary>
[RequireComponent(typeof(Button))]
public class BuildButtonController : MonoBehaviour
{
    private GameController _game;
    private Button _button;
    private bool _isActivated = false;

    public TowerController Tower;
    public float DeactivatedAlpha = 0.2f;


    public void Start()
    {
        _game = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<GameController>();
        _button = GetComponent<Button>();
        SetButtonAlpha(DeactivatedAlpha);
    }

    public void Activate()
    {
        _isActivated = true;
        _game.SetPlacingTower(Tower);
        SetButtonAlpha(1);
    }

    public void Deactivate()
    {
        _isActivated = false;
        SetButtonAlpha(DeactivatedAlpha);
    }

    public void Toggle()
    {
        if (_isActivated)
        {
            Deactivate();
            _game.SetPlacingTower(null);
        }
        else if (_game.Money >= Tower.Price)
        {
            Activate();
        }
    }

    private void SetButtonAlpha(float alpha)
    {
        ColorBlock colors = _button.colors;
        Color color = colors.normalColor;
        
        color.a = alpha;

        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;

        _button.colors = colors;
    }
}
