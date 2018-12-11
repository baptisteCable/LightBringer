using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightBringer;

public class WeaponCollider : MonoBehaviour {

    private Collider col;
    private CollisionAbility currentAbility;

	void Start () {
        col = GetComponent<Collider>();
        col.enabled = false;
        currentAbility = null;
	}

    public void SetAbility(CollisionAbility ability)
    {
        col.enabled = true;
        currentAbility = ability;
    }
	
    public void UnsetAbility()
    {
        col.enabled = false;
        currentAbility = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        currentAbility.OnCollision(other);
    }

}
