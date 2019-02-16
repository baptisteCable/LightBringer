using LightBringer.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class UltMotor : DelayedNetworkBehaviour
    {
        private const float ROTATION_SPEED = 12f;
        private const float EXTRA_DAMAGE_TAKER_DURATION = 10f;

        public GameObject[] quarters;
        public GameObject[] bigQuarters;
        public int qCount = 4;

        public Transform anchor;

        private void Start()
        {
            if (isServer)
            {
                CallForAll(M_Begin);
                RpcSetAnchor(anchor.gameObject);
            }
        }

        private void Update()
        {
            if (anchor != null)
            {
                transform.position = anchor.position;
            }
            transform.Rotate(Vector3.up, Time.deltaTime * ROTATION_SPEED);
        }

        [ClientRpc]
        private void RpcSetAnchor(GameObject anchorGO)
        {
            anchor = anchorGO.transform;
            Debug.Log("Anchor: " + anchor.name);
        }

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_DestroyObject: DestroyObject(); return true;
                case M_Begin: Begin(); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_DestroyObject = 0;
        private void DestroyObject()
        {
            Destroy(gameObject, .7f);
            transform.Find("DamageTaker/AllBrokenEffect").GetComponent<ParticleSystem>().Play();
        }

        // Called by id
        public const int M_Begin = 1;
        private void Begin()
        {
            transform.Find("DamageTaker").gameObject.SetActive(true);
            transform.rotation = Quaternion.identity;
            Destroy(gameObject, EXTRA_DAMAGE_TAKER_DURATION);
            transform.localScale = Vector3.one * anchor.GetComponent<CharacterController>().radius;
        }

        protected override bool CallById(int methdodId, int i)
        {
            if (base.CallById(methdodId, i))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_DestroyQuarter: DestroyQuarter(i); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_DestroyQuarter = 300;
        private void DestroyQuarter(int quarterId)
        {
            quarters[quarterId].transform.Find("Flash").gameObject.SetActive(true);
            Destroy(quarters[quarterId], .12f);
            quarters[quarterId] = null;
            Destroy(bigQuarters[quarterId]);
            bigQuarters[quarterId] = null;
            qCount -= 1;
        }
    }
}


