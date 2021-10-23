using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    /// <summary> Return true if this float has a greater unsigned value than the comparison float. </summary>
    public static bool FurtherFromZero(this float f, float comparison)
    {
        if ((comparison > 0 && f > comparison) || (comparison < 0 && f < comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this int has a greater unsigned value than the comparison int. </summary>
    public static bool FurtherFromZero(this int f, int comparison)
    {
        if ((comparison > 0 && f > comparison) || (comparison < 0 && f < comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this float has a smaller unsigned value than the comparison float. </summary>
    public static bool CloserToZero(this float f, float comparison)
    {
        if ((comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this int has a smaller unsigned value than the comparison int. </summary>
    public static bool CloserToZero(this int f, int comparison)
    {
        if ((comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison))
            return true;
        return false;
    }

    public static float Sign(this float signOf)
    {
        if (signOf > 0)
            return 1;
        else if (signOf == 0)
            return 0;
        return -1;
    }

    public static float TwoFigures(this float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    public static bool Outside(this float value, float lowerBounds, float upperBounds)
    {
        if (value < lowerBounds || value > upperBounds)
            return true;
        return false;
    }

    /// <summary> Return true if the integer is within the specified bounds [inclusive]. </summary>
    public static bool Inside(this int value, int lowerBounds, int upperBounds)
    {
        if (value >= lowerBounds && value <= upperBounds)
            return true;
        return false;
    }

    /// <summary> Return true if the float is within the specified bounds [exclusive]. </summary>
    public static bool Inside(this float value, float lowerBounds, float upperBounds)
    {
        if (value > lowerBounds && value < upperBounds)
            return true;
        return false;
    }

    public static bool Inside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Inside(lowerBounds.x, upperBounds.x) &&
            value.y.Inside(lowerBounds.y, upperBounds.y) &&
            value.z.Inside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static bool Outside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.y.Outside(lowerBounds.y, upperBounds.y) ||
            value.z.Outside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static bool Outside2D(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.z.Outside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static int lerp(this int value, int target, int t)
    {
        if (value < target)
        {
            value += Mathf.Abs(t);
            if (value >= target)
                return target;
            else
                return value;
        }
        else
        {
            value -= Mathf.Abs(t);
            if (value <= target)
                return target;
            else
                return value;
        }
    }

    public static Vector3 Invert(this Vector3 value)
    {
        return new Vector3(-value.x, -value.y, -value.z);
    }

    /// <summary> Replace the Y axis of this vector with the specified value. </summary>
    public static Vector3 FixedY(this Vector3 value, float newY)
    {
        return new Vector3(value.x, newY, value.z);
    }

    public static Vector3 MultipliedBy(this Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static Vector3Int MultipliedBy(this Vector3Int v1, Vector3 v2)
    {
        return new Vector3Int(Mathf.FloorToInt(v1.x * v2.x), Mathf.FloorToInt(v1.y * v2.y), Mathf.FloorToInt(v1.z * v2.z));
    }
}

public class Mean
{
    public float total = 0;
    public int count = 0;

    /// <summary> Add the value to the total and increase the count by 1 so the new average can be calculated. </summary>
    public void Add(float value)
    {
        total += value;
        count += 1;
    }

    /// <summary> Returns the current average (total value divided by the number of values added). </summary>
    public float Average
    {
        get { return total / count; }
    }

    /// <summary> Add a value to the total and also return the new average. </summary>
    public float NewAverage(float value)
    {
        total += value;
        count += 1;
        return total / count;
    }
}
