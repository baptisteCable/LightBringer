using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class BurningGround : MonoBehaviour
    {
        private const float FADING_TIME = .4f;
        private const string LAYER_NAME = "Enemy";

        [SerializeField] GameObject maskPrefab;
        [SerializeField] SpriteRenderer burningGroundSprite;
        [SerializeField] SpriteMask burningGroundMask;

        private float fadingStarting;
        private float initialAlpha;

        private List<SpriteRenderer> cones;

        void Start()
        {
            fadingStarting = Time.time + Attack1Behaviour.GROUND_DURATION - FADING_TIME;
            initialAlpha = burningGroundSprite.color.a;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > fadingStarting)
            {
                float alpha = Mathf.Max(initialAlpha * (1 - (Time.time - fadingStarting) / FADING_TIME), 0);
                Color col = burningGroundSprite.color;
                col.a = alpha;
                burningGroundSprite.color = col;
                
            }
        }

        public void addAngle3d(float angle, float length)
        {
            if (length < Attack1Behaviour.MAX_DISTANCE)
            {
                GameObject newMask = Instantiate(maskPrefab, burningGroundSprite.transform);
                newMask.transform.localPosition = new Vector3(.051f, -.142f, 0);
                newMask.transform.localRotation = Quaternion.Euler(0, 0, -angle);
                newMask.transform.localScale = Vector3.one * length;
            }
        }

        public void SetAngle(float angle)
        {
            burningGroundMask.alphaCutoff = .1f + (angle - Attack1Behaviour.CONE_STARTING) / 360f;
        }

        public void EndRotation()
        {
            burningGroundMask.gameObject.SetActive(false);
        }
    }
}
