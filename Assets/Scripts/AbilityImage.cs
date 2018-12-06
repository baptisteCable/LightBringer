using UnityEngine;
using UnityEngine.UI;

public class AbilityImage : MonoBehaviour
{

    public int abilityIndex;
    public Character character;

    private Image cdImage;
    private Image channelingImage;
    private Image abilityImage;


    // Use this for initialization
    void Start()
    {
        cdImage = transform.Find("CDImage").GetComponent<Image>();
        channelingImage = transform.Find("ChannelingImage").GetComponent<Image>();
        abilityImage = transform.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!character.abilities[abilityIndex].coolDownUp)
        {
            cdImage.gameObject.SetActive(true);
            cdImage.fillAmount = character.abilities[abilityIndex].coolDownRemaining / character.abilities[abilityIndex].coolDownDuration;
        }
        else
        {
            cdImage.gameObject.SetActive(false);
        }

        if (character.currentChanneling == character.abilities[abilityIndex])
        {
            channelingImage.gameObject.SetActive(true);
            channelingImage.fillAmount = character.abilities[abilityIndex].channelingTime / character.abilities[abilityIndex].channelingDuration;
        }
        else
        {
            channelingImage.gameObject.SetActive(false);
        }

        if (character.currentChanneling == character.abilities[abilityIndex] || character.currentAbility == character.abilities[abilityIndex])
        {
            abilityImage.color = Color.cyan;
        }
        else
        {
            abilityImage.color = Color.white;
        }
    }
}


