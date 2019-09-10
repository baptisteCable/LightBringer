using UnityEngine.EventSystems;
using LightBringer.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.UI
{
    public class AbilityImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject panelPrefab;

        private GameObject abilityDescriptionPanel;

        public int abilityIndex;
        public PlayerMotor character;

        private Image cdImage;
        private Image channelingImage;
        private Image abilityImage;
        private GameObject lockedImage;
        
        protected virtual void Start()
        {
            cdImage = transform.Find("CDImage").GetComponent<Image>();
            channelingImage = transform.Find("ChannelingImage").GetComponent<Image>();
            lockedImage = transform.Find("LockedImage").gameObject;
            abilityImage = transform.GetComponent<Image>();
        }

        protected virtual void Update()
        {
            // CD running
            if (character.abilities[abilityIndex].state == Player.Abilities.AbilityState.cooldownInProgress)
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
            if (character.abilities[abilityIndex].state == Player.Abilities.AbilityState.channeling)
            {
                channelingImage.gameObject.SetActive(true);
                channelingImage.fillAmount = (Time.time - character.abilities[abilityIndex].channelStartTime) / character.abilities[abilityIndex].channelDuration;
            }
            else
            {
                channelingImage.gameObject.SetActive(false);
            }

            // Current ability or not
            if (character.abilities[abilityIndex].state == Player.Abilities.AbilityState.casting)
            {
                abilityImage.color = Color.red;
            }
            else
            {
                abilityImage.color = Color.white;
            }

            // Locked
            lockedImage.SetActive(character.abilities[abilityIndex].locked || !character.abilities[abilityIndex].available);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            abilityDescriptionPanel = Instantiate(panelPrefab, gameObject.transform);
            abilityDescriptionPanel.GetComponent<AbilityDescriptionPanel>().SetTitle(
                character.abilities[abilityIndex].GetTitle());
            abilityDescriptionPanel.GetComponent<AbilityDescriptionPanel>().SetDescription(
                character.abilities[abilityIndex].GetDescription());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Destroy(abilityDescriptionPanel);
        }
    }
}

