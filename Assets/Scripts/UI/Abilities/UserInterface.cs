using LightBringer.Player;
using LightBringer.Player.Class;
using UnityEngine;

namespace LightBringer.UI
{
    public class UserInterface : MonoBehaviour
    {
        public PlayerMotor character;
        public GameObject abilityBar;
        public AbilityImage[] abIms;

        private void Start()
        {
            if (character != null)
            {
                SetCharacterToImages();
            }
        }

        private void Update()
        {
            if (character == null && abilityBar.activeSelf)
            {
                abilityBar.SetActive(false);
            }
        }

        public void SetPlayerMotor(PlayerMotor character)
        {
            this.character = character;
            SetCharacterToImages();
        }

        private void SetCharacterToImages()
        {
            foreach (AbilityImage abim in abIms)
            {
                abim.character = character;
                abilityBar.SetActive(true);
            }
        }
    }
}

