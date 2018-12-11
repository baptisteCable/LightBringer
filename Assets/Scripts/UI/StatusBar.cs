using UnityEngine;
using UnityEngine.UI;

public class StatusBar : BaseStatusBar {

    public Character character;
    public Camera cam;

    private Image mpContent;
    private Image channelingContent;
    private GameObject channelingBar;
    


    void Start () {
        Transform hpBG = transform.Find("BackGround").Find("HPBackGroung");
        hpImage = hpBG.Find("Content").GetComponent<Image>();
        deleteHPdImage = hpBG.Find("Deleted").GetComponent<UnityEngine.UI.Image>();

        Transform mpBG = transform.Find("BackGround").Find("MPBackGroung");
        mpContent = mpBG.Find("Content").GetComponent<Image>();

        Transform channelingBG = transform.Find("BackGround").Find("ChannelingBackGroung");
        channelingContent = channelingBG.Find("Content").GetComponent<Image>();
        channelingBar = channelingBG.gameObject;
        channelingBar.SetActive(false);
    }
	
	void Update () {
        LookAtCamera(cam);

        ComputeHPBar(character.currentHP, character.maxHP);

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
