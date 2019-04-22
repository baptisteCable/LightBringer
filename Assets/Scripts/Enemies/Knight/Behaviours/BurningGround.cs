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
        private int maskLayer = 0;

        private List<SpriteRenderer> cones;

        void Start()
        {
            fadingStarting = Time.time + Attack1Behaviour.GROUND_DURATION - FADING_TIME;
            initialAlpha = burningGroundSprite.color.a;

            if (maskLayer == 0)
            {
                InitLayerID();
                Debug.Log("Init: " + maskLayer);
            }

            burningGroundSprite.sortingLayerID = SortingLayer.NameToID(LAYER_NAME + maskLayer);
            burningGroundMask.isCustomRangeActive = true;
            burningGroundMask.frontSortingLayerID = SortingLayer.NameToID(LAYER_NAME + (maskLayer + 1));
            burningGroundMask.backSortingLayerID = SortingLayer.NameToID(LAYER_NAME + (maskLayer - 1));
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

        private void InitLayerID()
        {
            maskLayer = GameManager.gm.GetNextEnemyMaskIndex();
        }

        public void addAngle3d(float angle, float length)
        {
            if (length < Attack1Behaviour.MAX_DISTANCE)
            {
                GameObject newMask = Instantiate(maskPrefab, transform);
                newMask.transform.localPosition = Vector3.zero;
                newMask.transform.localRotation = Quaternion.Euler(0, angle, 0);
                newMask.transform.localScale = Vector3.one * length;
                SpriteMask sm = newMask.transform.Find("Cone").GetComponent<SpriteMask>();

                if (maskLayer == 0)
                {
                    InitLayerID();
                    Debug.Log("Init: " + maskLayer);
                }

                sm.isCustomRangeActive = true;
                sm.frontSortingLayerID = SortingLayer.NameToID(LAYER_NAME + (maskLayer + 1));
                sm.backSortingLayerID = SortingLayer.NameToID(LAYER_NAME + (maskLayer - 1));
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
