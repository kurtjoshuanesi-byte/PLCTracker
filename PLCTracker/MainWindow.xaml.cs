using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using PLCTracker.Data;
using PLCTracker.Models;
using PLCTracker.Services;
using PLCTracker.Views;

namespace PLCTracker;

public partial class MainWindow : Window
{
    private readonly NotificationService _notifier;
    private bool _reallyExit = false;
    private bool _hintShown = false;

    public MainWindow()
    {
        InitializeComponent();

        using (var db = new AppDbContext()) db.Database.EnsureCreated();

        _notifier = new NotificationService();
        _notifier.OnTasksUpdated += () => Dispatcher.Invoke(() => { Refresh(); RefreshClients(); });
        _notifier.OnOpenRequested += () => Dispatcher.Invoke(ShowFromTray);
        _notifier.OnExitRequested += () => Dispatcher.Invoke(() => { _reallyExit = true; Close(); });

        Refresh();
        RefreshClients();

        Loaded += (s, e) =>
        {
            using var db = new AppDbContext();
            var tomorrow = DateTime.Today.AddDays(1);
            var hasUrgentTasks = db.Tasks.Any(t => !t.IsCompleted && t.DueDate < tomorrow);

            if (hasUrgentTasks)
            {
                var agendaDlg = new DailyAgendaDialog { Owner = this };
                agendaDlg.ShowDialog();
            }
        };
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        Refresh();
        RefreshClients();
    }

    private void Refresh()
    {
        using var db = new AppDbContext();
        var query = db.Tasks.Include(t => t.Client).AsQueryable();

        if (ShowCompletedCheck.IsChecked != true)
            query = query.Where(t => !t.IsCompleted);

        if (!string.IsNullOrWhiteSpace(TaskSearchBox.Text))
        {
            var search = TaskSearchBox.Text.ToLower();
            query = query.Where(t => t.Client!.Name.ToLower().Contains(search) ||
                                     t.Notes.ToLower().Contains(search) ||
                                     t.Client!.Phone.Contains(search));
        }

        var tasks = query.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate).ToList();
        TaskGrid.ItemsSource = tasks;

