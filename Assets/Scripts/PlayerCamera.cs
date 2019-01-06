using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{

    private Camera cam;
    public Transform character;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        GetLookedPoint();
        MoveCamera();
    }

    void GetLookedPoint()
    {
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        float distance;

        if (GameManager.gm.lookingPlane.Raycast(mouseRay, out distance))
        {
            GameManager.gm.lookedPoint = mouseRay.GetPoint(distance);
        }
    }

    void MoveCamera()
    {
        if (GameManager.gm.staticCamera)
        {
            cam.transform.position = new Vector3(
                    character.position.x + GameManager.gm.camPositionFromPlayer.x,
                    GameManager.gm.camPositionFromPlayer.y,
                    character.position.z + GameManager.gm.camPositionFromPlayer.z
                );
        }
        else
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(
                    character.position.x + GameManager.gm.camPositionFromPlayer.x + (GameManager.gm.lookedPoint.x - character.position.x) * .3f,
                    GameManager.gm.camPositionFromPlayer.y,
                    character.position.z + GameManager.gm.camPositionFromPlayer.z + (GameManager.gm.lookedPoint.z - character.position.z) * .3f
                ), Time.deltaTime * 8f);
        }
    }
}
