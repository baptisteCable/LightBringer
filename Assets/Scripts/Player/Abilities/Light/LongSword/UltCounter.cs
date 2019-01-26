using LightBringer.Player.Class;
using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.UI
{
    public class UltCounter : MonoBehaviour
    {

        public LightLongSwordCharacter character;
        private Text text;

        void Start()
        {
            text = GetComponent<Text>();
            character = (LightLongSwordCharacter)(transform.parent.GetComponent<AbilityImage>().character);
        }

        void Update()
        {
            if (character.GetUltiShpereCount() > 0)
            {
                text.enabled = true;
                text.text = character.GetUltiShpereCount().ToString();
            }
            else
            {
                text.enabled = false;
            }
        }
    }
}

