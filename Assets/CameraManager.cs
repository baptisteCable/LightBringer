using UnityEngine;

public class CameraManager : MonoBehaviour {
    public static CameraManager singleton;

    public Camera overViewCamera;

    private void Start()
    {
        CameraManager.singleton = this;
    }

    public void ActivatePlayerCamera()
    {
        overViewCamera.enabled = false;
    }

    public void DisactivatePlayerCamera()
    {
        overViewCamera.enabled = true;
    }
}
