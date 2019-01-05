using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshCollider))]
public class ConeMesh : MonoBehaviour {
    public float distance;
    public float angle;
    public float top;

    private Mesh s;

    void Start () {
        
        CreateAngularAoEMesh(distance, angle, top);
    }

    private void CreateAngularAoEMesh(float distance, float angle, float height)
    {
        s = new Mesh();
        
        Vector3[] vertices;
        int[] triangles;
        int j = 0;

        int nbVert = (int)Mathf.Ceil(angle / 22.5f) + 2;

        vertices = new Vector3[2 * nbVert];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[nbVert] = new Vector3(0, height, 0);

        for (int i = 0; i < nbVert - 1; i++)
        {
            vertices[i + 1] = cartFromPol(distance, -angle / 2 + i * (angle / (nbVert - 2)), 0);
            vertices[nbVert + i + 1] = cartFromPol(distance, -angle / 2 + i * (angle / (nbVert - 2)), height);
        }

        triangles = new int[12 * (nbVert - 1)];
        for (int i = 0; i < nbVert - 2; i++)
        {
            // lower side
            triangles[j++] = 0;
            triangles[j++] = i + 1;
            triangles[j++] = i + 2;

            // upper side
            triangles[j++] = nbVert + 0;
            triangles[j++] = nbVert + i + 1;
            triangles[j++] = nbVert + i + 2;

            // front 1
            triangles[j++] = nbVert + i + 1;
            triangles[j++] = i + 1;
            triangles[j++] = i + 2;

            // front 2
            triangles[j++] = nbVert + i + 2;
            triangles[j++] = nbVert + i + 1;
            triangles[j++] = i + 2;
        }

        triangles[j++] = 0;
        triangles[j++] = 1;
        triangles[j++] = nbVert;
        triangles[j++] = 1;
        triangles[j++] = nbVert + 1;
        triangles[j++] = nbVert;

        triangles[j++] = 0;
        triangles[j++] = 2 * nbVert - 1;
        triangles[j++] = nbVert - 1;
        triangles[j++] = 0;
        triangles[j++] = nbVert;
        triangles[j++] = 2 * nbVert - 1;
        
        s.vertices = vertices;
        s.triangles = triangles;

        if (GetComponent<MeshFilter>())
        {
            GetComponent<MeshFilter>().mesh = s;
        }
        if (GetComponent<MeshCollider>())
        {
            GetComponent<MeshCollider>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = s;
        }

        s.name = "S";
    }

    private Vector3 cartFromPol(float module, float angle, float y)
    {
        return new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * (90f - angle)) * module,
                y,
                Mathf.Sin(Mathf.Deg2Rad * (90f - angle)) * module
            );
    }
}