        var allTasks = db.Tasks.ToList();
        var overdue = allTasks.Count(t => !t.IsCompleted && t.DueDate < DateTime.Now);
        var today = allTasks.Count(t => !t.IsCompleted && t.DueDate.Date == DateTime.Today && t.DueDate >= DateTime.Now);
        var completed = allTasks.Count(t => t.IsCompleted);
        StatusText.Text = $"🔴 Overdue: {overdue}  |  🟡 Today: {today}  |  ✅ Done: {completed}";
    }

    private void TaskSearchBox_TextChanged(object sender, TextChangedEventArgs e) => Refresh();
    private void ShowCompletedCheck_Changed(object sender, RoutedEventArgs e) => Refresh();

    private void CompleteTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem taskItem)
        {
            using var db = new AppDbContext();
            var task = db.Tasks.Find(taskItem.Id);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
                db.SaveChanges();
                Refresh();
            }
            e.Handled = true;
        }
    }

    private void EditTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem taskItem)
        {
            using var db = new AppDbContext();
            var task = db.Tasks.Find(taskItem.Id);
            if (task != null)
            {
                var dlg = new TaskDialog(task) { Owner = this };
                if (dlg.ShowDialog() == true) Refresh();
            }
            e.Handled = true;
        }
    }

    private void DeleteTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem taskItem)
        {
            if (MessageBox.Show("Delete this task?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using var db = new AppDbContext();
                var task = db.Tasks.Find(taskItem.Id);
                if (task != null) { db.Tasks.Remove(task); db.SaveChanges(); }
                Refresh();
            }
            e.Handled = true;
        }
    }

    private void CopyPhone_Click(object sender, RoutedEventArgs e)
    {
        if (TaskGrid.SelectedItem is TaskItem task && task.Client != null)
        {
            Clipboard.SetText(task.Client.Phone);
            MessageBox.Show($"Copied: {task.Client.Phone}", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else MessageBox.Show("Pilia una ang task.");
    }

    private void Snooze15_Click(object sender, RoutedEventArgs e) => SnoozeTask(15);
    private void Snooze1H_Click(object sender, RoutedEventArgs e) => SnoozeTask(60);

    private void SnoozeTask(int minutes)
    {
        if (TaskGrid.SelectedItem is not TaskItem task) return;
        using var db = new AppDbContext();
        var dbTask = db.Tasks.Find(task.Id);
        if (dbTask != null)
        {
            dbTask.DueDate = DateTime.Now.AddMinutes(minutes);
            dbTask.IsNotified = false;
            db.SaveChanges();
            Refresh();
            MessageBox.Show($"Task snoozed for {minutes} minutes.", "Snoozed");
        }
    }

    private void RefreshClients()
    {
        using var db = new AppDbContext();
        var query = db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ClientSearchBox.Text))
        {
            var search = ClientSearchBox.Text.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                     c.Phone.Contains(search) ||
                                     c.Email.ToLower().Contains(search));
        }

        var clients = query.OrderBy(c => c.Name).ToList();
        ClientGrid.ItemsSource = clients;
        ClientCountText.Text = $"Total Clients: {clients.Count}";
    }

    private void ClientSearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshClients();

    private void EditClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Client clientItem)
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(clientItem.Id);
            if (client != null)
            {
                var dlg = new ClientDialog(client) { Owner = this };
                if (dlg.ShowDialog() == true) { Refresh(); RefreshClients(); }
            }
            e.Handled = true;
        }
    }

    private void DeleteClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Client clientItem)
        {
            using var db = new AppDbContext();
            var client = db.Clients.Include(c => c.Tasks).FirstOrDefault(c => c.Id == clientItem.Id);
            if (client != null)
            {
                var msg = client.Tasks.Count > 0 ? $"Delete '{client.Name}' and {client.Tasks.Count} task(s)?" : $"Delete '{client.Name}'?";
                if (MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    db.Tasks.RemoveRange(client.Tasks);
                    db.Clients.Remove(client);
                    db.SaveChanges();
                    Refresh(); RefreshClients();
                }
            }
            e.Handled = true;
        }
    }

    private void ClientGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ClientGrid.SelectedItem is Client client)
        {
            var dlg = new ClientHistoryDialog(client) { Owner = this };
            dlg.ShowDialog();
        }
    }

    private void CallClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Client clientItem)
        {
            if (string.IsNullOrWhiteSpace(clientItem.Phone))
            {
                MessageBox.Show("Walay phone number kining clienta.", "No Phone", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"tel:{clientItem.Phone}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error calling: {ex.Message}");
                }
            }
            e.Handled = true;
        }
    }

    private void EmailClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Client clientItem)
        {
            if (string.IsNullOrWhiteSpace(clientItem.Email))
            {
                MessageBox.Show("Walay email kining clienta.", "No Email", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"mailto:{clientItem.Email}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening email: {ex.Message}");
                }
            }
            e.Handled = true;
        }
    }

    private void AddClient_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ClientDialog { Owner = this };
        if (dlg.ShowDialog() == true) { Refresh(); RefreshClients(); MainTabs.SelectedIndex = 1; }
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TaskDialog() { Owner = this };
        if (dlg.ShowDialog() == true) { Refresh(); MainTabs.SelectedIndex = 0; }
    }

    private void ShowAgenda_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new DailyAgendaDialog { Owner = this };
        dlg.ShowDialog();
    }


    private string GetDbPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PLCTracker", "tracker.db");
    }

    private void BackupDb_Click(object sender, RoutedEventArgs e)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "SQLite Database (*.db)|*.db",
            FileName = $"PLCTracker_Backup_{DateTime.Now:yyyyMMdd_HHmm}.db"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                File.Copy(GetDbPath(), sfd.FileName, true);
                MessageBox.Show("Backup successful! Naka-save na ang kopya sa imong database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RestoreDb_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "SQLite Database (*.db)|*.db" };

        if (ofd.ShowDialog() == true)
        {
            var confirm = MessageBox.Show("Kining aksyona mo-overwrite sa imong karon nga data. Sigurado ka ba?", "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var dbPath = GetDbPath();
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                File.Copy(ofd.FileName, dbPath, true);

                MessageBox.Show("Database restored successfully! I-restart ang app para makita ang mga kausaban.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restore failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"PLCTracker_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (sfd.ShowDialog() != true) return;

        using var db = new AppDbContext();
        var tasks = db.Tasks.Include(t => t.Client).OrderBy(t => t.DueDate).ToList();

        var csv = new StringBuilder();
        csv.AppendLine("Client Name,Phone,Task Type,Due Date,Status,Notes");

        foreach (var t in tasks)
        {
            var notes = $"\"{t.Notes.Replace("\"", "\"\"")}\"";
            var clientName = $"\"{t.Client?.Name}\"";
            var phone = t.Client?.Phone ?? "";
            var type = t.TypeLabel.Replace("📞 ", "").Replace("🔁 ", "").Replace("📋 ", "").Replace("📌 ", "");

            csv.AppendLine($"{clientName},{phone},{type},{t.DueDate:yyyy-MM-dd HH:mm},{t.StatusLabel},{notes}");
        }

        File.WriteAllText(sfd.FileName, csv.ToString(), Encoding.UTF8);
        MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AutoStart_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            string appName = "PLCTracker";
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName!;

            using var key = Registry.CurrentUser.OpenSubKey(runKey, true);
            if (key == null) throw new Exception("Could not open registry key.");

            if (key.GetValue(appName) != null)
            {
                key.DeleteValue(appName);
                MessageBox.Show("Auto-start DISABLED. Dili na mo-open inig-boot sa PC.", "Auto-Start", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                key.SetValue(appName, $"\"{exePath}\"");
                MessageBox.Show("Auto-start ENABLED! Mo-open na ang app inig-boot sa PC.", "Auto-Start", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error setting auto-start: {ex.Message}");
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized) Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_reallyExit)
        {
            e.Cancel = true;
            Hide();
            if (!_hintShown)
            {
                _hintShown = true;
                _notifier.ShowBalloon("PLC Tracker is still running", "Naa ra ko sa system tray. Right-click → Exit para i-close.", System.Windows.Forms.ToolTipIcon.Info, 5000);
            }
            return;
        }
        base.OnClosing(e);
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
    }

    protected override void OnClosed(EventArgs e)
    {
        _notifier.Dispose();
        base.OnClosed(e);
    }
}