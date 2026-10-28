using System.Windows.Forms;
using PLCTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace PLCTracker.Services;

public class NotificationService : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Threading.DispatcherTimer _timer;

    public event Action? OnTasksUpdated;
    public event Action? OnOpenRequested;
    public event Action? OnExitRequested;

    public NotificationService()
    {
        var menu = new ContextMenuStrip();
        var openItem = new ToolStripMenuItem("Open PLC Tracker");
        openItem.Click += (_, _) => OnOpenRequested?.Invoke();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => OnExitRequested?.Invoke();

        menu.Items.Add(openItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "PLC Intake Tracker",
            ContextMenuStrip = menu
        };

        _trayIcon.DoubleClick += (_, _) => OnOpenRequested?.Invoke();

        // Mag-check siya every 1 minute kung naay task nga hapit na (10 mins before)
        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timer.Tick += (_, _) => CheckDueTasks();
        _timer.Start();
    }

    private void CheckDueTasks()
    {
        try
        {
            using var db = new AppDbContext();

            // KINI ANG GIKAUSAB: Mag-alarm siya kung ang task kay due na WITHIN THE NEXT 10 MINUTES
            var warningTime = DateTime.Now.AddMinutes(10);

            var due = db.Tasks
                .Include(t => t.Client)
                .Where(t => !t.IsCompleted && !t.IsNotified && t.DueDate <= warningTime)
                .ToList();

            foreach (var task in due)
            {
                // I-play ang Windows Exclamation sound para madunggan
                System.Media.SystemSounds.Exclamation.Play();

                // Ipakita ang notification
                _trayIcon.ShowBalloonTip(
                    10000,
                    "⏰ Task Approaching!",
                    $"{task.TypeLabel}: {task.Client?.Name}\nDue na in 10 mins or less!\n\nNotes: {task.Notes}",
                    ToolTipIcon.Warning);

                task.IsNotified = true;
            }

            if (due.Count > 0)
            {
                db.SaveChanges();
                OnTasksUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    public void ShowBalloon(string title, string text, ToolTipIcon icon = ToolTipIcon.Info, int timeoutMs = 3000)
    {
        _trayIcon.ShowBalloonTip(timeoutMs, title, text, icon);
    }

    public void Dispose()
    {
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
    }
}