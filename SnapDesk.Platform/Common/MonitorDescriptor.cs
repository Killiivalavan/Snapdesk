using System;
using SnapDesk.Shared;

namespace SnapDesk.Platform.Common;

/// <summary>
/// Platform-level monitor descriptor used by the platform layer to report
/// monitor enumeration details without depending on Core types.
/// </summary>
public class MonitorDescriptor
{
    public IntPtr Handle { get; set; }
    public int Index { get; set; }
    public bool IsPrimary { get; set; }
    public int BoundsX { get; set; }
    public int BoundsY { get; set; }
    public int BoundsWidth { get; set; }
    public int BoundsHeight { get; set; }
    public int WorkingX { get; set; }
    public int WorkingY { get; set; }
    public int WorkingWidth { get; set; }
    public int WorkingHeight { get; set; }
    public int Dpi { get; set; }
    public int RefreshRate { get; set; }
    public string Name { get; set; } = string.Empty;
}


