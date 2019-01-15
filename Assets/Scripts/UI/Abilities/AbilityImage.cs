﻿using LightBringer.Player;
using UnityEngine;
using UnityEngine.UI;

public class AbilityImage : MonoBehaviour
{

    public int abilityIndex;
    public Character character;

    private Image cdImage;
    private Image channelingImage;
    private Image abilityImage;
    private GameObject lockedImage;


    void Start()
    {
        cdImage = transform.Find("CDImage").GetComponent<Image>();
        channelingImage = transform.Find("ChannelingImage").GetComponent<Image>();
        lockedImage = transform.Find("LockedImage").gameObject;
        abilityImage = transform.GetComponent<Image>();
    }

    void Update()
    {
        // CD running
        if (!character.abilities[abilityIndex].coolDownUp)
        {
            cdImage.gameObject.SetActive(true);
            cdImage.fillAmount = character.abilities[abilityIndex].coolDownRemaining / character.abilities[abilityIndex].coolDownDuration;
        }
        // CD Up
        else
        {
            cdImage.gameObject.SetActive(false);
        }
        // Channeling?
        if (character.currentChanneling == character.abilities[abilityIndex])
        {
            channelingImage.gameObject.SetActive(true);
            channelingImage.fillAmount = (Time.time - character.abilities[abilityIndex].channelStartTime) / character.abilities[abilityIndex].channelDuration;
        }
        else
        {
            channelingImage.gameObject.SetActive(false);
        }

        // Current ability
        if (character.currentAbility == character.abilities[abilityIndex])
        {
            abilityImage.color = Color.red;
        }
        else
        {
            abilityImage.color = Color.white;
        }

        // Locked
        lockedImage.SetActive(character.abilities[abilityIndex].locked);
    }
}


