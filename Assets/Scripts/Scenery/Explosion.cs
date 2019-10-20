using UnityEngine;

public struct Explosion
{
    public Vector2 center;
    public float startTime;
    public float power;

    public Explosion (Vector2 center, float startTime, float power)
    {
        this.center = center;
        this.startTime = startTime;
        this.power = power;
    }
}
