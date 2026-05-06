using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AntiDrag.Config;

namespace AntiDrag.Core;

public class DragInterceptor
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;

    private enum DragState
    {
        Idle,
        Held,       // Button down, suppressing moves until conditions met
        Dragging,   // Conditions met, moves pass through normally
    }

    private readonly Settings _settings;
    private readonly FeedbackManager _feedback;
    private readonly Timer _pollTimer;

    private DragState _state = DragState.Idle;
    private int _startX;
    private int _startY;
    private long _downTimestamp;

    public DragInterceptor(Settings settings, FeedbackManager feedback)
    {
        _settings = settings;
        _feedback = feedback;

        _pollTimer = new Timer { Interval = 15 };
        _pollTimer.Tick += OnPollTick;
    }

    public bool OnMouseDown(int x, int y)
    {
        _state = DragState.Held;
        _startX = x;
        _startY = y;
        _downTimestamp = Stopwatch.GetTimestamp();
        _pollTimer.Start();
        return false; // Let real down through for instant feedback
    }

    public bool OnMouseMove(int x, int y)
    {
        if (_state == DragState.Held)
        {
            if (_settings.LockAllDrags)
            {
                _feedback.OnDragBlocked();
                return true; // Suppress all moves
            }

            double distance = Distance(x, y);
            if (AreConditionsMet(distance))
            {
                _pollTimer.Stop();
                _state = DragState.Dragging;
                return false; // Allow this move through — starts the drag
            }

            return true; // Suppress move — prevents drag initiation
        }

        if (_state == DragState.Dragging)
            return false;

        return false;
    }

    public bool OnMouseUp(int x, int y)
    {
        _state = DragState.Idle;
        _pollTimer.Stop();
        return false; // Always let up through
    }

    public void Reset()
    {
        _state = DragState.Idle;
        _pollTimer.Stop();
    }

    private void OnPollTick(object? sender, EventArgs e)
    {
        if (_state != DragState.Held)
        {
            _pollTimer.Stop();
            return;
        }

        // Timer keeps running in Held state.
        // Transition to Dragging happens in OnMouseMove once conditions are met.
    }

    private bool AreConditionsMet(double distance)
    {
        if (_settings.DragDelayEnabled)
        {
            double elapsedMs = (Stopwatch.GetTimestamp() - _downTimestamp)
                * 1000.0 / Stopwatch.Frequency;
            if (elapsedMs < _settings.DragDelayMs)
                return false;
        }

        if (_settings.ModifierKeyEnabled)
        {
            int vk = _settings.RequiredModifier switch
            {
                ModifierKey.Shift => VK_SHIFT,
                ModifierKey.Ctrl => VK_CONTROL,
                ModifierKey.Alt => VK_MENU,
                _ => VK_SHIFT
            };

            if ((GetAsyncKeyState(vk) & 0x8000) == 0)
                return false;
        }

        if (_settings.DragDistanceEnabled)
        {
            if (distance < _settings.DragDistancePx)
                return false;
        }

        return true;
    }

    private double Distance(int x, int y)
    {
        int dx = x - _startX;
        int dy = y - _startY;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
