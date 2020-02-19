﻿using LightBringer.Enemies.Knight;
using LightBringer.Player;
using LightBringer.TerrainGeneration;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    public static TestManager singleton;

    public GameObject knightPrefab;
    public GameObject playerPrefab;
    public GameObject testUI;

    public WorldManager wm;
    private NavMeshSurface nms;

    private GameObject knight;
    private KnightController kc;

    private PlayerStatusManager psm;
    private PlayerMotor playerMotor;

    private bool knightPassive = true;
    private bool canDie = true;
    private bool highSpeed = true;

    [SerializeField] private Vector3 playerSpawnCoord = Vector3.zero;
    [SerializeField] private Vector3 knightSpawnCoord = Vector3.zero;

    [SerializeField] private List<GameObject> NotToDestroyItems = null;

    [SerializeField] private GameObject treePackPrefab = null;
    [SerializeField] private GameObject grassPrefab = null;

    private void Start ()
    {
        singleton = this;

        if (nms != null)
        {
            nms = wm.GetComponent<NavMeshSurface> ();
        }

        testUI.SetActive (true);
    }

    public void SetPlayer (PlayerMotor pm)
    {
        playerMotor = pm;
        psm = pm.GetComponent<PlayerStatusManager> ();

        psm.canDie = canDie;
    }

    public void RestartFight ()
    {
        if (playerMotor != null)
        {
            playerMotor.DestroyPlayer ();
        }

        KillKnight ();
        DestroyEverything ();

        // Player
        GameObject playerGo = Instantiate (playerPrefab);
        playerMotor = playerGo.GetComponent<PlayerMotor> ();
        SetHighSpeed (highSpeed);

        playerMotor.transform.position = playerSpawnCoord;

        // Knight
        knight = Instantiate (knightPrefab, knightSpawnCoord, Quaternion.AngleAxis (180, Vector3.up));

        kc = knight.GetComponent<KnightController> ();
        kc.passive = knightPassive;

        // Scenery
        
        if (treePackPrefab != null)
        {
            GameObject treePack = Instantiate (treePackPrefab);
            treePack.transform.position = knightSpawnCoord + 15 * Vector3.left;
            treePack = Instantiate (treePackPrefab);
            treePack.transform.position = knightSpawnCoord + 15 * Vector3.right;
            treePack = Instantiate (treePackPrefab);
            treePack.transform.position = knightSpawnCoord + 15 * Vector3.forward;
            treePack = Instantiate (treePackPrefab);
            treePack.transform.position = knightSpawnCoord + 15 * Vector3.back;
        }
        
        if (grassPrefab != null)
        {
            GameObject grass = Instantiate (grassPrefab);
            grass.transform.position = knightSpawnCoord;
        }
    }

    private void DestroyEverything ()
    {
        GameObject[] allObjects = SceneManager.GetActiveScene ().GetRootGameObjects ();
        foreach (GameObject go in allObjects)
        {
            if (!NotToDestroyItems.Contains (go) && go.layer != 9)
            {
                Destroy (go);
            }
        }
    }

    public void KillKnight ()
    {
        if (knight != null)
        {
            Destroy (knight);
        }
    }

    public void SetKnightPassive (bool isPassive)
    {
        knightPassive = isPassive;
        if (kc != null)
        {
            kc.passive = knightPassive;
        }
    }

    public void SetCanDie (bool canDie)
    {
        this.canDie = canDie;
        if (psm != null)
        {
            psm.canDie = canDie;
        }
    }

    public void SetNoCD (bool noCD)
    {
        if (GameManager.gm != null)
        {
            GameManager.gm.ignoreCD = noCD;
        }
    }

    public void SetHighSpeed (bool hs)
    {
        highSpeed = hs;
        if (playerMotor != null)
        {
            if (hs)
            {
                playerMotor.moveSpeed = 25f;
            }
            else
            {
                playerMotor.moveSpeed = 8f;
            }
        }
    }

    public void QuitGame ()
    {
        Application.Quit ();
    }
}
