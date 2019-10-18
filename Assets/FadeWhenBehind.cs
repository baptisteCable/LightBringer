using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FadeWhenBehind : MonoBehaviour
{
    private const float MIN_ALPHA = .15f;
    private const float MAX_ALPHA = 1f;
    private const float FADING_TIME = 2f;

    private float currentAlpha = MAX_ALPHA;

    Collider col;
    [SerializeField] MeshRenderer[] objectsToFade = null;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ViewLine")
        {
            FadeOut();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ViewLine")
        {
            FadeIn();
        }
    }

    void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadingOut());
    }

    void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadingIn());
    }

    IEnumerator FadingOut()
    {
        while (currentAlpha > MIN_ALPHA)
        {
            currentAlpha -= Time.deltaTime * FADING_TIME;
            if (currentAlpha < MIN_ALPHA)
            {
                currentAlpha = MIN_ALPHA;
            }

            SetAlpha();

            yield return new WaitForEndOfFrame();
        }


    }

    IEnumerator FadingIn()
    {
        while (currentAlpha < MAX_ALPHA)
        {
            currentAlpha += Time.deltaTime * FADING_TIME;
            if (currentAlpha > MAX_ALPHA)
            {
                currentAlpha = MAX_ALPHA;
            }

            SetAlpha();

            yield return new WaitForEndOfFrame();
        }
    }

    void SetAlpha()
    {
        foreach (MeshRenderer rend in objectsToFade)
        {
            Color col = rend.material.GetColor("_Color");
            col.a = currentAlpha;
            rend.material.SetColor("_Color", col);
        }
    }
}
