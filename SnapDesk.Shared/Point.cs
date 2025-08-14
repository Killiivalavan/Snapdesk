using System;

namespace SnapDesk.Shared;

/// <summary>
/// Represents a 2D point with X and Y coordinates
/// </summary>
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Calculates the distance between two points
    /// </summary>
    /// <param name="other">Other point</param>
    /// <returns>Distance between points</returns>
    public double DistanceTo(Point other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Adds two points together
    /// </summary>
    /// <param name="a">First point</param>
    /// <param name="b">Second point</param>
    /// <returns>Sum of the points</returns>
    public static Point operator +(Point a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y);
    }

    /// <summary>
    /// Subtracts one point from another
    /// </summary>
    /// <param name="a">First point</param>
    /// <param name="b">Second point</param>
    /// <returns>Difference of the points</returns>
    public static Point operator -(Point a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}
