using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using AntiDrag.Config;

namespace AntiDrag.Core;

public class FeedbackManager
{
    private readonly Settings _settings;
    private NotifyIcon? _notifyIcon;
    private DateTime _lastFeedback = DateTime.MinValue;
    private static readonly TimeSpan FeedbackCooldown = TimeSpan.FromMilliseconds(500);

    public FeedbackManager(Settings settings)
    {
        _settings = settings;
    }

    public void SetNotifyIcon(NotifyIcon icon)
    {
        _notifyIcon = icon;
    }

    public void OnDragBlocked()
    {
        // Throttle feedback to avoid spam
        var now = DateTime.UtcNow;
        if (now - _lastFeedback < FeedbackCooldown)
            return;
        _lastFeedback = now;

        if (_settings.TrayIconFeedback)
            FlashTrayIcon();

        if (_settings.NotificationFeedback)
            ShowNotification();

        if (_settings.SoundFeedback)
            PlaySound();
    }

    private void FlashTrayIcon()
    {
        if (_notifyIcon == null) return;

        var originalIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = App.TrayIcon.CreatePlaceholderIcon(Color.Red);

        // Restore after a brief flash
        var timer = new Timer { Interval = 300 };
        timer.Tick += (_, _) =>
        {
            _notifyIcon.Icon = originalIcon;
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private void ShowNotification()
    {
        _notifyIcon?.ShowBalloonTip(1000, "Anti-Drag", "Drag blocked", ToolTipIcon.Info);
    }

    private void PlaySound()
    {
        SystemSounds.Asterisk.Play();
    }
}
