using LightBringer.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerStatusManager : MonoBehaviour
    {
        private const float FLASH_DURATION = .1f;

        // HP MP
        public float maxHP;
        [HideInInspector]
        public float currentHP;

        // Gradable
        [HideInInspector]
        public Dictionary<State, float> moveMultiplicators;
        [HideInInspector]
        public Dictionary<State, float> maxRotation;

        // Crowd control
        [HideInInspector]
        public bool isRooted;
        [HideInInspector]
        public bool isStunned;
        [HideInInspector]
        public float rootDuration;
        [HideInInspector]
        public float stunDuration;

        // Status
        [HideInInspector]
        public List<State> states;
        private List<State> queuedStates;
        public bool isDead;

        // Special status
        public Transform anchor;
        [HideInInspector]
        public bool isTargetable;
        [HideInInspector]
        public bool abilitySuppress; // Can't do anything because an ability prevents it

        // Components
        public StatusBar statusBar;
        [HideInInspector]
        public PlayerMotor character;

        // Training
        public bool canDie = true;

        void Start()
        {
            character = GetComponent<PlayerMotor>();

            statusBar.psm = this;

            // Init is called by playerMotor
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

            if (currentHP <= 0 && canDie)
            {
                Die();
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
            }

            if (affected)
            {
                switch(cc.ccType)
                {
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
                character.animator.SetBool("isStunned", true);
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

        public void CCComputation()
        {
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
                    character.animator.SetBool("isStunned", false);
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

        private void Die()
        {
            if (!isDead)
            {
                isDead = true;

                // TODO animation

                character.Die();

            }
        }

        public float GetStateMoveSpeedMultiplicator()
        {
            float mult = 1f;

            foreach (KeyValuePair<State, float> pair in moveMultiplicators)
            {
                mult *= pair.Value;
            }

            return mult;
        }

        public float GetMaxRotationSpeed()
        {
            float mrs = Mathf.Infinity;

            foreach (KeyValuePair<State, float> pair in maxRotation)
            {
                if (pair.Value < mrs)
                {
                    mrs = pair.Value;
                }
            }

            return mrs;
        }

        public void Init()
        {
            //HP
            currentHP = maxHP;

            // Crowd control
            isRooted = false;
            isStunned = false;
            rootDuration = 0f;
            stunDuration = 0f;
            anchor = null;
            isTargetable = true;
            abilitySuppress = false;

            // States
            states = new List<State>();
            queuedStates = new List<State>();
            isDead = false;

            // Gradables
            moveMultiplicators = new Dictionary<State, float>();
            maxRotation = new Dictionary<State, float>();
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