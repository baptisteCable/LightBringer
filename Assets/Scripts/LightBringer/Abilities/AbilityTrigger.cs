using UnityEngine;

namespace LightBringer.Abilities
{
    public class AbilityTrigger : MonoBehaviour
    {
        protected Mesh mesh;
        protected MeshCollider meshCollider;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        static protected void CreateAngularAoEMesh(float distance, float angle, float height, ref Mesh mesh)
        {
            Vector3[] vertices;
            int[] triangles;
            int nbVert = (int)Mathf.Ceil(angle / 22.5f) + 2;

            vertices = new Vector3[2 * nbVert];
            vertices[0] = new Vector3(0, 0, 0);
            vertices[nbVert] = new Vector3(0, height, 0);

            for (int i = 0; i < nbVert - 1; i++)
            {
                vertices[i + 1] = cartFromPol(distance, -angle / 2 + i * (angle / (nbVert - 2)), 0);
                vertices[nbVert + i + 1] = cartFromPol(distance, -angle / 2 + i * (angle / (nbVert - 2)), height);
            }

            triangles = new int[6 * (nbVert - 2)];
            for (int i = 0; i < nbVert - 2; i++)
            {
                // lower side
                triangles[3 * i] = 0;
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;

                // upper side
                triangles[3 * (nbVert - 2 + i)] = nbVert + 0;
                triangles[3 * (nbVert - 2 + i) + 1] = nbVert + i + 1;
                triangles[3 * (nbVert - 2 + i) + 2] = nbVert + i + 2;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;

        }

        static Vector3 cartFromPol(float module, float angle, float y)
        {
            return new Vector3(
                    Mathf.Cos(Mathf.Deg2Rad * (90f - angle)) * module,
                    y,
                    Mathf.Sin(Mathf.Deg2Rad * (90f - angle)) * module
                );
        }
    }
}