using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class BurningGround : MonoBehaviour
    {
        private const float FADING_TIME = .4f;

        [SerializeField] GameObject maskGOPrefab;
        [SerializeField] SpriteRenderer spriteRenderer;

        private float fadingStarting;
        private float initialAlpha;
        private int maskLayer = 0;

        void Start()
        {
            fadingStarting = Time.time + Attack1Behaviour.GROUND_DURATION - FADING_TIME;
            initialAlpha = spriteRenderer.color.a;

            if (maskLayer == 0)
            {
                InitLayerID();
            }

            // Set the sprite mask
            spriteRenderer.sortingLayerID = SortingLayer.NameToID("Enemy" + maskLayer);
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > fadingStarting)
            {
                float alpha = Mathf.Max(initialAlpha * (1 - (Time.time - fadingStarting) / FADING_TIME), 0);
                Color col = spriteRenderer.color;
                col.a = alpha;
                spriteRenderer.color = col;
            }
        }

        private void InitLayerID()
        {
            maskLayer = GameManager.gm.GetNextEnemyMaskIndex();
        }

        public void addAngle3d(float angle, float length)
        {
            GameObject newMask = Instantiate(maskGOPrefab, transform);
            newMask.transform.localPosition = Vector3.zero;
            newMask.transform.localRotation = Quaternion.Euler(0, angle, 0);
            newMask.transform.localScale = Vector3.one * length;
            SpriteMask mask = newMask.transform.Find("Mask").GetComponent<SpriteMask>();
            mask.isCustomRangeActive = true;

            if (maskLayer == 0)
            {
                InitLayerID();
            }

            mask.frontSortingLayerID = SortingLayer.NameToID("Enemy" + (maskLayer + 1));
            mask.backSortingLayerID = SortingLayer.NameToID("Enemy" + (maskLayer - 1));

        }
    }
}
