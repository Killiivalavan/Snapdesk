namespace SnapDesk.Shared;

/// <summary>
/// Represents dimensions with width and height
/// </summary>
public struct Size
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the aspect ratio of the size
    /// </summary>
    /// <returns>Aspect ratio (width/height)</returns>
    public double AspectRatio => (double)Width / Height;

    /// <summary>
    /// Checks if this size is square
    /// </summary>
    /// <returns>True if width equals height</returns>
    public bool IsSquare => Width == Height;

    /// <summary>
    /// Checks if this size is landscape (wider than tall)
    /// </summary>
    /// <returns>True if landscape orientation</returns>
    public bool IsLandscape => Width > Height;

    /// <summary>
    /// Checks if this size is portrait (taller than wide)
    /// </summary>
    /// <returns>True if portrait orientation</returns>
    public bool IsPortrait => Height > Width;

    /// <summary>
    /// Scales the size by a factor
    /// </summary>
    /// <param name="factor">Scaling factor</param>
    /// <returns>Scaled size</returns>
    public Size Scale(double factor)
    {
        return new Size((int)(Width * factor), (int)(Height * factor));
    }

    public override string ToString()
    {
        return $"{Width} x {Height}";
    }
}
