using System;

[Serializable]
public class ConditionnedTexture
{
    public float minHeight = -10f;
    public float maxHeight = -10f;
    public int groundTexIndex;

    public bool Fits(float height)
    {
        return (height >= minHeight && height <= maxHeight);
    }
}
