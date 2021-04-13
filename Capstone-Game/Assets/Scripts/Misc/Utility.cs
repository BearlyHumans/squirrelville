using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    /// <summary> Return true if this float has a greater unsigned value than the comparison float. </summary>
    public static bool FurtherFromZero(this float f, float comparison)
    {
        return (comparison > 0 && f > comparison) || (comparison < 0 && f < comparison);
    }

    /// <summary> Return true if this int has a greater unsigned value than the comparison int. </summary>
    public static bool FurtherFromZero(this int f, int comparison)
    {
        return (comparison > 0 && f > comparison) || (comparison < 0 && f < comparison);
    }

    /// <summary> Return true if this float has a smaller unsigned value than the comparison float. </summary>
    public static bool CloserToZero(this float f, float comparison)
    {
        return (comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison);
    }

    /// <summary> Return true if this int has a smaller unsigned value than the comparison int. </summary>
    public static bool CloserToZero(this int f, int comparison)
    {
        return (comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison);
    }

    public static bool Outside(this float value, float lowerBounds, float upperBounds)
    {
        return value < lowerBounds || value > upperBounds;
    }

    /// <summary> Return true if the integer is within the specified bounds [inclusive]. </summary>
    public static bool Inside(this int value, int lowerBounds, int upperBounds)
    {
        return value >= lowerBounds && value <= upperBounds;
    }

    /// <summary> Return true if the float is within the specified bounds [exclusive]. </summary>
    public static bool Inside(this float value, float lowerBounds, float upperBounds)
    {
        return value > lowerBounds && value < upperBounds;
    }

    public static bool Inside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        return (
            value.x.Inside(lowerBounds.x, upperBounds.x) &&
            value.y.Inside(lowerBounds.y, upperBounds.y) &&
            value.z.Inside(lowerBounds.z, upperBounds.z)
        );
    }

    public static bool Outside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        return (
            value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.y.Outside(lowerBounds.y, upperBounds.y) ||
            value.z.Outside(lowerBounds.z, upperBounds.z)
        );
    }

    public static bool Outside2D(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        return (
            value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.z.Outside(lowerBounds.z, upperBounds.z)
        );
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
