using UnityEngine;
namespace LightBringer.Enemies
{
    public class EnemyStatusBar : BaseStatusBar
    {

        public StatusManager damageController;

        private void Awake()
        {

        }

        void Start()
        {
            Transform hpBG = transform.Find("BackGround").Find("HPBackGroung");
            hpImage = hpBG.Find("HPContent").GetComponent<UnityEngine.UI.Image>();
            deleteHPdImage = hpBG.Find("Deleted").GetComponent<UnityEngine.UI.Image>();

            deleteHPdImage.fillAmount = damageController.currentHP / damageController.maxHP;
            lastHP = damageController.currentHP;
            timeSinceDmg = 10f;
        }

        void Update()
        {

            LookAtCamera(Camera.main);

            ComputeHPBar(damageController.currentHP, damageController.maxHP);
        }
    }
}

