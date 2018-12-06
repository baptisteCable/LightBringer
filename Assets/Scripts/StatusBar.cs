using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {

    public GameObject characterGo;
    public Camera cam;

    private Character character;
    private Image hpContent;
    private Image mpContent;
    private Image channelingContent;
    private GameObject channelingBar;

    void Start () {
        character = characterGo.GetComponent<Character>();
        hpContent = transform.Find("HPBackGroung").Find("Content").GetComponent<Image>();
        mpContent = transform.Find("MPBackGroung").Find("Content").GetComponent<Image>();
        channelingContent = transform.Find("ChannelingBackGroung").Find("Content").GetComponent<Image>();
        channelingBar = transform.Find("ChannelingBackGroung").gameObject;
        channelingBar.SetActive(false);
    }
	
	void Update () {
        transform.position = cam.WorldToScreenPoint(characterGo.transform.position + new Vector3(0, 1f, 0)) + new Vector3(0,60,0);
        hpContent.fillAmount = character.currentHP / character.maxHP;
        mpContent.fillAmount = character.currentMP / character.maxMP;
        if (character.currentChanneling != null)
        {
            channelingBar.SetActive(true);
            channelingContent.fillAmount = character.currentChanneling.channelingTime / character.currentChanneling.channelingDuration;

        }
        else
        {
            channelingBar.SetActive(false);
        }
    }
}
