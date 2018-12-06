using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusBar : MonoBehaviour {

    private const float c_timeBeforeDelete = .5f;

    public GameObject enemy;
    public float displayHeight;
    private UnityEngine.UI.Image hpImage;
    private UnityEngine.UI.Image deletedImage;
    private DamageController damageController;

    private float timeBeforeDelete = -1f;
    private bool deleting = false;

    private void Awake()
    {
        
    }

    void Start () {
        hpImage = transform.Find("HPBackGroung").Find("HPContent").GetComponent<UnityEngine.UI.Image>();
        deletedImage = transform.Find("HPBackGroung").Find("Deleted").GetComponent<UnityEngine.UI.Image>();
        damageController = (DamageController)(enemy.GetComponent("DamageController"));

        deletedImage.fillAmount = damageController.currentHP / damageController.maxHP;
    }
	
	void Update () {
        transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + new Vector3(0, displayHeight - .6f, 0)) + new Vector3(0,60,0);
        hpImage.fillAmount = damageController.currentHP / damageController.maxHP;

        if (deletedImage.fillAmount > hpImage.fillAmount)
        {
            if (!deleting)
            {
                deleting = true;
                timeBeforeDelete = c_timeBeforeDelete;
            }

            timeBeforeDelete -= Time.deltaTime;

            if (timeBeforeDelete < 0f)
            {
                deletedImage.fillAmount -= .5f * Time.deltaTime;
                if (deletedImage.fillAmount <= hpImage.fillAmount)
                {
                    deletedImage.fillAmount = hpImage.fillAmount;
                    deleting = false;
                }
            }
        }
	}
}
