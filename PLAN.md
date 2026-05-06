# Anti-Drag: Accidental Drag Prevention for Windows

## Context
Accidental folder/file dragging in Windows Explorer is a common frustration — one wrong mouse movement and a folder ends up nested inside another. This program sits in the system tray and intercepts drag operations in Explorer, requiring intentional action (delay, modifier key, or minimum distance) before allowing a drag. Inspired by RuneLite's inventory drag prevention plugin.

## Tech Stack
- **Language:** C# / .NET 8 (Windows Desktop)
- **UI Framework:** WinForms (lightweight, perfect for tray app + small settings window)
- **Hooks:** Low-level mouse hook via `SetWindowsHookEx` (Win32 P/Invoke)
- **Settings storage:** JSON file in `%APPDATA%\AntiDrag\settings.json`
- **Installer/startup:** Windows Registry `Run` key for start-on-boot

## Architecture Overview

```
AntiDrag/
├── Program.cs                  # Entry point, single-instance check
├── App/
│   ├── TrayIcon.cs             # System tray icon, context menu
│   ├── SettingsForm.cs         # Settings UI (WinForms)
│   └── HotkeyManager.cs       # Global hotkey registration
├── Core/
│   ├── MouseHookManager.cs     # Low-level mouse hook (SetWindowsHookEx)
│   ├── DragInterceptor.cs      # Drag prevention logic (delay, modifier, distance)
│   ├── ExplorerDetector.cs     # Detects if foreground window is Explorer/Desktop
│   └── FeedbackManager.cs      # Toast notifications, tray icon color, sounds
├── Config/
│   ├── Settings.cs             # Settings model (POCO, serialized to JSON)
│   └── SettingsManager.cs      # Load/save/defaults for settings.json
├── Resources/
│   └── tray_icon.ico           # Placeholder tray icon (shield or lock symbol)
└── AntiDrag.csproj
```

## Core Mechanics

### 1. Low-Level Mouse Hook (`MouseHookManager.cs`)
- Uses `SetWindowsHookEx(WH_MOUSE_LL, ...)` to intercept all mouse events globally
- Captures `WM_LBUTTONDOWN`, `WM_MOUSEMOVE`, `WM_LBUTTONUP`
- Passes events to `DragInterceptor` for decision-making
- Only active when foreground window is Explorer (checked by `ExplorerDetector`)

### 2. Drag Interception (`DragInterceptor.cs`)
User-selectable prevention modes (can combine multiple):

| Mode | How it works | Default |
|------|-------------|---------|
| **Drag Delay** | Must hold mouse button for X ms before drag starts | 300ms |
| **Modifier Key** | Must hold Shift/Ctrl/Alt during drag | Off |
| **Drag Distance** | Increase minimum pixel distance before drag registers | 10px |
| **Full Lock** | Completely block all drag-and-drop | Off |

**Logic flow:**
1. On `WM_LBUTTONDOWN` → record start position + timestamp
2. On `WM_MOUSEMOVE` while button held → check all active conditions:
   - If delay mode: has enough time elapsed since button down?
   - If modifier mode: is the required modifier key currently pressed?
   - If distance mode: has mouse moved beyond the threshold from start position?
   - If lock mode: block unconditionally
3. If conditions NOT met → suppress the mouse movement (return non-zero from hook to block, or reset the drag state)
4. If conditions met → allow the drag to proceed normally

### 3. Explorer Detection (`ExplorerDetector.cs`)
- Uses `GetForegroundWindow()` + `GetWindowThreadProcessId()` to get the active process
- Checks if the process is `explorer.exe`
- Also detects Desktop (which is a special Explorer window via `GetShellWindow()`)
- Only intercept when Explorer/Desktop is focused — other apps behave normally

### 4. Feedback (`FeedbackManager.cs`)
- **Tray icon change:** Green shield = active, gray = paused, red flash = drag blocked
- **Toast notification:** Brief "Drag blocked" balloon tip (optional, can be noisy)
- **Sound:** Subtle click/beep on blocked drag (optional)
- User can enable/disable each feedback type independently

