using LightBringer;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CubeSkillShotTrigger : MonoBehaviour {

    public CubeSkillShot ability;

    private void OnTriggerEnter(Collider other)
    {
        ability.OnCollision(other);
    }
}
