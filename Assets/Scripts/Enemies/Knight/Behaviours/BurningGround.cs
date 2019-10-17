using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class BurningGround : MonoBehaviour
    {
        private const float FADING_TIME = .5f;

        [SerializeField] SpriteRenderer burningGroundSprite = null;

        private float fadingStarting;
        private Color initialColor;

        private float[] sectors = new float[Attack1Behaviour.NB_SECTORS];

        void Start()
        {
            fadingStarting = Time.time + Attack1Behaviour.GROUND_DURATION - FADING_TIME;
            initialColor = burningGroundSprite.material.GetColor("_Color");
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > fadingStarting)
            {
                float alpha = Mathf.Max(initialColor.a * (1 - (Time.time - fadingStarting) / FADING_TIME), 0);
                Color col = initialColor;
                col.a = alpha;
                burningGroundSprite.material.SetColor("_Color", col);
            }
        }

        public void setSector(int index, float length)
        {
            sectors[index] = length / (Attack1Behaviour.MAX_DISTANCE + Attack1Behaviour.DIST_FROM_CENTER_COLLIDER) * .95f;
            burningGroundSprite.material.SetFloatArray("_Sectors", sectors);
        }

        public void SetAngle(float angle)
        {
            burningGroundSprite.material.SetFloat("_Angle", angle);
        }
    }
}
