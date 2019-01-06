﻿using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public class MeleeAoE1Trigger : AbilityTrigger
    {
        private const float TRIGGER_TOP = 3f;

        public MeleeAoE1 caller;

        void Start()
        {
            CreateAngularAoEMesh(2f, 90f, TRIGGER_TOP, ref mesh);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.tag == "Enemy")
            {   
                caller.AddEnemyDamageController(col.GetComponent("DamageController") as DamageController);
            }
            Debug.Log(col.name);
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.tag == "Enemy")
            {
                caller.RemoveEnemyDamageController(col.GetComponent("DamageController") as DamageController);
            }
        }
    }
}