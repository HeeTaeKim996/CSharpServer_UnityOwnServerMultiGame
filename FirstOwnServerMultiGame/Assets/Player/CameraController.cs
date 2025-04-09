using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera { get; private set; }

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
}
