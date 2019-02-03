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

    private PlayerStatusManager psm;
    private PlayerMotor playerMotor;

    private bool knightPassive = true;
    private bool canDie = true;
    private bool noCD = false;

    public GameObject newTerrainButton;

    private void Start()
    {
        Debug.Log("Démarrage du test");

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
        playerMotor.Init();

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
        if (kc!= null)
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
