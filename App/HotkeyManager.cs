using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AntiDrag.Config;

namespace AntiDrag.App;

public class HotkeyManager : NativeWindow, IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 1;

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_ALT = 0x0001;

    private readonly Settings _settings;
    private readonly Action _onToggle;
    private bool _registered;

    public HotkeyManager(Settings settings, Action onToggle)
    {
        _settings = settings;
        _onToggle = onToggle;
        CreateHandle(new CreateParams());
    }

    public void Register()
    {
        if (_registered) return;

        if (TryParseHotkey(_settings.GlobalHotkey, out uint modifiers, out uint vk))
        {
            _registered = RegisterHotKey(Handle, HOTKEY_ID, modifiers, vk);
        }
    }

    public void Unregister()
    {
        if (_registered)
        {
            UnregisterHotKey(Handle, HOTKEY_ID);
            _registered = false;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            _onToggle();
        }
        base.WndProc(ref m);
    }

    private static bool TryParseHotkey(string hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        if (string.IsNullOrWhiteSpace(hotkey))
            return false;

        var parts = hotkey.Split('+');
        foreach (var part in parts)
        {
            string trimmed = part.Trim();
            switch (trimmed.ToUpperInvariant())
            {
                case "CTRL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                default:
                    if (Enum.TryParse<Keys>(trimmed, true, out var key))
                        vk = (uint)key;
                    else
                        return false;
                    break;
            }
        }

        return vk != 0;
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
    }
}
