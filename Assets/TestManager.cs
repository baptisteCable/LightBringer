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
    private bool knightPassive = true;
    private PlayerStatusManager psm;

    private void Start()
    {
        tg = terrain.GetComponent<TerrainGenerator>();
        nms = terrain.GetComponent<NavMeshSurface>();
    }

    private void Update()
    {
        if (kc != null)
        {
            kc.passive = knightPassive;
        }
        if (psm != null && psm.isDead)
        {
            StartCoroutine(StopInXSec(.5f));
        }
    }

    private IEnumerator StopInXSec(float duration)
    {
        yield return new WaitForSeconds(duration);
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
        StartFight();
    }

    public void StartFight()
    {
        overViewCamera.enabled = false;

        player = Instantiate(playerPrefab);
        player.transform.position = new Vector3(0, 0, 0);
        psm = player.GetComponent<PlayerStatusManager>();
        ui.SetCharacter(player.GetComponent<Character>());

        playerCamera = Instantiate(playerCameraPrefab);
        playerCamera.GetComponent<PlayerCamera>().character = player.transform;

        knight = Instantiate(knightPrefab);
        knight.transform.position = new Vector3(0, 0, 20);
        knight.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
        kc = knight.GetComponent<KnightController>();
        kc.target = player.transform;
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
    }
}
