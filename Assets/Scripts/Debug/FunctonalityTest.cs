using LightBringer.Enemies.Knight;
using UnityEngine;

public class FunctonalityTest : MonoBehaviour {

    public bool done = false;
    public Transform origin;
    public Transform destination;
    public float distance;
    public bool sightLineReq;
    public bool canGoBehindPlayer;
    public bool canGoBackward;

    public GameObject spherePrefab;

    void Start () {    
    }

    private void Update()
    {
        if (!done)
        {
            TestFunction();
            done = true;
        }
    }

    private void TestFunction()
    {
        Vector3 pos;

        for (int i = 0; i < 1000; i++)
        {
            if (Charge1Behaviour.ComputeTargetPoint(origin, destination.position, distance, sightLineReq, canGoBehindPlayer, canGoBackward, out pos))
            {
                Destroy(Instantiate(spherePrefab, pos, Quaternion.identity, null), 3);
            }
        }
    }
}
