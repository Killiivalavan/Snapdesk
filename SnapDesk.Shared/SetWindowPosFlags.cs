namespace SnapDesk.Shared;

/// <summary>
/// Flags for SetWindowPos operations
/// </summary>
[Flags]
public enum SetWindowPosFlags : uint
{
    /// <summary>
    /// Retains the current position (ignores X and Y parameters)
    /// </summary>
    SWP_NOMOVE = 0x0001,

    /// <summary>
    /// Retains the current size (ignores cx and cy parameters)
    /// </summary>
    SWP_NOSIZE = 0x0002,

    /// <summary>
    /// Retains the current Z order (ignores hWndInsertAfter parameter)
    /// </summary>
    SWP_NOZORDER = 0x0004,

    /// <summary>
    /// Does not redraw changes
    /// </summary>
    SWP_NOREDRAW = 0x0008,

    /// <summary>
    /// Does not activate the window
    /// </summary>
    SWP_NOACTIVATE = 0x0010,

    /// <summary>
    /// Applies new frame styles
    /// </summary>
    SWP_FRAMECHANGED = 0x0020,

    /// <summary>
    /// Shows the window
    /// </summary>
    SWP_SHOWWINDOW = 0x0040,

    /// <summary>
    /// Hides the window
    /// </summary>
    SWP_HIDEWINDOW = 0x0080,

    /// <summary>
    /// Discards the entire contents of the client area
    /// </summary>
    SWP_NOCOPYBITS = 0x0100,

    /// <summary>
    /// Does not change the owner window's position in the Z order
    /// </summary>
    SWP_NOOWNERZORDER = 0x0200,

    /// <summary>
    /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message
    /// </summary>
    SWP_NOSENDCHANGING = 0x0400,

    /// <summary>
    /// Draws a frame around the window
    /// </summary>
    SWP_DRAWFRAME = SWP_FRAMECHANGED,

    /// <summary>
    /// Sends WM_NCCALCSIZE message
    /// </summary>
    SWP_NOREPOSITION = SWP_NOOWNERZORDER,

    /// <summary>
    /// Prevents generation of the WM_SYNCPAINT message
    /// </summary>
    SWP_DEFERERASE = 0x2000,

    /// <summary>
    /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window
    /// </summary>
    SWP_ASYNCWINDOWPOS = 0x4000
}
