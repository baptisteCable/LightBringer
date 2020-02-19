using System.Collections;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public class FadeWhenBehind : MonoBehaviour
{
    private const float MIN_ALPHA = 0.1f;
    private const float MAX_ALPHA = 1f;
    private const float MIN_INTENSITY = .1f;
    private const float MAX_INTENSITY = 1f;
    private const float FADING_SPEED = 2f;

    private float t = 1;

    [SerializeField] MeshRenderer[] objectsToFade = null;

    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("ViewLine"))
        {
            FadeOut ();
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("ViewLine"))
        {
            FadeIn ();
        }
    }

    void FadeOut ()
    {
        StopAllCoroutines ();
        StartCoroutine (FadingOut ());
    }

    void FadeIn ()
    {
        StopAllCoroutines ();
        StartCoroutine (FadingIn ());
    }

    IEnumerator FadingOut ()
    {
        while (t > 0)
        {
            t -= Time.deltaTime * FADING_SPEED;
            if (t < 0)
            {
                t = 0;
            }

            SetAlpha ();

            yield return new WaitForEndOfFrame ();
        }


    }

    IEnumerator FadingIn ()
    {
        while (t < 1)
        {
            t += Time.deltaTime * FADING_SPEED;
            if (t > 1)
            {
                t = 1;
            }

            SetAlpha ();

            yield return new WaitForEndOfFrame ();
        }
    }

    void SetAlpha ()
    {
        foreach (MeshRenderer rend in objectsToFade)
        {
            // Alpha
            Color col = rend.material.GetColor ("_Color");
            col.a = t * MAX_ALPHA + (1 - t) * MIN_ALPHA;
            rend.material.SetColor ("_Color", col);

            // Emission
            rend.material.SetFloat ("_EmissionIntensity", t * MAX_INTENSITY + (1 - t) * MIN_INTENSITY);
        }
    }
}
