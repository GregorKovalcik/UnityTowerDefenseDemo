using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Adds a click sound to a button.
/// </summary>
public class ButtonClickController : MonoBehaviour
{
    private AudioSource _audioSource;

    public AudioClip AudioClip;
    public void Start()
    {
        _audioSource = GameObject.Find(GameController.GAMECONTROLLER_STRING).GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(() => PlayClickSound());
    }

    private void PlayClickSound()
    {
        if (AudioClip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(AudioClip);
        }
    }
}
