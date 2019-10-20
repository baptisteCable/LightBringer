using UnityEngine;

[RequireComponent (typeof (CapsuleCollider))]
public class ViewLine : MonoBehaviour
{
    [SerializeField] CapsuleCollider col = null;

    // Update is called once per frame
    void Update ()
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 vec = camPos - col.transform.position;
        col.height = vec.magnitude + 2;
        col.center = Vector3.forward * (col.height / 2f - 1);
        col.transform.rotation = Quaternion.LookRotation (vec);
    }
}
