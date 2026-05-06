# Anti-Drag

A lightweight Windows utility that prevents accidental drag-and-drop operations in Windows Explorer and on the Desktop.

## What It Does

Anti-Drag intercepts mouse events using a low-level Windows hook and suppresses drag operations until configurable conditions are met — helping you avoid accidentally moving files when you only meant to click.

It only activates when Windows Explorer or the Desktop is in the foreground, so it won't interfere with other applications.

## Features

- **Drag delay** — requires the mouse button to be held for a set duration (default: 300ms) before a drag is allowed
- **Minimum distance** — requires the cursor to move a minimum number of pixels (default: 10px) before a drag starts
- **Modifier key** — optionally require Shift, Ctrl, or Alt to be held to permit dragging
- **Lock mode** — completely block all drag-and-drop operations
- **Global hotkey** — toggle protection on/off from anywhere (default: `Ctrl+Alt+D`)
- **System tray** — runs silently in the background; left-click the tray icon to toggle, right-click for the menu
- **Feedback** — optional tray icon flash, desktop notification, or sound when a drag is blocked
- **Start with Windows** — optional autostart via the registry

## Requirements

- Windows 10 or later
- [.NET 7.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (or use the standalone build)

## Installation

### Option A — Standalone executable (no .NET required)

Download `AntiDrag.exe` from the `publish-standalone/` folder or the Releases page and run it directly.

### Option B — Framework-dependent build

Download `AntiDrag.exe` from the `publish/` folder. Requires the .NET 7.0 Desktop Runtime to be installed.

## Building from Source

```
dotnet build
```

To publish a self-contained single-file executable:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

1. Launch `AntiDrag.exe`. A welcome screen will appear on first run.
2. Anti-Drag starts active and appears in the system tray with a **green icon**.
3. Use the tray icon to:
   - **Left-click** — toggle protection on/off
   - **Right-click → Settings** — open the settings window
   - **Right-click → Start with Windows** — enable/disable autostart
   - **Right-click → Exit** — quit the app
4. Press `Ctrl+Alt+D` (configurable) to toggle from anywhere.

When paused, the tray icon turns **gray**.

## Settings

| Setting | Default | Description |
|---|---|---|
| Drag delay | 300 ms | Time the button must be held before a drag is permitted |
| Minimum drag distance | 10 px | Distance the cursor must move before a drag is permitted |
| Require modifier key | Off | Hold Shift, Ctrl, or Alt to allow a drag |
| Lock all drags | Off | Completely block all drag operations |
| Tray icon feedback | On | Flash the tray icon when a drag is blocked |
| Notification feedback | Off | Show a toast notification when a drag is blocked |
| Sound feedback | Off | Play a sound when a drag is blocked |
| Global hotkey | Ctrl+Alt+D | Toggle protection on/off |
| Start with Windows | Off | Launch Anti-Drag automatically on login |

Settings are saved automatically when you close the Settings window.

## How It Works

Anti-Drag installs a `WH_MOUSE_LL` low-level mouse hook via `SetWindowsHookEx`. When the left button is pressed inside Explorer or on the Desktop, mouse-move events are suppressed until the configured delay, distance, or modifier conditions are satisfied. Once conditions are met, the move event passes through normally and the drag proceeds as usual.

## License

MIT
