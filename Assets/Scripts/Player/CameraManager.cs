using UnityEngine;

public class CameraManager : MonoBehaviour {
    public static CameraManager singleton;

    public GameObject overviewCamera;

    private void Start()
    {
        CameraManager.singleton = this;
    }

    public void ActivatePlayerCamera()
    {
        overviewCamera.SetActive(false);
    }

    public void DisactivatePlayerCamera()
    {
        overviewCamera.SetActive(true);
    }
}
