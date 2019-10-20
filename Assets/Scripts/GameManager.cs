using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const float FLOOR_HEIGHT = 5f;
    private const int ENEMY_MASK_INDEX_COUNT = 12;

    public const float GRAVITY = 10f;
    public bool staticCamera;

    public int currentFloor = 0;
    public Plane floorPlane;
    public float currentAlt = 0f;

    public static GameManager gm;

    private int lastEnemyMaskIndex = 1;

    // Training
    [HideInInspector] public bool ignoreCD = false;

    // Use this for initialization
    void Start ()
    {
        if (gm != null)
        {
            throw new System.Exception ("Multiple game managers");
        }
        gm = this;
    }

    // Increment 2 by 2 because we must use -1 and +1 layer to render sprites
    public int GetNextEnemyMaskIndex ()
    {
        lastEnemyMaskIndex = (lastEnemyMaskIndex + 2) % ENEMY_MASK_INDEX_COUNT;
        return lastEnemyMaskIndex;
    }
}
