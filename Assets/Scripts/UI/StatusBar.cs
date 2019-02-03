﻿using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.Player
{
    public class StatusBar : BaseStatusBar
    {

        public PlayerStatusManager psm;
        public PlayerMotor character;
        
        private Image channelingContent;
        private GameObject channelingBar;

        void Start()
        {
            Transform hpBG = transform.Find("BackGround").Find("HPBackGroung");
            hpImage = hpBG.Find("Content").GetComponent<Image>();
            deleteHPdImage = hpBG.Find("Deleted").GetComponent<UnityEngine.UI.Image>();

            Transform channelingBG = transform.Find("BackGround").Find("ChannelingBackGroung");
            channelingContent = channelingBG.Find("Content").GetComponent<Image>();
            channelingBar = channelingBG.gameObject;
            channelingBar.SetActive(false);

            deleteHPdImage.fillAmount = psm.currentHP / psm.maxHP;
            lastHP = psm.currentHP;
            timeSinceDmg = 10f;
        }

        void Update()
        {
            LookAtCamera(Camera.main);

            ComputeHPBar(psm.currentHP, psm.maxHP);
            
            if (character.currentChanneling != null)
            {
                channelingBar.SetActive(true);
                channelingContent.color = Color.yellow;
                channelingContent.fillAmount = (Time.time - character.currentChanneling.channelStartTime) / character.currentChanneling.channelDuration;
            }
            else if (character.currentAbility != null)
            {
                channelingBar.SetActive(true);
                channelingContent.color = Color.green;
                channelingContent.fillAmount = 1;
            }
            else
            {
                channelingBar.SetActive(false);
            }
        }
    }
}
