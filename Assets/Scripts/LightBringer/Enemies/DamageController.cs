using System.Collections;
using UnityEngine;
using LightBringer;

[RequireComponent(typeof(EnemyMotor))]
public class DamageController : MonoBehaviour {
    // status
    public float maxHP;
    public float currentHP;
    public GameObject statusBarGO;
    public float displayHeight;
    private EnemyMotor motor;

    void Start () {
        EnemyStatusBar esb = (EnemyStatusBar)(statusBarGO.GetComponent("EnemyStatusBar"));
        motor = GetComponent<EnemyMotor>();
        esb.damageController = this;
    }
	
	public void TakeDamage(float amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            motor.Die();
            Destroy(statusBarGO);
        }


        StopCoroutine("Flash");
        StartCoroutine("Flash");
    }

    private IEnumerator Flash()
    {
        RecFlashOn(transform);
        yield return new WaitForSeconds(.25f);
        RecFlashOff(transform);     
    }

    private void RecFlashOn(Transform tr)
    {
        if (tr.tag != "Shield" && tr.tag != "UI")
        {
            Renderer renderer = tr.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = tr.GetComponent<Renderer>().material;

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 153f / 255, 153f / 255));
            }
        }

        foreach (Transform child in tr)
        {
            RecFlashOn(child);
        }
    }

    private void RecFlashOff(Transform tr)
    {
        if (tr.tag != "Shield" && tr.tag != "UI")
        {
            Renderer renderer = tr.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = tr.GetComponent<Renderer>().material;

                mat.DisableKeyword("_EMISSION");
            }
        }

        foreach (Transform child in tr)
        {
            RecFlashOff(child);
        }
    }
}
