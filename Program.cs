using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AntiDrag.App;
using AntiDrag.Config;
using AntiDrag.Core;

namespace AntiDrag;

static class Program
{
    private const string MutexName = "AntiDrag_SingleInstance_Mutex";

    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Anti-Drag is already running.", "Anti-Drag",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var settings = SettingsManager.Load();
        var startupManager = new StartupManager();
        var trayApp = new TrayIcon(settings, startupManager);

        bool launchedFromStartup = args.Contains("--startup", StringComparer.OrdinalIgnoreCase);
        bool showWelcome = !(launchedFromStartup && settings.SuppressWelcomeOnStartup);

        if (showWelcome)
        {
            using var welcome = new WelcomeForm(settings, startupManager);
            var result = welcome.ShowDialog();

            if (result == DialogResult.Yes)
                trayApp.ShowSettings();
        }

        Application.Run(trayApp);
    }
}
