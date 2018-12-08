using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightBringer;

public class WeaponCollider : MonoBehaviour {

    private Collider col;
    private MeleeAttack1 currentAbility;

	void Start () {
        col = GetComponent<Collider>();
        col.enabled = false;
        currentAbility = null;
	}

    public void SetAbility(MeleeAttack1 ability)
    {
        col.enabled = true;
        currentAbility = ability;
        Debug.Log("Ability : " + currentAbility.ToString());
    }
	
    public void UnsetAbility()
    {
        col.enabled = false;
        currentAbility = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        currentAbility.OnCollision(other);
        Debug.Log("Ennemi détecté : " + other.name);
    }

}
