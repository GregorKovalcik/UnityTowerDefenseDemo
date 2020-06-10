using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls user interface overlay.
/// (menu screens, game related buttons and labels, etc.)
/// </summary>
[RequireComponent(typeof(GameController))]
public class UIController : MonoBehaviour
{
    private GameController _game;

    public GameObject Canvas;
    public GameObject[] Panels;
    public Button GameMenuButton;
    public Text LevelText;
    public Text ScoreText;
    public Text MoneyText;


    public void Start()
    {
        _game = GetComponent<GameController>();
    }

    public void OnEnable()
    {
        SetActivePanel(0);
    }

    public void Update()
    {
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (LevelText != null)
        {
            LevelText.text = _game.Level.LoadedLevel;
        }
        if (ScoreText != null)
        {
            ScoreText.text = _game.Score.ToString();
        }
        if (MoneyText != null)
        {
            MoneyText.text = _game.Money.ToString();
        }
    }



    /// <summary>
    /// Inspired by Unity Platformer template.
    /// </summary>
    /// <param name="index"></param>
    public void SetActivePanel(int index)
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            bool isActive = (i == index);
            GameObject panel = Panels[i];
            if (panel.activeSelf != isActive)
            {
                panel.SetActive(isActive);
            }
        }

        GameMenuButton.interactable = false;
    }

    public void HideAllPanels()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            Panels[i].SetActive(false);
        }

        GameMenuButton.interactable = true;
    }
}
