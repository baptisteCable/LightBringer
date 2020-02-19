#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Combiner : MonoBehaviour
{

    public bool combineMeshes = true;

    private void Update ()
    {
        if (!combineMeshes)
        {
            combineMeshes = true;
            Combine ();
        }
    }

    void Combine ()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter> ();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            i++;
        }
        Mesh m = new Mesh ();
        m.CombineMeshes (combine);

        AssetDatabase.CreateAsset (m, "Assets/m.asset");
    }
}
#endif