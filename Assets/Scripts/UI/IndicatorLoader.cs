using System.Collections;
using UnityEngine;

public class IndicatorLoader : MonoBehaviour
{
    [HideInInspector]
    public float duration;
    private float beginning;
    private float filling;

    public SpriteMask mask;

    public void Load (float dur)
    {
        duration = dur - .05f;
        StartCoroutine (RunLoading ());
    }

    private IEnumerator RunLoading ()
    {
        beginning = Time.time;
        filling = 0f;

        while (filling < 1f)
        {
            filling = (Time.time - beginning) / duration;
            mask.alphaCutoff = 1 - filling;

            yield return null;
        }
    }
}
