namespace AntiDrag.Config;

public enum ModifierKey
{
    Shift,
    Ctrl,
    Alt
}

public class Settings
{
    // Drag prevention
    public bool DragDelayEnabled { get; set; } = true;
    public int DragDelayMs { get; set; } = 300;
    public bool ModifierKeyEnabled { get; set; } = false;
    public ModifierKey RequiredModifier { get; set; } = ModifierKey.Shift;
    public bool DragDistanceEnabled { get; set; } = true;
    public int DragDistancePx { get; set; } = 10;
    public bool LockAllDrags { get; set; } = false;

    // Feedback
    public bool TrayIconFeedback { get; set; } = true;
    public bool NotificationFeedback { get; set; } = false;
    public bool SoundFeedback { get; set; } = false;

    // General
    public bool StartWithWindows { get; set; } = false;
    public string GlobalHotkey { get; set; } = "Ctrl+Alt+D";
    public bool SuppressWelcomeOnStartup { get; set; } = false;
}
