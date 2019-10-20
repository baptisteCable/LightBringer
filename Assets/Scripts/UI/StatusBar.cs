using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.Player
{
    public class StatusBar : BaseStatusBar
    {

        public PlayerStatusManager psm;
        public PlayerMotor character;

        private GameObject channelingBar;

        void Start ()
        {
            Transform hpBG = transform.Find ("BackGround").Find ("HPBackGroung");
            hpImage = hpBG.Find ("Content").GetComponent<Image> ();
            deleteHPdImage = hpBG.Find ("Deleted").GetComponent<UnityEngine.UI.Image> ();

            deleteHPdImage.fillAmount = psm.currentHP / psm.maxHP;
            lastHP = psm.currentHP;
            timeSinceDmg = 10f;
        }

        void Update ()
        {
            LookAtCamera (Camera.main);

            ComputeHPBar (psm.currentHP, psm.maxHP);
        }
    }
}
