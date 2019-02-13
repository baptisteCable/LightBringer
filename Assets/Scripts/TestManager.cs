using LightBringer.Enemies.Knight;
using LightBringer.Player;
using LightBringer.TerrainGeneration;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class TestManager : NetworkBehaviour
{
    public static TestManager singleton;
    
    public GameObject knightPrefab;
    
    private TerrainGenerator tg;
    public Terrain terrain;
    private NavMeshSurface nms;

    private GameObject knight;
    private KnightController kc;
    private GameObject knight2;
    private KnightController kc2;

    private PlayerStatusManager psm;
    private PlayerMotor playerMotor;

    private bool knightPassive = true;
    private bool canDie = true;
    private bool noCD = false;

    public GameObject newTerrainButton;

    private void Start()
    {
        singleton = this;

        tg = terrain.GetComponent<TerrainGenerator>();
        nms = terrain.GetComponent<NavMeshSurface>();

        newTerrainButton.SetActive(isServer);

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
        playerMotor.ignoreCD = noCD;
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
        playerMotor.transform.position = Vector3.zero;
        playerMotor.CmdServerInit();

        if (!isServer)
        {
            return;
        }

        // Knight
        knight = Instantiate(knightPrefab);
        knight.transform.position = new Vector3(0, 0, 20);
        knight.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);

        kc = knight.GetComponent<KnightController>();
        kc.target = playerMotor.transform;
        kc.passive = knightPassive;

        NetworkServer.Spawn(knight);

        // Knight 2
        knight2 = Instantiate(knightPrefab);
        knight2.transform.position = new Vector3(10, 0, 20);
        knight2.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);

        kc2 = knight2.GetComponent<KnightController>();
        kc2.target = playerMotor.transform;
        kc2.passive = knightPassive;

        NetworkServer.Spawn(knight2);
    }

    public void KillKnight()
    {
        if (knight != null)
        {
            Destroy(knight);
        }
        if (knight2 != null)
        {
            Destroy(knight2);
        }
    }

    public void SetKnightPassive(bool isPassive)
    {
        knightPassive = isPassive;
        if (kc != null)
        {
            kc.passive = knightPassive;
        }
        if (kc2 != null)
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
        this.noCD = noCD;
        if (playerMotor != null)
        {
            playerMotor.ignoreCD = noCD;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
