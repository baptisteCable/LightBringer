using UnityEngine;

public class EnemyStatusBar : MonoBehaviour {

    private const float c_timeBeforeDelete = 1f;

    public GameObject enemy;
    public float displayHeight;
    private UnityEngine.UI.Image hpImage;
    private UnityEngine.UI.Image deletedImage;
    private DamageController damageController;

    private bool deleting = false;
    private float timeSinceDmg;
    private float lastHP;

    private void Awake()
    {
        
    }

    void Start () {
        hpImage = transform.Find("HPBackGroung").Find("HPContent").GetComponent<UnityEngine.UI.Image>();
        deletedImage = transform.Find("HPBackGroung").Find("Deleted").GetComponent<UnityEngine.UI.Image>();
        damageController = (DamageController)(enemy.GetComponent("DamageController"));

        deletedImage.fillAmount = damageController.currentHP / damageController.maxHP;
        lastHP = damageController.currentHP;
        timeSinceDmg = 10f;
    }
	
	void Update () {
        transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + new Vector3(0, displayHeight - .6f, 0)) + new Vector3(0,60,0);
        hpImage.fillAmount = damageController.currentHP / damageController.maxHP;

        if (lastHP > damageController.currentHP)
        {
            lastHP = damageController.currentHP;
            deleting = false;
            timeSinceDmg = 0;
        }

        if (!deleting && deletedImage.fillAmount > hpImage.fillAmount)
        {
            timeSinceDmg += Time.deltaTime;
            if (timeSinceDmg > c_timeBeforeDelete)
            {
                deleting = true;
            }
        }

        if (deletedImage.fillAmount > hpImage.fillAmount && deleting)
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
