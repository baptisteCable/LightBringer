using UnityEngine;
namespace LightBringer.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class CollisionManager : MonoBehaviour
    {
        CharacterController cc;
        private float radius;
        LayerMask mask;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            radius = cc.radius;
            mask = LayerMask.GetMask("Enemy");
        }

        void FixedUpdate()
        { 
            Vector3 point0;
            Vector3 point1;

            GetCapsuleInfo(out point0, out point1);
            

            Collider[] cols = Physics.OverlapCapsule(point0, point1, radius, mask);

            foreach (Collider col in cols)
            {
                if (!col.isTrigger)
                {
                    Debug.Log(col.name);

                    Depenetrate(col);
                }
            }
        }

        private void GetCapsuleInfo(out Vector3 point0, out Vector3 point1)
        {
            point0 = transform.position + cc.center + Vector3.up * cc.height / 2f;
            point1 = transform.position + cc.center - Vector3.up * cc.height / 2f;
        }

        private void Depenetrate(Collider col)
        {
            if (col.GetType() == typeof(CharacterController))
            {
                CharacterController ccCol = (CharacterController)col;

                Vector3 direction;
                float distance;

                Vector3 colWorldCenter = ccCol.transform.TransformPoint(ccCol.center);

                direction = transform.position - colWorldCenter;
                direction.y = 0;
                direction.Normalize();

                distance = radius + ccCol.radius;

                Vector3 newPosition = ccCol.transform.position + direction * distance;
                newPosition.y = 0;
                transform.position = newPosition;
            }
            else if (col.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider capsCol = (CapsuleCollider)col;

                Vector3 direction;
                float distance;

                direction = cc.center + transform.position - capsCol.transform.position - capsCol.center;
                direction.y = 0;
                direction.Normalize();

                distance = radius + capsCol.radius;

                Vector3 newPosition = capsCol.transform.position + direction * distance;
                newPosition.y = 0;
                transform.position = newPosition;
            }
            else if (col.GetType() == typeof(BoxCollider))
            {
                BoxCollider boxCol = (BoxCollider)col;

                float xPenetrationDepth;
                float zPenetrationDepth;

                float scale = boxCol.transform.lossyScale.x;

                Vector3 colWorldCenter = boxCol.transform.TransformPoint(boxCol.center);

                Vector3 colXDir = boxCol.transform.right;
                float xDist = Vector3.Dot(colXDir, transform.position - colWorldCenter);
                xPenetrationDepth = boxCol.size.x / 2f * scale + radius - Mathf.Abs(xDist);

                Vector3 colZDir = boxCol.transform.forward;
                Debug.Log("ColZDir: " + colZDir);
                float zDist = Vector3.Dot(colZDir, transform.position - colWorldCenter);
                zPenetrationDepth = boxCol.size.z / 2f * scale + radius - Mathf.Abs(zDist);

                Vector3 depenetration;
                if (Mathf.Abs(zPenetrationDepth) <= Mathf.Abs(xPenetrationDepth))
                {
                    depenetration = colZDir * zPenetrationDepth * Mathf.Sign(zDist);
                }
                else
                {
                    depenetration = colXDir * xPenetrationDepth * Mathf.Sign(xDist);
                }

                Vector3 newPosition = transform.position + depenetration;
                newPosition.y = 0;
                transform.position = newPosition;
            }

        }
    }
}