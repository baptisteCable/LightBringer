using UnityEngine;
namespace LightBringer.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class CollisionManager : MonoBehaviour
    {
        CharacterController cc;
        private float radius;
        LayerMask mask;
        // [SerializeField] private PlayerMotor motor;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            radius = cc.radius;
            mask = LayerMask.GetMask("Enemy");
        }

        void FixedUpdate()
        {
            if (LayerMask.LayerToName(gameObject.layer) == "Player")
            {
                CollisionManagement();
            }
        }

        void CollisionManagement()
        {
            Vector3 point0;
            Vector3 point1;

            GetCapsuleInfo(out point0, out point1);


            Collider[] cols = Physics.OverlapCapsule(point0, point1, radius, mask);

            foreach (Collider col in cols)
            {
                if (!col.isTrigger /*&& motor.GetMovementMode() != MovementMode.Anchor*/)
                {
                    Debug.Log(col.name);
                    Depenetrate(col);
                }
            }
        }

        private void GetCapsuleInfo(out Vector3 point0, out Vector3 point1)
        {
            point0 = transform.TransformPoint(cc.center + Vector3.up * cc.height / 2f);
            point1 = transform.TransformPoint(cc.center - Vector3.up * cc.height / 2f);
        }

        private void Depenetrate(Collider col)
        {
            if (col.GetType() == typeof(CharacterController))
            {
                CharacterControllerDepenetration((CharacterController)col);
            }
            else if (col.GetType() == typeof(BoxCollider))
            {
                BoxColliderDepenetration((BoxCollider)col);
            }
        }

        private void CharacterControllerDepenetration(CharacterController ccCol)
        {
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

        // TODO: Vertical collision and vertex in cylinder collision
        private void BoxColliderDepenetration(BoxCollider boxCol)
        {
            Vector3 colWorldCenter = boxCol.transform.TransformPoint(boxCol.center);
            float scale = boxCol.transform.lossyScale.x;

            // Box collider properties
            Vector3 xDir = boxCol.transform.right;
            Vector3 yDir = boxCol.transform.up;
            Vector3 zDir = boxCol.transform.forward;
            Vector3 boxColSize = boxCol.size * scale / 2f;

            // Bottom point of the player collider
            Vector3 playerP0 = cc.transform.TransformPoint(cc.center - cc.transform.up * (cc.height / 2f - radius));
            Vector3 playerP1 = cc.transform.TransformPoint(cc.center + cc.transform.up * (cc.height / 2f - radius));

            Vector3[] depenetrations = new Vector3[6];

            depenetrations[0] = HalfSphereDepenetration(colWorldCenter, boxColSize.z, boxColSize.x, boxColSize.y,
                zDir, xDir, yDir, playerP0, false);
            depenetrations[1] = HalfSphereDepenetration(colWorldCenter, boxColSize.x, boxColSize.y, boxColSize.z,
                xDir, yDir, zDir, playerP0, false);
            depenetrations[2] = HalfSphereDepenetration(colWorldCenter, boxColSize.y, boxColSize.z, boxColSize.x,
                yDir, zDir, xDir, playerP0, false);
            depenetrations[3] = HalfSphereDepenetration(colWorldCenter, boxColSize.z, boxColSize.x, boxColSize.y,
                zDir, xDir, yDir, playerP1, true);
            depenetrations[4] = HalfSphereDepenetration(colWorldCenter, boxColSize.x, boxColSize.y, boxColSize.z,
                xDir, yDir, zDir, playerP1, true);
            depenetrations[5] = HalfSphereDepenetration(colWorldCenter, boxColSize.y, boxColSize.z, boxColSize.x,
                yDir, zDir, xDir, playerP1, true);

            // find the smallest depenetration
            Vector3 finalDepen = Vector3.positiveInfinity;
            foreach (Vector3 depen in depenetrations)
            {
                if (depen.magnitude < finalDepen.magnitude)
                {
                    finalDepen = depen;
                }
            }

            // apply the depenetration
            if (!float.IsPositiveInfinity(finalDepen.magnitude))
            {
                transform.position += finalDepen;
            }
        }

        private Vector3 HalfSphereDepenetration(Vector3 colWorldCenter, float mainSize, float size1, float size2,
            Vector3 mainDir, Vector3 dir1, Vector3 dir2, Vector3 sphereCenter, bool topHalfSphere)
        {
            Vector3 depenetration = Vector3.positiveInfinity;

            Vector3 colToSphereCenter = sphereCenter - colWorldCenter;

            // Face to consider
            float sign = Mathf.Sign(Vector3.Dot(colToSphereCenter, mainDir));
            Vector3 midPlanePoint = colWorldCenter + mainDir * sign * mainSize;

            // if the sphere center is on the wrong side of the face, no depenetration
            if (Vector3.Dot(sign * mainDir, sphereCenter - midPlanePoint) <= 0)
            {
                return depenetration;
            }

            // Closest point to sphereCenter on the plane (can be out of the face)
            Vector3 closestPointOnPlane = midPlanePoint
                + Vector3.Dot(dir1, colToSphereCenter) * dir1
                + Vector3.Dot(dir2, colToSphereCenter) * dir2;

            // If out of the sphere, null depenetration
            if ((closestPointOnPlane - sphereCenter).magnitude >= radius)
            {
                return depenetration;
            }

            // Find the tangent point on the sphere, that should be touching the sphere in the end
            Vector3 ray = (closestPointOnPlane - sphereCenter).normalized * radius;
            Vector3 sphereTangentPoint = sphereCenter + ray;

            // Horizontal projection
            Vector3 horizontalRay = ray;
            horizontalRay.y = 0;

            // If plane parallel to ray, no depenetration
            if (horizontalRay.magnitude == 0)
            {
                return depenetration;
            }

            // find the intersection between the horizontal ray and the plane. This point should be moved to the tangent point of
            // the sphere. t is the value in the parametric representation of the line.
            float t = Vector3.Dot(ray, closestPointOnPlane - sphereTangentPoint) / Vector3.Dot(horizontalRay, horizontalRay);

            // intersection point
            Vector3 intersection = sphereTangentPoint + t * horizontalRay;

            // If intersection not on the face, take the closest point on the face
            intersection = ClosestPointOnFace(intersection, midPlanePoint, dir1, dir2, size1, size2);

            // if closest point out of the sphere, no depen
            if ((intersection - sphereCenter).magnitude >= radius)
            {
                return depenetration;
            }

            // if bottom half-sphere
            if ((!topHalfSphere && intersection.y < sphereCenter.y) || (topHalfSphere && intersection.y > sphereCenter.y))
            {
                // point on the sphere aligned to the intersection point, following the depentration vector
                Vector3 spherePoint = LineAndSphereIntersection(intersection, horizontalRay, sphereCenter, radius);
                depenetration = intersection - spherePoint;
            }

            return depenetration;
        }

        // 
        private Vector3 ClosestPointOnFace(Vector3 point, Vector3 midPlanePoint, Vector3 dir1, Vector3 dir2, float size1, float size2)
        {
            float coord1 = Mathf.Min(size1, Mathf.Max(-size1, Vector3.Dot(dir1, point - midPlanePoint)));
            float coord2 = Mathf.Min(size2, Mathf.Max(-size2, Vector3.Dot(dir2, point - midPlanePoint)));

            return midPlanePoint + coord1 * dir1 + coord2 * dir2;
        }

        private Vector3 LineAndSphereIntersection(Vector3 linePoint, Vector3 lineDirection, Vector3 sphereCenter, float radius)
        {
            Vector3 u = lineDirection;
            Vector3 v = linePoint - sphereCenter;

            // t is the parameter of parametric representation of the line.
            // At intersection, t followes the equation : t^2 * ||u||^2 + 2 * t * u.v + ||v||^2 - radius^2 = 0
            float delta = 4f * (Mathf.Pow(Vector3.Dot(u, v), 2) - Vector3.Dot(u, u) * (Vector3.Dot(v, v) - radius * radius));
            float t = (-2f * Vector3.Dot(u, v) + Mathf.Sqrt(delta)) / (2f * Vector3.Dot(u, u));
            return linePoint + lineDirection * t;
        }
    }
}