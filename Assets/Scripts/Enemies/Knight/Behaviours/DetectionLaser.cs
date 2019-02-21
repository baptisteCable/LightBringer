using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class DetectionLaser : MonoBehaviour
    {
        [SerializeField] private Transform[] bones;
        [SerializeField] private float length = 4;
        [SerializeField] private float height = 1;
        [SerializeField] private float width = 2;

        [SerializeField] private bool done = true;

        private Mesh mesh;
        private SkinnedMeshRenderer rend;

        void Start()
        {

            CreateMesh();
        }

        private void Update()
        {
            if (!done)
            {
                done = true;
                CreateMesh();
            }
        }

        private void CreateMesh()
        {
            mesh = new Mesh();
            mesh.name = "S";

            rend = GetComponent<SkinnedMeshRenderer>();

            Vector3[] vertices = new Vector3[5];

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(-width / 2, height / 2, length);
            vertices[2] = new Vector3(width / 2, height / 2, length);
            vertices[3] = new Vector3(width / 2, -height / 2, length);
            vertices[4] = new Vector3(-width / 2, -height / 2, length);

            mesh.vertices = vertices;

            int[] triangles = new int[12];

            // Top triangle
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            // Right triangle
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            // Bot triangle
            triangles[6] = 0;
            triangles[7] = 3;
            triangles[8] = 4;

            // Left triangle
            triangles[9] = 0;
            triangles[10] = 4;
            triangles[11] = 1;

            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            BoneWeight[] weights = new BoneWeight[5];
            weights[0].boneIndex0 = 0;
            weights[0].weight0 = 1;
            weights[1].boneIndex0 = 1;
            weights[1].weight0 = 1;
            weights[2].boneIndex0 = 2;
            weights[2].weight0 = 1;
            weights[3].boneIndex0 = 2;
            weights[3].weight0 = 1;
            weights[4].boneIndex0 = 1;
            weights[4].weight0 = 1;
            mesh.boneWeights = weights;
            
            Matrix4x4[] bindPoses = new Matrix4x4[3];
            bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;
            bindPoses[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;
            bindPoses[2] = bones[2].worldToLocalMatrix * transform.localToWorldMatrix;

            mesh.bindposes = bindPoses;

            rend.bones = bones;
            rend.sharedMesh = mesh;
        }
    }

}
