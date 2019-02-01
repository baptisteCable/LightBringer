using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LightBringer.Player.Class;

namespace LightBringer.UI.Light.LongSword
{
    public class UltiImage : AbilityImage
    {
        private Image loadingImage;
        private Text text;

        protected override void Start()
        {
            base.Start();

            text = transform.Find("Counter").GetComponent<Text>();
            loadingImage = transform.Find("LoadingImage").GetComponent<Image>();
        }

        protected override void Update()
        {
            base.Update();

            int sphereCount = ((LightLongSwordCharacter)character).GetUltiShpereCount();

            if (sphereCount > 0)
            {
                text.enabled = true;
                text.text = sphereCount.ToString();
            }
            else
            {
                text.enabled = false;
            }

            if (sphereCount == 0 || sphereCount >= LightLongSwordCharacter.MAX_SPHERE_COUNT)
            {
                loadingImage.gameObject.SetActive(false);
            }
            else
            {
                loadingImage.gameObject.SetActive(true);
                loadingImage.fillAmount = 1f * sphereCount / LightLongSwordCharacter.MAX_SPHERE_COUNT;
            }
        }
    }
}

