using System;
using System.Drawing;
using System.Windows.Forms;
using AntiDrag.Config;
using AntiDrag.Core;

namespace AntiDrag.App;

public class TrayIcon : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Settings _settings;
    private readonly MouseHookManager _hookManager;
    private readonly DragInterceptor _dragInterceptor;
    private readonly FeedbackManager _feedbackManager;
    private readonly HotkeyManager _hotkeyManager;
    private readonly StartupManager _startupManager;
    private ToolStripMenuItem _statusItem = null!;
    private ToolStripMenuItem _startupItem = null!;
    private bool _isActive = true;

    public TrayIcon(Settings settings, StartupManager startupManager)
    {
        _settings = settings;
        _feedbackManager = new FeedbackManager(settings);
        _dragInterceptor = new DragInterceptor(settings, _feedbackManager);
        _hookManager = new MouseHookManager(_dragInterceptor);
        _hotkeyManager = new HotkeyManager(settings, OnHotkeyToggle);
        _startupManager = startupManager;

        _notifyIcon = new NotifyIcon
        {
            Icon = CreatePlaceholderIcon(Color.Green),
            Text = "Anti-Drag: Active",
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                ToggleActive();
        };

        _feedbackManager.SetNotifyIcon(_notifyIcon);
        _hookManager.Install();
        _hotkeyManager.Register();
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        _statusItem = new ToolStripMenuItem("Anti-Drag: Active");
        _statusItem.Click += (_, _) => ToggleActive();
        _statusItem.Font = new Font(_statusItem.Font, FontStyle.Bold);
        menu.Items.Add(_statusItem);

        menu.Items.Add(new ToolStripSeparator());

        var settingsItem = new ToolStripMenuItem("Settings...");
        settingsItem.Click += (_, _) => ShowSettings();
        menu.Items.Add(settingsItem);

        _startupItem = new ToolStripMenuItem("Start with Windows");
        _startupItem.Checked = _startupManager.IsEnabled();
        _startupItem.Click += (_, _) => ToggleStartup();
        menu.Items.Add(_startupItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitApp();
        menu.Items.Add(exitItem);

        return menu;
    }

    private void ToggleActive()
    {
        _isActive = !_isActive;

        if (_isActive)
        {
            _hookManager.Install();
            _statusItem.Text = "Anti-Drag: Active";
            _notifyIcon.Icon = CreatePlaceholderIcon(Color.Green);
            _notifyIcon.Text = "Anti-Drag: Active";
        }
        else
        {
            _hookManager.Uninstall();
            _statusItem.Text = "Anti-Drag: Paused";
            _notifyIcon.Icon = CreatePlaceholderIcon(Color.Gray);
            _notifyIcon.Text = "Anti-Drag: Paused";
        }
    }

    private void OnHotkeyToggle()
    {
        ToggleActive();
    }

    public void ShowSettings()
    {
        using var form = new SettingsForm(_settings, _startupManager);
        if (form.ShowDialog() == DialogResult.OK)
        {
            SettingsManager.Save(_settings);
            _hotkeyManager.Unregister();
            _hotkeyManager.Register();
        }
    }

    private void ToggleStartup()
    {
        if (_startupManager.IsEnabled())
            _startupManager.Disable();
        else
            _startupManager.Enable();

        _startupItem.Checked = _startupManager.IsEnabled();
    }

    private void ExitApp()
    {
        _hookManager.Uninstall();
        _hotkeyManager.Unregister();
        _notifyIcon.Visible = false;
        Application.Exit();
    }

    internal static Icon CreatePlaceholderIcon(Color color)
    {
        var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1, 1, 14, 14);
            using var pen = new Pen(Color.White, 1);
            g.DrawEllipse(pen, 1, 1, 14, 14);
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
