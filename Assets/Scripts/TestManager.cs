using LightBringer.Enemies.Knight;
using LightBringer.Player;
using LightBringer.TerrainGeneration;
using UnityEngine;
using UnityEngine.AI;

public class TestManager : MonoBehaviour
{
    public static TestManager singleton;

    public GameObject knightPrefab;
    public GameObject playerPrefab;
    public GameObject testUI;

    private TerrainGenerator tg;
    public Terrain terrain;
    private NavMeshSurface nms;

    private GameObject knight;
    private KnightController kc;

    private PlayerStatusManager psm;
    private PlayerMotor playerMotor;

    private bool knightPassive = true;
    private bool canDie = true;

    public GameObject newTerrainButton;

    private void Start()
    {
        singleton = this;

        tg = terrain.GetComponent<TerrainGenerator>();
        nms = terrain.GetComponent<NavMeshSurface>();

        testUI.SetActive(true);
    }

    public void GenerateNewTerrain()
    {
        tg.GenerateTerrain();
        nms.BuildNavMesh();
    }

    public void SetPlayer(PlayerMotor pm)
    {
        playerMotor = pm;
        psm = pm.GetComponent<PlayerStatusManager>();

        psm.canDie = canDie;
    }

    public void RemovePlayer()
    {
        playerMotor = null;
        psm = null;
    }

    public void RestartFight()
    {
        KillKnight();

        // Player
        if (playerMotor == null)
        {
            GameObject playerGo = Instantiate(playerPrefab);
            playerMotor = playerGo.GetComponent<PlayerMotor>();
        }

        playerMotor.transform.position = Vector3.zero;
        playerMotor.Init();

        // Knight
        knight = Instantiate(knightPrefab);
        knight.transform.position = new Vector3(0, 0, 20);
        knight.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);

        kc = knight.GetComponent<KnightController>();
        kc.passive = knightPassive;
    }

    public void KillKnight()
    {
        if (knight != null)
        {
            Destroy(knight);
        }
    }

    public void SetKnightPassive(bool isPassive)
    {
        knightPassive = isPassive;
        if (kc != null)
        {
            kc.passive = knightPassive;
        }
    }

    public void SetCanDie(bool canDie)
    {
        this.canDie = canDie;
        if (psm != null)
        {
            psm.canDie = canDie;
        }
    }

    public void SetNoCD(bool noCD)
    {
        if (GameManager.gm != null)
        {
            GameManager.gm.ignoreCD = noCD;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
