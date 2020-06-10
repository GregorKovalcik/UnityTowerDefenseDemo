using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls overall game application (shutting down, etc.).
/// </summary>
public class AppController : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
    }
}
