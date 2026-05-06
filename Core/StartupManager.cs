using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AntiDrag.Core;

public class StartupManager
{
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "AntiDrag";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
        return key?.GetValue(AppName) != null;
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.SetValue(AppName, $"\"{Application.ExecutablePath}\" --startup");
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.DeleteValue(AppName, false);
    }
}
