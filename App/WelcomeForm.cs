using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AntiDrag.Config;
using AntiDrag.Core;

namespace AntiDrag.App;

public class WelcomeForm : Form
{
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_SETREDRAW = 0x000B;
    private const int WM_VSCROLL = 0x0115;
    private const int SB_TOP = 6;

    private readonly Settings _settings;
    private readonly StartupManager _startupManager;
    private readonly CheckBox _chkSuppress;

    public WelcomeForm(Settings settings, StartupManager startupManager)
    {
        _settings = settings;
        _startupManager = startupManager;

        Text = "Anti-Drag";
        Size = new Size(580, 580);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var titleLabel = new Label
        {
            Text = "Anti-Drag",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Location = new Point(25, 15),
            AutoSize = true
        };
        Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "Stop accidentally dragging files and folders in Explorer.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.DimGray,
            Location = new Point(27, 52),
            AutoSize = true
        };
        Controls.Add(subtitleLabel);

        var descriptionBox = new RichTextBox
        {
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            Location = new Point(25, 90),
            Size = new Size(515, 350),
            Font = new Font("Segoe UI", 10f),
            TabStop = false,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        Controls.Add(descriptionBox);

        // Freeze drawing while we set text and apply formatting
        SendMessage(descriptionBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

        descriptionBox.Text =
            "What does this do?\n" +
            "Ever accidentally move a folder into another folder?\n" +
            "Anti-Drag " +
            "sits in the background and watches " +
            "for drag operations in Explorer. If you click and slip, " +
            "nothing moves. Holding left click/drag " +
            "for a set duration, holding a key, or dragging a set distance will initiate a drag.\n\n" +

            "Ways to control it:\n" +
            "\u2022  Drag Delay : wait a bit before the drag kicks in (default: 300ms)\n" +
            "\u2022  Modifier Key : only drag while holding Shift, Ctrl, or Alt\n" +
            "\u2022  Drag Distance : move the mouse further before it counts as a drag\n" +
            "\u2022  Full Lock : just turn off dragging entirely\n" +
            "Mix and match these however you want.\n\n" +

            "Anti-Drag lives in your tray\n" +
            "Once you close this window, Anti-Drag keeps running in your " +
            "system tray (bottom-right corner, near the clock). " +
            "Look for the green circle.\n" +
            "\u2022  Right-click it for Settings, startup options, or to quit\n" +
            "\u2022  Left-click it to pause or resume\n" +
            "\u2022  Ctrl+Alt+D toggles it from anywhere";

        BoldRange(descriptionBox, "What does this do?");
        BoldRange(descriptionBox, "Ways to control it:");
        BoldRange(descriptionBox, "Anti-Drag lives in your tray?");

        // Reset scroll and cursor to top, then unfreeze
        descriptionBox.SelectionStart = 0;
        descriptionBox.SelectionLength = 0;
        SendMessage(descriptionBox.Handle, WM_VSCROLL, (IntPtr)SB_TOP, IntPtr.Zero);
        SendMessage(descriptionBox.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        descriptionBox.Invalidate();

        _chkSuppress = new CheckBox
        {
            Text = "Hide this message on Windows startup",
            Location = new Point(25, 475),
            AutoSize = true,
            Checked = _settings.SuppressWelcomeOnStartup
        };
        Controls.Add(_chkSuppress);

        var chkStartup = new CheckBox
        {
            Text = "Run Anti-Drag on Windows start",
            Location = new Point(25, 452),
            AutoSize = true,
            Checked = _startupManager.IsEnabled()
        };
        chkStartup.CheckedChanged += (_, _) =>
        {
            if (chkStartup.Checked)
                _startupManager.Enable();
            else
                _startupManager.Disable();
        };
        Controls.Add(chkStartup);

        var btnSettings = new Button
        {
            Text = "Settings",
            Location = new Point(335, 495),
            Size = new Size(100, 32),
            DialogResult = DialogResult.Yes
        };
        Controls.Add(btnSettings);

        var btnClose = new Button
        {
            Text = "Got it!",
            Location = new Point(445, 495),
            Size = new Size(100, 32),
            DialogResult = DialogResult.OK
        };
        Controls.Add(btnClose);

        AcceptButton = btnClose;

        Shown += (_, _) =>
        {
            SendMessage(descriptionBox.Handle, WM_VSCROLL, (IntPtr)SB_TOP, IntPtr.Zero);
            descriptionBox.SelectionStart = 0;
            descriptionBox.SelectionLength = 0;
            btnClose.Focus();
        };

        FormClosing += (_, _) => SaveSuppressSetting();
    }

    private void SaveSuppressSetting()
    {
        _settings.SuppressWelcomeOnStartup = _chkSuppress.Checked;
        SettingsManager.Save(_settings);
    }

    private static void BoldRange(RichTextBox rtb, string text)
    {
        int start = rtb.Text.IndexOf(text);
        if (start < 0) return;
        rtb.Select(start, text.Length);
        rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
        rtb.DeselectAll();
    }
}
