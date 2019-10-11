using LightBringer.Player;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    private const float ZOOM_LENGTH = 10f;
    private const float SCROLL_SPEED = 1f;

    private Camera cam;
    public Transform player;
    private Vector3 camPositionFromPlayer;
    private float currentPosition; // 0 for clostest, 1 for farthest
    private float targetXRotation;
    private float currentXRotation;
    private float targetFieldOfView;

    private PlayerController pc;

    private AnimationCurve xPos, yPos, zPos, xRot, fieldOfView;

    private void Start()
    {
        pc = player.GetComponent<PlayerController>();

        cam = GetComponent<Camera>();
        GameManager.gm.floorPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, GameManager.gm.currentAlt, 0));

        ComputeCurves();

        currentPosition = 0f;
        currentXRotation = 70f;
        ChangeTargetFromCurrent();
    }

    private void ComputeCurves()
    {
        xPos = new AnimationCurve();
        yPos = new AnimationCurve();
        zPos = new AnimationCurve();
        xRot = new AnimationCurve();
        fieldOfView = new AnimationCurve();

        // top position
        xPos.AddKey(new Keyframe(0f, -12f, 0, 18));
        yPos.AddKey(new Keyframe(0f, 40f, 0, -60));
        zPos.AddKey(new Keyframe(0f, -12f, 0, 18));
        xRot.AddKey(new Keyframe(0f, 70f));
        fieldOfView.AddKey(new Keyframe(0, 41f));

        // bot position
        xPos.AddKey(new Keyframe(.5f, -2.26f));
        yPos.AddKey(new Keyframe(.5f, 9.2f, 0f, 0f));
        zPos.AddKey(new Keyframe(.5f, -2.26f));
        xRot.AddKey(new Keyframe(.5f, 70f));
        fieldOfView.AddKey(new Keyframe(0, 41f));

        // bot horizontal position
        xPos.AddKey(new Keyframe(1f, -2.5f));
        yPos.AddKey(new Keyframe(1f, 5f, 0f, 0f));
        zPos.AddKey(new Keyframe(1f, -2.5f));
        xRot.AddKey(new Keyframe(1f, 54f));
        fieldOfView.AddKey(new Keyframe(1f, 55.5f));
    }

    private void Update()
    {
        MoveCamera();
        Zoom();
    }

    void MoveCamera()
    {
        if (GameManager.gm.staticCamera)
        {
            cam.transform.position = new Vector3(
                    player.position.x + camPositionFromPlayer.x,
                    camPositionFromPlayer.y,
                    player.position.z + camPositionFromPlayer.z
                );
        }
        else
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(
                    player.position.x + camPositionFromPlayer.x + (pc.pointedWorldPoint.x - player.position.x) * .3f,
                    camPositionFromPlayer.y,
                    player.position.z + camPositionFromPlayer.z + (pc.pointedWorldPoint.z - player.position.z) * .3f
                ), Time.deltaTime * 8f);
        }
    }

    private void Zoom()
    {
        float scrollValue = Input.GetAxisRaw("MouseScrollWheel");

        if (scrollValue != 0f)
        {
            currentPosition = Mathf.Clamp(currentPosition + scrollValue * SCROLL_SPEED, 0, 1);
            ChangeTargetFromCurrent();
        }

        if (Mathf.Abs(currentXRotation - targetXRotation) > 1e-5f)
        {
            SlipToCurrentRotationAndFov();
        }
    }

    private void ChangeTargetFromCurrent()
    {
        camPositionFromPlayer = new Vector3(
                xPos.Evaluate(currentPosition),
                yPos.Evaluate(currentPosition),
                zPos.Evaluate(currentPosition)
            );
        targetXRotation = xRot.Evaluate(currentPosition);
        targetFieldOfView = fieldOfView.Evaluate(currentPosition);
    }

    private void SlipToCurrentRotationAndFov()
    {
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * 8f);
        cam.transform.rotation = Quaternion.Euler(currentXRotation, 45f, 0);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFieldOfView, Time.deltaTime * 8f);
    }
}
