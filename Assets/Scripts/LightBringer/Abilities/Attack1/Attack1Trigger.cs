using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer
{
    public class Attack1Trigger : AbilityTrigger
    {
        float triggerTop;

        private List<Collider> enemyList;

        void Start()
        {
            triggerTop = 3f;
            CreateAngularAoEMesh(2f, 90f, triggerTop, ref mesh);
            enemyList = new List<Collider>();
        }


        private void OnTriggerStay(Collider col)
        {
            if (col.tag == "Enemy")
            {
                if (!enemyList.Contains(col))
                {
                    enemyList.Add(col);
                    Debug.Log(col.gameObject.name);
                    DamageController dc = col.GetComponent("DamageController") as DamageController;
                    dc.TakeDamage(10f);
                }
            }
        }
    }
}