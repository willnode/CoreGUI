using UnityEngine;

// Free Utility: Rect as Linq :)
public static class RectLinq
{

    public static Rect Move(this Rect r, Vector2 offset)
    {
        r.x += offset.x;
        r.y += offset.y;
        return r;
    }

    public static Rect Scale(this Rect r, float scale)
    {
        r.x *= scale;
        r.y *= scale;
        r.width *= scale;
        r.height *= scale;
        return r;
    }

    public static Rect Scale(this Rect r, Vector2 scale)
    {
        r.x *= scale.x;
        r.y *= scale.y;
        r.width *= scale.x;
        r.height *= scale.y;
        return r;
    }

    public static Rect Union(this Rect r, Rect other)
    {
        r.min = Vector2.Min(r.min, other.min);
        r.max = Vector2.Max(r.max, other.max);
        return r;
    }

    public static Rect Intersect(this Rect r, Rect other)
    {
        r.min = Vector2.Max(r.min, other.min);
        r.max = Vector2.Min(r.max, other.max);
        return r;
    }

    public static Rect Shrink(this Rect r, float amount)
    {
        r.x += amount;
        r.y += amount;
        r.width -= amount * 2;
        r.height -= amount * 2;
        return r;
    }

    public static Rect Shrink(this Rect r, Vector2 amount)
    {
        r.x += amount.x;
        r.y += amount.y;
        r.width -= amount.x * 2;
        r.height -= amount.y * 2;
        return r;
    }
    
    public static Rect Shrink(this Rect r, CoreGUI.Side side, float amount)
    {
        switch (side)
        {
            case CoreGUI.Side.Left:
                r.xMin += amount;
                break;
            case CoreGUI.Side.Right:
                r.xMax -= amount;
                break;
            case CoreGUI.Side.Top:
                r.yMin += amount;
                break;
            case CoreGUI.Side.Bottom:
                r.yMax -= amount;
                break;
        }
        return r;
    }

    public static Vector2 Interpolate(this Rect r, Vector2 ratio)
    {
        return new Vector2(
            r.x + r.width * ratio.x,
            r.y + r.height * ratio.y
            );
    }

    public static Rect PixelPerfect(this Rect r)
    {
        return new Rect(
           Mathf.Round(r.x),
           Mathf.Round(r.y),
           Mathf.Round(r.width),
           Mathf.Round(r.height)
           );
    }
}
