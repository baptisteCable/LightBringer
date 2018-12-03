using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour {
    // status
    public float maxHP;
    public float currentHP;
    public GameObject statusBarPrefab;
    public float displayHeight;

    private GameObject statusBarGO;
    private EnemyStatusBar statusBar;

    void Start () {
        // init barre de vie
        statusBarGO = Instantiate(statusBarPrefab, GameObject.Find("Canvas").transform);
        EnemyStatusBar esb = (EnemyStatusBar)(statusBarGO.GetComponent("EnemyStatusBar"));
        esb.enemy = transform.gameObject;
        esb.displayHeight = displayHeight;
    }
	
	public void TakeDamage(float amount)
    {
        Debug.Log("L'ennemi a encaissé " + amount.ToString() + " points de dégâts.");
    }
}
