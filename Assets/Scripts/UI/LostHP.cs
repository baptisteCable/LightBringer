using UnityEngine;

public class LostHP : MonoBehaviour
{
    private const float SPEED = 1.2f;

    void Update ()
    {
        transform.Translate (Vector3.up * SPEED * Time.deltaTime);
    }
}
