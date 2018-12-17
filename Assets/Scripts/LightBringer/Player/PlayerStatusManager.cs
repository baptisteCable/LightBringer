using System.Collections;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(Character))]
    public class PlayerStatusManager : MonoBehaviour {
        // HP MP
        public float maxHP;
        public float currentHP;
        public float maxMP;
        public float currentMP;

        // Crowd control
        public bool isRooted;
        public bool isStunned;
        public bool isInterrupted;
        public float rootDuration;
        public float stunDuration;
        public float interruptedDuration;

        // Components
        public StatusBar statusBar;
        private Character character;

        void Start()
        {
            character = GetComponent<Character>();

            statusBar.psm = this;

            // Crowd control
            isRooted = false;
            isStunned = false;
            isInterrupted = false;
            rootDuration = 0f;
            stunDuration = 0f;
            interruptedDuration = 0f;
        }

        public void TakeDamage(float amount)
        {
            currentHP -= amount;

            StopCoroutine("Flash");
            StartCoroutine("Flash");
        }

        public void Stun(float duration)
        {
            isStunned = true;
            if (stunDuration < duration)
            {
                stunDuration = duration;
            }
        }

        public void Root(float duration)
        {
            isRooted = true;
            if (rootDuration < duration)
            {
                rootDuration = duration;
            }
        }

        public void Interrupt(float duration)
        {
            isInterrupted = true;
            if (interruptedDuration < duration)
            {
                interruptedDuration = duration;
            }
            character.animator.SetBool("isInterrupted", true);
            character.animator.Play("Interrupt");
        }

        public void CCComputation()
        {
            if (isInterrupted)
            {
                interruptedDuration -= Time.deltaTime;
                if (interruptedDuration <= 0f)
                {
                    isInterrupted = false;
                    character.animator.SetBool("isInterrupted", false);
                }
            }

            if (isRooted)
            {
                rootDuration -= Time.deltaTime;
                if (rootDuration <= 0f)
                {
                    isRooted = false;
                }
            }

            if (isStunned)
            {
                stunDuration -= Time.deltaTime;
                if (stunDuration <= 0f)
                {
                    isStunned = false;
                }
            }
        }

        #region Flash
        private IEnumerator Flash()
        {
            RecFlashOn(transform);
            yield return new WaitForSeconds(.25f);
            RecFlashOff(transform);
        }

        private void RecFlashOn(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "Weapon")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(1f, 153f / 255, 153f / 255));
                }
            }

            foreach (Transform child in tr)
            {
                RecFlashOn(child);
            }
        }

        private void RecFlashOff(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "Weapon")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.DisableKeyword("_EMISSION");
                }
            }

            foreach (Transform child in tr)
            {
                RecFlashOff(child);
            }
        }
        #endregion
    }
}