using LightBringer.Enemies.Knight;
using UnityEngine;

public class BurningGround : MonoBehaviour
{
    private const float FADING_TIME = .4f;

    [SerializeField] SpriteMask mask;
    [SerializeField] SpriteRenderer spriteRenderer;

    private float fadingStarting;
    private float initialAlpha;

    // Start is called before the first frame update
    void Start()
    {
        fadingStarting = Time.time + Attack1Behaviour.GROUND_DURATION - FADING_TIME;
        mask.transform.localRotation = Quaternion.Euler(90f, Attack1Behaviour.CONE_STARTING, 0);
        initialAlpha = spriteRenderer.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > fadingStarting)
        {
            float alpha = initialAlpha * (1 - (Time.time - fadingStarting) / FADING_TIME);
            Color col = spriteRenderer.color;
            col.a = alpha;
            spriteRenderer.color = col;
        }
    }

    public void SetAngle(float angle)
    {
        mask.alphaCutoff = 1 - (angle - Attack1Behaviour.CONE_STARTING) / 360f;
    }
}
