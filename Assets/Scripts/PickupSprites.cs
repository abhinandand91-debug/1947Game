// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System;
using UnityEngine;

/// <summary>
/// Draws the pickup and shield sprites at runtime, so the game needs no extra art for them.
/// </summary>
public static class PickupSprites
{
    private const int Size = 64;

    private static Sprite _heart;
    private static Sprite _shield;
    private static Sprite _bomb;
    private static Sprite _ring;

    public static Sprite Heart
    {
        get { return _heart ?? (_heart = Create(HeartPixel)); }
    }

    public static Sprite Shield
    {
        get { return _shield ?? (_shield = Create(ShieldPixel)); }
    }

    public static Sprite Bomb
    {
        get { return _bomb ?? (_bomb = Create(BombPixel)); }
    }

    /// <summary>A translucent bubble used as the aura around a shielded player.</summary>
    public static Sprite Ring
    {
        get { return _ring ?? (_ring = Create(RingPixel)); }
    }

    private static Sprite Create(Func<float, float, Color> pixel)
    {
        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Coordinates run from -1 to 1 with y pointing up
                texture.SetPixel(x, y, pixel((x + 0.5f) / Size * 2f - 1f, (y + 0.5f) / Size * 2f - 1f));
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    private static Color HeartPixel(float x, float y)
    {
        float hx = x * 1.25f;
        float hy = y * 1.25f + 0.1f;
        float v = Mathf.Pow(hx * hx + hy * hy - 1f, 3f) - hx * hx * hy * hy * hy;
        if (v > 0f)
        {
            return Color.clear;
        }
        bool highlight = x < -0.2f && y > 0.2f && x > -0.6f && y < 0.6f;
        return highlight ? new Color(1f, 0.6f, 0.7f) : new Color(1f, 0.25f, 0.4f);
    }

    private static Color ShieldPixel(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        if (r > 0.92f)
        {
            return Color.clear;
        }
        if (r > 0.78f)
        {
            return Color.white;
        }
        return Color.Lerp(new Color(0.2f, 0.55f, 1f), new Color(0.5f, 0.85f, 1f), (y + 1f) * 0.5f);
    }

    private static Color BombPixel(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float fuse = Mathf.Sqrt((x - 0.45f) * (x - 0.45f) + (y - 0.55f) * (y - 0.55f));
        if (fuse < 0.2f)
        {
            return new Color(1f, 0.9f, 0.2f);
        }
        if (r > 0.85f)
        {
            return Color.clear;
        }
        if (r > 0.7f)
        {
            return new Color(1f, 0.5f, 0.1f);
        }
        return new Color(0.12f, 0.12f, 0.18f);
    }

    private static Color RingPixel(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        if (r > 0.97f)
        {
            return Color.clear;
        }
        if (r > 0.88f)
        {
            return new Color(0.6f, 0.9f, 1f, 0.9f);
        }
        return new Color(0.3f, 0.7f, 1f, 0.15f);
    }
}