## Settings UI (`SettingsForm.cs`)
Simple single-page WinForms dialog:

```
┌─ Anti-Drag Settings ──────────────────────┐
│                                            │
│  ☑ Enable drag delay      [300] ms        │
│  ☐ Require modifier key   [Shift ▾]       │
│  ☑ Minimum drag distance  [10  ] px       │
│  ☐ Lock all drag-and-drop                 │
│                                            │
│  ── Feedback ──                            │
│  ☑ Change tray icon when drag blocked      │
│  ☐ Show notification on block              │
│  ☐ Play sound on block                     │
│                                            │
│  ── General ──                             │
│  ☑ Start with Windows                      │
│  Global hotkey: [Ctrl+Shift+D]             │
│                                            │
│           [Save]  [Cancel]                 │
└────────────────────────────────────────────┘
```

## Settings Model (`Settings.cs`)
```csharp
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
    public string GlobalHotkey { get; set; } = "Ctrl+Shift+D";
}
```

## System Tray (`TrayIcon.cs`)
- Right-click context menu:
  - **Status:** "Anti-Drag: Active" / "Paused" (toggle on click)
  - **Settings...** → opens SettingsForm
  - **Start with Windows** (checkbox toggle)
  - **Exit**
- Left-click: toggle active/paused
- Icon changes color based on state (active/paused/blocking)

## Start on Boot
- Toggle via Settings UI or tray menu
- Adds/removes registry key: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Value: path to the executable
- Shows in Task Manager's "Startup" tab automatically

## Global Hotkey (`HotkeyManager.cs`)
- Uses `RegisterHotKey` Win32 API
- Default: `Ctrl+Shift+D` to toggle pause/resume
- Customizable in settings

## Implementation Order
1. **Project scaffolding** — .csproj, Program.cs, single-instance mutex
2. **Settings model + manager** — Settings.cs, SettingsManager.cs, JSON load/save
3. **System tray** — TrayIcon.cs with context menu, basic icon
4. **Mouse hook** — MouseHookManager.cs, raw hook setup
5. **Explorer detection** — ExplorerDetector.cs
6. **Drag interception logic** — DragInterceptor.cs with all 4 modes
7. **Feedback** — FeedbackManager.cs (tray icon changes, notifications, sound)
8. **Settings UI** — SettingsForm.cs with all controls
9. **Hotkey toggle** — HotkeyManager.cs
10. **Start on boot** — Registry key management
11. **Placeholder tray icon** — Simple .ico file

## Key Technical Notes
- **Single instance:** Use a named `Mutex` to prevent multiple instances
- **Hook performance:** The mouse hook callback must return quickly — do minimal work, offload to background if needed
- **Thread safety:** Mouse hook runs on the thread that installed it; UI updates need `Invoke`
- **Graceful shutdown:** Unhook on exit (`UnhookWindowsHookEx`), unregister hotkey
- **The hook approach:** We intercept at the mouse level, not at Explorer's drag-drop level. When a drag should be blocked, we suppress the `WM_MOUSEMOVE` events that would trigger Explorer's drag detection (Explorer uses `DragDetect` internally, which checks for movement beyond `SM_CXDRAG`/`SM_CYDRAG` system metrics)

## Verification
- Run the app → tray icon appears
- Open Explorer → try dragging a folder quickly → should be blocked
- Hold mouse button for 300ms+ then drag → should work
- Press `Ctrl+Shift+D` → tray icon changes to "paused", drags work normally
- Open Settings → change delay to 500ms → save → verify new delay works
- Enable "Start with Windows" → verify registry key created → reboot → app starts
- Right-click tray → Exit → verify hook is cleaned up (drags work normally again)

## Future (out of scope for now)
- Linux support (via X11/Wayland input hooks)
- macOS support (via CGEventTap)
- Per-app custom rules
- Undo last drag operation
- Activity/block log
