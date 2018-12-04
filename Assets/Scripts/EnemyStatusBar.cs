using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusBar : MonoBehaviour {

    public GameObject enemy;
    public float displayHeight;
    private UnityEngine.UI.Image hpImage;
    private DamageController damageController;

    private void Awake()
    {
        
    }

    void Start () {
        hpImage = transform.Find("HPBackGroung").Find("HPContent").GetComponent<UnityEngine.UI.Image>();
        damageController = (DamageController)(enemy.GetComponent("DamageController"));
    }
	
	void Update () {
        transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + new Vector3(0, displayHeight - .6f, 0)) + new Vector3(0,60,0);
        hpImage.fillAmount = damageController.currentHP / damageController.maxHP;
	}
}
