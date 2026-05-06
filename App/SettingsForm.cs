using System;
using System.Drawing;
using System.Windows.Forms;
using AntiDrag.Config;
using AntiDrag.Core;

namespace AntiDrag.App;

public class SettingsForm : Form
{
    private readonly Settings _settings;
    private readonly StartupManager _startupManager;

    // Drag prevention controls
    private CheckBox _chkDragDelay = null!;
    private NumericUpDown _numDragDelay = null!;
    private CheckBox _chkModifierKey = null!;
    private ComboBox _cboModifierKey = null!;
    private CheckBox _chkDragDistance = null!;
    private NumericUpDown _numDragDistance = null!;
    private CheckBox _chkLockAll = null!;

    // Feedback controls
    private CheckBox _chkTrayFeedback = null!;
    private CheckBox _chkNotification = null!;
    private CheckBox _chkSound = null!;

    // General controls
    private CheckBox _chkStartup = null!;
    private TextBox _txtHotkey = null!;

    public SettingsForm(Settings settings, StartupManager startupManager)
    {
        _settings = settings;
        _startupManager = startupManager;
        InitializeComponents();
        LoadSettings();
    }

    private void InitializeComponents()
    {
        Text = "Anti-Drag Settings";
        Size = new Size(420, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        int y = 15;
        int leftMargin = 20;

        // === Drag Prevention Section ===
        AddSectionLabel("Drag Prevention", leftMargin, ref y);

        _chkDragDelay = AddCheckBox("Enable drag delay", leftMargin, ref y);
        _numDragDelay = AddNumericUpDown(260, y - 23, 60, 50, 5000, 50);
        AddLabel("ms", 325, y - 23);

        _chkModifierKey = AddCheckBox("Require modifier key", leftMargin, ref y);
        _cboModifierKey = AddComboBox(260, y - 23, 80);
        _cboModifierKey.Items.AddRange(new object[] { "Shift", "Ctrl", "Alt" });

        _chkDragDistance = AddCheckBox("Minimum drag distance", leftMargin, ref y);
        _numDragDistance = AddNumericUpDown(260, y - 23, 60, 4, 200, 1);
        AddLabel("px", 325, y - 23);

        _chkLockAll = AddCheckBox("Lock all drag-and-drop", leftMargin, ref y);

        y += 10;

        // === Feedback Section ===
        AddSectionLabel("Feedback", leftMargin, ref y);

        _chkTrayFeedback = AddCheckBox("Flash tray icon when drag blocked", leftMargin, ref y);
        _chkNotification = AddCheckBox("Show notification on block", leftMargin, ref y);
        _chkSound = AddCheckBox("Play sound on block", leftMargin, ref y);

        y += 10;

        // === General Section ===
        AddSectionLabel("General", leftMargin, ref y);

        _chkStartup = AddCheckBox("Start with Windows", leftMargin, ref y);

        AddLabel("Global hotkey:", leftMargin, y);
        _txtHotkey = new TextBox
        {
            Location = new Point(260, y),
            Size = new Size(120, 23),
            ReadOnly = true,
            BackColor = SystemColors.Window
        };
        _txtHotkey.KeyDown += OnHotkeyKeyDown;
        _txtHotkey.GotFocus += (_, _) => _txtHotkey.BackColor = Color.LightYellow;
        _txtHotkey.LostFocus += (_, _) => _txtHotkey.BackColor = SystemColors.Window;
        Controls.Add(_txtHotkey);
        y += 35;

        // === Buttons ===
        y += 15;
        var btnSave = new Button
        {
            Text = "Save",
            Location = new Point(200, y),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        btnSave.Click += (_, _) => SaveSettings();
        Controls.Add(btnSave);

        var btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(290, y),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(btnCancel);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private void LoadSettings()
    {
        _chkDragDelay.Checked = _settings.DragDelayEnabled;
        _numDragDelay.Value = Math.Clamp(_settings.DragDelayMs, 50, 5000);
        _chkModifierKey.Checked = _settings.ModifierKeyEnabled;
        _cboModifierKey.SelectedIndex = (int)_settings.RequiredModifier;
        _chkDragDistance.Checked = _settings.DragDistanceEnabled;
        _numDragDistance.Value = Math.Clamp(_settings.DragDistancePx, 4, 200);
        _chkLockAll.Checked = _settings.LockAllDrags;

        _chkTrayFeedback.Checked = _settings.TrayIconFeedback;
        _chkNotification.Checked = _settings.NotificationFeedback;
        _chkSound.Checked = _settings.SoundFeedback;

        _chkStartup.Checked = _startupManager.IsEnabled();
        _txtHotkey.Text = _settings.GlobalHotkey;
    }

    private void SaveSettings()
    {
        _settings.DragDelayEnabled = _chkDragDelay.Checked;
        _settings.DragDelayMs = (int)_numDragDelay.Value;
        _settings.ModifierKeyEnabled = _chkModifierKey.Checked;
        _settings.RequiredModifier = (ModifierKey)_cboModifierKey.SelectedIndex;
        _settings.DragDistanceEnabled = _chkDragDistance.Checked;
        _settings.DragDistancePx = (int)_numDragDistance.Value;
        _settings.LockAllDrags = _chkLockAll.Checked;

        _settings.TrayIconFeedback = _chkTrayFeedback.Checked;
        _settings.NotificationFeedback = _chkNotification.Checked;
        _settings.SoundFeedback = _chkSound.Checked;

        _settings.GlobalHotkey = _txtHotkey.Text;

        if (_chkStartup.Checked)
            _startupManager.Enable();
        else
            _startupManager.Disable();
    }

    private void OnHotkeyKeyDown(object? sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;

        if (e.KeyCode == Keys.Escape)
        {
            _txtHotkey.Text = _settings.GlobalHotkey;
            return;
        }

        // Ignore bare modifier keys
        if (e.KeyCode is Keys.ShiftKey or Keys.ControlKey or Keys.Menu)
            return;

        var parts = new System.Collections.Generic.List<string>();
        if (e.Control) parts.Add("Ctrl");
        if (e.Shift) parts.Add("Shift");
        if (e.Alt) parts.Add("Alt");
        parts.Add(e.KeyCode.ToString());

        _txtHotkey.Text = string.Join("+", parts);
    }

    // Helper methods for building the form
    private void AddSectionLabel(string text, int x, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(label);
        y += 25;
    }

    private CheckBox AddCheckBox(string text, int x, ref int y)
    {
        var cb = new CheckBox
        {
            Text = text,
            Location = new Point(x + 10, y),
            AutoSize = true
        };
        Controls.Add(cb);
        y += 28;
        return cb;
    }

    private NumericUpDown AddNumericUpDown(int x, int y, int width, int min, int max, int increment)
    {
        var nud = new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(width, 23),
            Minimum = min,
            Maximum = max,
            Increment = increment
        };
        Controls.Add(nud);
        return nud;
    }

    private ComboBox AddComboBox(int x, int y, int width)
    {
        var cbo = new ComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cbo);
        return cbo;
    }

    private Label AddLabel(string text, int x, int y)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true
        };
        Controls.Add(label);
        return label;
    }
}
