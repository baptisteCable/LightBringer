using UnityEngine;
using UnityEngine.UI;

public class BaseStatusBar : MonoBehaviour {

    private const float c_timeBeforeDelete = 1f;

    protected Image hpImage;
    protected Image deleteHPdImage;

    protected bool deleting = false;
    protected float timeSinceDmg;
    protected float lastHP;

    protected void ComputeHPBar (float currentHP, float maxHP) {

        hpImage.fillAmount = currentHP / maxHP;

        if (lastHP > currentHP)
        {
            lastHP = currentHP;
            deleting = false;
            timeSinceDmg = 0;
        }

        if (!deleting && deleteHPdImage.fillAmount > hpImage.fillAmount)
        {
            timeSinceDmg += Time.deltaTime;
            if (timeSinceDmg > c_timeBeforeDelete)
            {
                deleting = true;
            }
        }

        if (deleteHPdImage.fillAmount > hpImage.fillAmount && deleting)
        {
            deleteHPdImage.fillAmount -= .5f * Time.deltaTime;
            if (deleteHPdImage.fillAmount <= hpImage.fillAmount)
            {
                deleteHPdImage.fillAmount = hpImage.fillAmount;
                deleting = false;
            }
        }
	}

    protected void LookAtCamera(Camera camera)
    {
        if (camera != null)
        {
            transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        }
    }
}
