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
        currentHP -= amount;

        StopCoroutine("Flash");
        StartCoroutine("Flash");
    }

    private IEnumerator Flash()
    {     
        foreach (Transform child in transform)
        {
            if (child.tag != "Shield")
            {
                Renderer renderer = child.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = child.GetComponent<Renderer>().material;

                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(1f, 153f / 255, 153f / 255));
                }
            }
            
        }

        yield return new WaitForSeconds(.25f);

        foreach (Transform child in transform)
        {
            if (child.tag != "Shield")
            {
                Renderer renderer = child.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = child.GetComponent<Renderer>().material;

                    mat.DisableKeyword("_EMISSION");
                }
            }
        }
     
    }
}
