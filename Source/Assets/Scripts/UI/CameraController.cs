using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controls camera position.
/// Used primarily to snap camera to right edge of the game tilemap.
/// This is done to display game properly using various display aspect ratios.
/// 
/// Secondary, it manages camera height for smaller than designed aspect ratios.
/// </summary>
public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private float _designWidth;
    
    public float CameraRightEdgeXCoordinate = 13.72f;
    public float DesignOrthographicSize = 4;
    public float DesignAspectRatio = (float)(16 / 9);

    void Start()
    {
        _camera = GetComponent<Camera>();
        _designWidth = DesignOrthographicSize * DesignAspectRatio;
    }

    void LateUpdate()
    {
        // adjust camera height to satisfy design aspect ratio (expected camera width)
        _camera.orthographicSize = Mathf.Max(DesignOrthographicSize, _designWidth / _camera.aspect);

        // align camera to the right edge of game field
        Vector3 oldPosition = _camera.transform.position;
        float cameraHalfWidth = _camera.orthographicSize * 2f * _camera.aspect / 2f;
        float newX = -cameraHalfWidth + CameraRightEdgeXCoordinate;
        _camera.transform.position = new Vector3(newX, oldPosition.y, oldPosition.z);
    }
}
