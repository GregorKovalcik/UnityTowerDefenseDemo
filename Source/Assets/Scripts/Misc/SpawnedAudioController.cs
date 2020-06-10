using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Plays an audio clip (once or looped) when a gameObject is spawned.
/// (enemy noises, rocket and explosion sounds, etc.)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SpawnedAudioController : MonoBehaviour
{
    private Random _random;
    private AudioSource _audioSource;

    public bool IsLooped = false;
    public AudioClip[] AudioClips;

    void Start()
    {
        _random = new Random(gameObject.GetInstanceID());
        _audioSource = GetComponent<AudioSource>();

        // play sound
        if (_audioSource != null && AudioClips != null && AudioClips.Length != 0)
        {
            if (IsLooped)
            {
                _audioSource.clip = AudioClips[_random.Next(0, AudioClips.Length)];
                _audioSource.loop = true;
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot(AudioClips[_random.Next(0, AudioClips.Length)]);
            }
        }
    }


}
