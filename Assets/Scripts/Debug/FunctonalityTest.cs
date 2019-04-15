using LightBringer.Enemies;
using UnityEngine;

public class FunctonalityTest : MonoBehaviour
{

    public bool done = false;
    public Transform enemy;
    public Transform player;
    public float distanceOrMin;
    public float maxDistance;
    public bool sightLineReq;
    public bool canGoBehindPlayer;
    public bool canGoBackward;
    public float minChargeDist;
    public float maxChargeDist;

    public GameObject spherePrefab;

    void Start()
    {
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
            if (maxDistance > 0)
            {
                if (Controller.CanFindTargetPoint(enemy, player.position, distanceOrMin, maxDistance, sightLineReq, canGoBehindPlayer,
                    canGoBackward, minChargeDist, maxChargeDist, out pos))
                {
                    Destroy(Instantiate(spherePrefab, pos, Quaternion.identity, null), 3);
                }
            }
            else
            {
                if (Controller.CanFindTargetPoint(enemy, player.position, distanceOrMin, canGoBehindPlayer,
                    canGoBackward, minChargeDist, maxChargeDist, out pos))
                {
                    Destroy(Instantiate(spherePrefab, pos, Quaternion.identity, null), 3);
                }
            }
        }
    }
}
