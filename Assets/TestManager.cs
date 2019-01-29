using System.Collections;
using LightBringer.Enemies.Knight;
using LightBringer.Player;
using LightBringer.Player.Class;
using LightBringer.TerrainGeneration;
using LightBringer.UI;
using UnityEngine;
using UnityEngine.AI;

public class TestManager : MonoBehaviour
{
    public Terrain terrain;
    public GameObject playerPrefab;
    public GameObject knightPrefab;
    public GameObject playerCameraPrefab;
    public Camera overViewCamera;
    public UserInterface ui;


    private GameObject playerCamera;
    private GameObject player;
    private GameObject knight;
    private TerrainGenerator tg;
    private NavMeshSurface nms;
    private KnightController kc;
    private PlayerStatusManager psm;
    private bool knightPassive = true;
    private bool canDie = true;
    private bool noCD = false;

    private void Start()
    {
        tg = terrain.GetComponent<TerrainGenerator>();
        nms = terrain.GetComponent<NavMeshSurface>();
    }

    private void Update()
    {
        if (psm != null && psm.isDead)
        {
            StartCoroutine(StopAfterFrame());
        }
    }

    private IEnumerator StopAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        StopFight();
    }

    public void GenerateNewTerrain()
    {
        tg.GenerateTerrain();
        nms.BuildNavMesh();
    }

    public void RestartFight()
    {
        StopFight();
        StartCoroutine(StartAtNextFrame());
    }

    private IEnumerator StartAtNextFrame()
    {
        yield return new WaitForEndOfFrame();
        StartFight();
    }

    public void StartFight()
    {
        overViewCamera.enabled = false;

        // Player
        player = Instantiate(playerPrefab);
        player.transform.position = new Vector3(0, 0, 0);
        psm = player.GetComponent<PlayerStatusManager>();
        ui.SetCharacter(player.GetComponent<Character>());
        player.GetComponent<Character>().ignoreCD = noCD;
        psm.canDie = canDie;

        // Camera
        playerCamera = Instantiate(playerCameraPrefab);
        playerCamera.GetComponent<PlayerCamera>().character = player.transform;

        // Knight
        knight = Instantiate(knightPrefab);
        knight.transform.position = new Vector3(0, 0, 20);
        knight.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
        kc = knight.GetComponent<KnightController>();
        kc.target = player.transform;
        kc.passive = knightPassive;
    }

    public void StopFight()
    {
        ui.SetCharacter(null);

        if (player != null)
        {
            Destroy(player);
        }
        if (knight != null)
        {
            Destroy(knight);
        }
        if (playerCamera != null)
        {
            Destroy(playerCamera);
        }

        overViewCamera.enabled = true;
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
        if (player != null)
        {
            player.GetComponent<Character>().ignoreCD = noCD;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
