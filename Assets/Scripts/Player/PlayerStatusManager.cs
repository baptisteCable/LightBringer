using LightBringer.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(Character))]
    public class PlayerStatusManager : MonoBehaviour
    {
        private const float FLASH_DURATION = .1f;

        // HP MP
        public float maxHP;
        public float currentHP;
        public float maxMP;
        public float currentMP;

        // Gradable
        [HideInInspector]
        public float hasteMoveMultiplicator;

        // Crowd control
        [HideInInspector]
        public bool isRooted;
        [HideInInspector]
        public bool isStunned;
        [HideInInspector]
        public bool isInterrupted;
        [HideInInspector]
        public float rootDuration;
        [HideInInspector]
        public float stunDuration;
        [HideInInspector]
        public float interruptedDuration;

        // Status
        [HideInInspector]
        public List<State> states;
        private List<State> queuedStates;

        // Special status
        public Transform anchor;
        [HideInInspector]
        public bool isTargetable;
        [HideInInspector]
        public bool abilitySuppress; // Can't do anything because an ability prevents it

        // Components
        public StatusBar statusBar;
        [HideInInspector]
        public Character character;

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
            anchor = null;
            isTargetable = true;
            abilitySuppress = false;

            // States
            states = new List<State>();
            queuedStates = new List<State>();

            // Gradables
            hasteMoveMultiplicator = 1f;
        }

        private void Update()
        {
            AddAndStartQueuedStates();

            foreach (State s in states)
            {
                s.Update();
            }

            RemoveCompletedStates();
        }

        private void RemoveCompletedStates()
        {
            int i = 0;

            while (i < states.Count)
            {
                if (states[i].complete)
                {
                    states.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        // return false if the damage cannot target the player
        public bool IsAffectedBy(Damage dmg, Motor dealer, Vector3 origin = default(Vector3))
        {
            foreach (State s in states)
            {
                if (!s.IsAffectedBy(dmg, dealer, origin))
                {
                    return false;
                }
            }

            return true;
        }


        public void TakeDamage(Damage dmg, Motor dealer, Vector3 origin = default(Vector3))
        {
            foreach (State s in states)
            {
                dmg = s.AlterTakenDamage(dmg, dealer, origin);
            }

            if (dmg.amount > 0)
            {
                currentHP -= dmg.amount;

                StopCoroutine("Flash");
                StartCoroutine("Flash");
            }
        }

        // compute damage depending on states
        public Damage AlterDealtDamage(Damage dmg)
        {
            foreach (State s in states)
            {
                dmg = s.AlterDealtDamage(dmg);
            }

            return dmg;
        }

        public void AddAndStartState(State state)
        {
            queuedStates.Add(state);
        }

        private void AddAndStartQueuedStates()
        {
            while (queuedStates.Count > 0)
            {
                State s = queuedStates[0];
                states.Add(s);
                s.Start(this);
                queuedStates.RemoveAt(0);
            }
        }

        public void StopState(State state)
        {
            state.Stop();
        }

        public void RemoveState(State state)
        {
            states.Remove(state);
        }

        public void ApplyCrowdControl(CrowdControl cc, float duration)
        {
            bool affected = true;
            foreach (State s in states)
            {
                if(!s.isAffectedByCC(cc))
                {
                    affected = false;
                }
                Debug.Log(s + " : " + affected);
            }

            if (affected)
            {
                switch(cc.ccType)
                {
                    case CrowdControlType.Interrupt: Interrupt(duration); break;
                    case CrowdControlType.Root: Root(duration); break;
                    case CrowdControlType.Stun: Stun(duration); break;
                }
            }
        }

        private void Stun(float duration)
        {
            isStunned = true;
            if (stunDuration < duration)
            {
                stunDuration = duration;
            }
        }

        private void Root(float duration)
        {
            isRooted = true;
            if (rootDuration < duration)
            {
                rootDuration = duration;
            }
        }

        private void Interrupt(float duration)
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

        public void CancelStates()
        {
            foreach (State s in states)
            {
                if (s.cancellable)
                {
                    s.Cancel();
                }
            }
        }

        #region Flash
        private IEnumerator Flash()
        {
            RecFlashOn(transform);
            yield return new WaitForSeconds(FLASH_DURATION);
            RecFlashOff(transform);
        }

        private void RecFlashOn(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "Weapon" && tr.tag != "Spell" && tr.tag != "UI" && tr.tag != "MainCamera")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(.2f, .1f, .1f));
                }

                foreach (Transform child in tr)
                {
                    RecFlashOn(child);
                }
            }
        }

        private void RecFlashOff(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "Weapon" && tr.tag != "Spell" && tr.tag != "UI" && tr.tag != "MainCamera")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.DisableKeyword("_EMISSION");
                }

                foreach (Transform child in tr)
                {
                    RecFlashOff(child);
                }
            }
        }
        #endregion
        
    }
}