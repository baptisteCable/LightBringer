using UnityEngine;
namespace LightBringer.Enemies
{
    public class EnemyStatusBar : BaseStatusBar
    {
        [SerializeField] private StatusManager statusManager = null;

        void Start ()
        {
            Transform hpBG = transform.Find ("BackGround").Find ("HPBackGroung");
            hpImage = hpBG.Find ("HPContent").GetComponent<UnityEngine.UI.Image> ();
            deleteHPdImage = hpBG.Find ("Deleted").GetComponent<UnityEngine.UI.Image> ();

            deleteHPdImage.fillAmount = statusManager.currentHP / statusManager.maxHP;
            lastHP = statusManager.currentHP;
            timeSinceDmg = 10f;
        }

        void Update ()
        {

            LookAtCamera (Camera.main);

            ComputeHPBar (statusManager.currentHP, statusManager.maxHP);
        }
    }
}

