using UnityEngine;
using System.Collections;

public class SwitchCamera : MonoBehaviour
{
    [SerializeField]
    int newCamera = 0;

    public void SwitchToCamera (int cameraIndex)
    {
        CameraManager.SetActiveCamera(cameraIndex, 0.5f);
    }
}
