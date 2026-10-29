using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using PLCTracker.Data;
using PLCTracker.Models;
using PLCTracker.Services;
using PLCTracker.Views;

namespace PLCTracker;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
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
            var hasUrgentTasks = db.Tasks.Any(t => !t.IsCompleted && !t.IsDeleted && t.DueDate < tomorrow);

            if (hasUrgentTasks)
            {
                var agendaDlg = new DailyAgendaDialog { Owner = this };
                agendaDlg.ShowDialog();
            }
        };
    }

    // ===== HELPER: LOG ACTION =====
    private static void LogAction(int clientId, string action, string details)
    {
        using var db = new AppDbContext();
        db.AuditLogs.Add(new AuditLog
        {
            ClientId = clientId,
            Action = action,
            Details = details,
            Timestamp = DateTime.Now
        });
        db.SaveChanges();
    }

    // ===== NAVIGATION =====
    private void BtnTasks_Click(object sender, RoutedEventArgs e)
    {
        TasksPanel.Visibility = Visibility.Visible;
        ClientsPanel.Visibility = Visibility.Collapsed;
        Refresh();
    }

    private void BtnClients_Click(object sender, RoutedEventArgs e)
    {
        TasksPanel.Visibility = Visibility.Collapsed;
        ClientsPanel.Visibility = Visibility.Visible;
        RefreshClients();
    }

    // ===== TASKS =====
    private void Refresh()
    {
        using var db = new AppDbContext();
        var query = db.Tasks.Include(t => t.Client).Where(t => !t.IsDeleted).AsQueryable();

        if (ShowCompletedCheck.IsChecked is not true)
            query = query.Where(t => !t.IsCompleted);

        if (!string.IsNullOrWhiteSpace(TaskSearchBox.Text))
        {
            var search = TaskSearchBox.Text;
            query = query.Where(t => t.Client!.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     t.Notes.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     t.Client!.Phone.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var tasks = query.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate).ToList();
        TaskGrid.ItemsSource = tasks;

        var allTasks = db.Tasks.Where(t => !t.IsDeleted).ToList();
        var overdue = allTasks.Count(t => !t.IsCompleted && t.DueDate < DateTime.Now);
        var today = allTasks.Count(t => !t.IsCompleted && t.DueDate.Date == DateTime.Today && t.DueDate >= DateTime.Now);
        var completed = allTasks.Count(t => t.IsCompleted);
        StatusText.Text = $"🔴 Overdue: {overdue}  |  🟡 Today: {today}  |  ✅ Done: {completed}";
    }

    private void TaskSearchBox_TextChanged(object sender, TextChangedEventArgs e) => Refresh();
    private void ShowCompletedCheck_Changed(object sender, RoutedEventArgs e) => Refresh();

    private void CompleteTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is TaskItem taskItem)
        {
            using var db = new AppDbContext();
            var task = db.Tasks.Find(taskItem.Id);
            if (task is not null)
            {
                task.IsCompleted = !task.IsCompleted;
                task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
                db.SaveChanges();
                LogAction(task.ClientId, task.IsCompleted ? "Task Completed" : "Task Reopened", $"Task: {task.TypeLabel} - {task.Notes}");
                Refresh();
            }
            e.Handled = true;
        }
    }

    private void EditTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is TaskItem taskItem)
        {
            using var db = new AppDbContext();
            var task = db.Tasks.Find(taskItem.Id);
            if (task is not null)
            {
                var dlg = new TaskDialog(task) { Owner = this };
                if (dlg.ShowDialog() is true)
                {
                    LogAction(task.ClientId, "Task Edited", $"Updated Task: {task.TypeLabel}");
                    Refresh();
                }
            }
            e.Handled = true;
        }
    }

    private void DeleteTask_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is TaskItem taskItem)
        {
            if (System.Windows.MessageBox.Show("Delete this task?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using var db = new AppDbContext();
                var task = db.Tasks.Find(taskItem.Id);
                if (task is not null)
                {
                    task.IsDeleted = true;
                    db.SaveChanges();
                    LogAction(task.ClientId, "Task Deleted", $"Deleted Task: {task.TypeLabel}");
                }
                Refresh();
            }
            e.Handled = true;
        }
    }

    // ===== CLIENTS =====
    private void RefreshClients()
    {
        using var db = new AppDbContext();
        var query = db.Clients.Where(c => !c.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(ClientSearchBox.Text))
        {
            var search = ClientSearchBox.Text;
            query = query.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     c.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var clients = query.OrderBy(c => c.Name).ToList();
        ClientGrid.ItemsSource = clients;
        ClientCountText.Text = $"Total Clients: {clients.Count}";
    }

    private void ClientSearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshClients();

    private void EditClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is Client clientItem)
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(clientItem.Id);
            if (client is not null)
            {
                var oldStatus = client.Status;
                var dlg = new ClientDialog(client) { Owner = this };
                if (dlg.ShowDialog() is true)
                {
                    // KUNG NAG-USAB ANG STATUS, I-LOG ANG REASON UG TIMESTAMP
                    if (oldStatus != dlg.UpdatedStatus)
                    {
                        string reasonLog = string.IsNullOrWhiteSpace(dlg.UpdatedReason) ? "No reason provided." : dlg.UpdatedReason;
                        LogAction(client.Id, "Status Changed", $"Moved from {oldStatus} to {dlg.UpdatedStatus}. Reason: {reasonLog}");
                    }
                    else
                    {
                        LogAction(client.Id, "Client Profile Updated", $"Status: {client.Status}");
                    }

                    Refresh(); RefreshClients();
                }
            }
            e.Handled = true;
        }
    }

    private void DeleteClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is Client clientItem)
        {
            using var db = new AppDbContext();
            var client = db.Clients.Include(c => c.Tasks).FirstOrDefault(c => c.Id == clientItem.Id);
            if (client is not null)
            {
                var msg = $"Delete '{client.Name}' and {client.Tasks.Count} task(s)?";
                if (System.Windows.MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    client.IsDeleted = true;
                    foreach (var t in client.Tasks) t.IsDeleted = true;
                    db.SaveChanges();

                    LogAction(client.Id, "Client Deleted", "Client moved to archive.");
                    Refresh(); RefreshClients();
                }
            }
            e.Handled = true;
        }
    }

    private void AddClient_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ClientDialog { Owner = this };
        if (dlg.ShowDialog() is true)
        {
            LogAction(dlg.SavedClientId, "Client Added", $"New referral registered. Status: {dlg.UpdatedStatus}");
            Refresh(); RefreshClients();
            BtnClients_Click(this, new RoutedEventArgs());
        }
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TaskDialog() { Owner = this };
        if (dlg.ShowDialog() is true)
        {
            using var db = new AppDbContext();
            var newTask = db.Tasks.Find(dlg.SavedTaskId);
            if (newTask is not null) LogAction(newTask.ClientId, "Task Created", $"Type: {newTask.TypeLabel}");

            Refresh();
            BtnTasks_Click(this, new RoutedEventArgs());
        }
    }

    // ===== MGA LAING METHODS (Parehas sa miaging code) =====
    private void CopyPhone_Click(object sender, RoutedEventArgs e)
    {
        if (TaskGrid.SelectedItem is TaskItem task && task.Client is not null)
        {
            Clipboard.SetText(task.Client.Phone);
            System.Windows.MessageBox.Show($"Copied: {task.Client.Phone}", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else System.Windows.MessageBox.Show("Pilia una ang task.");
    }

    private void Snooze15_Click(object sender, RoutedEventArgs e) => SnoozeTask(15);
    private void Snooze1H_Click(object sender, RoutedEventArgs e) => SnoozeTask(60);

    private void SnoozeTask(int minutes)
    {
        if (TaskGrid.SelectedItem is not TaskItem task) return;
        using var db = new AppDbContext();
        var dbTask = db.Tasks.Find(task.Id);
        if (dbTask is not null)
        {
            dbTask.DueDate = DateTime.Now.AddMinutes(minutes);
            dbTask.IsNotified = false;
            db.SaveChanges();
            LogAction(dbTask.ClientId, "Task Snoozed", $"Snoozed for {minutes} mins");
            Refresh();
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
        if (sender is System.Windows.Controls.Button btn && btn.Tag is Client clientItem && !string.IsNullOrWhiteSpace(clientItem.Phone))
        {
            LogAction(clientItem.Id, "Called Client", $"Dialed: {clientItem.Phone}");
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"tel:{clientItem.Phone}") { UseShellExecute = true }); }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Error calling: {ex.Message}"); }
        }
    }

    private void EmailClient_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is Client clientItem && !string.IsNullOrWhiteSpace(clientItem.Email))
        {
            LogAction(clientItem.Id, "Emailed Client", $"To: {clientItem.Email}");
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"mailto:{clientItem.Email}") { UseShellExecute = true }); }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Error emailing: {ex.Message}"); }
        }
    }

    private void PrintSummary_Click(object sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var tomorrow = DateTime.Today.AddDays(1);
        var tasks = db.Tasks.Include(t => t.Client)
            .Where(t => !t.IsCompleted && !t.IsDeleted && t.DueDate < tomorrow)
            .OrderBy(t => t.DueDate)
            .ToList();

        if (tasks.Count == 0)
        {
            System.Windows.MessageBox.Show("Walay tasks for today nga ma-print.", "Empty", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = new FlowDocument { PagePadding = new Thickness(50), ColumnWidth = double.PositiveInfinity };

        doc.Blocks.Add(new Paragraph(new Run("PLC Intake Tracker - Daily Call Sheet"))
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        });

        doc.Blocks.Add(new Paragraph(new Run($"Date Printed: {DateTime.Now:MMMM dd, yyyy hh:mm tt}"))
        {
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(150) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(250) });

        var rowGroup = new TableRowGroup();
        table.RowGroups.Add(rowGroup);

        var headerRow = new TableRow { Background = Brushes.LightGray };
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Client Name")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Phone")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Due Time")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Task Type")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Notes")) { FontWeight = FontWeights.Bold }));
        rowGroup.Rows.Add(headerRow);

        foreach (var task in tasks)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(task.Client?.Name ?? "Unknown"))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(task.Client?.Phone ?? ""))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(task.DueDate.ToString("hh:mm tt")))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(task.TypeLabel))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(task.Notes))));
            rowGroup.Rows.Add(row);
        }

        doc.Blocks.Add(table);

        PrintDialog printDialog = new();
        if (printDialog.ShowDialog() == true)
        {
            doc.PageHeight = printDialog.PrintableAreaHeight;
            doc.PageWidth = printDialog.PrintableAreaWidth;
            IDocumentPaginatorSource idpSource = doc;
            printDialog.PrintDocument(idpSource.DocumentPaginator, "PLC Daily Call Sheet");
        }
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"PLCTracker_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (sfd.ShowDialog() is not true) return;

        using var db = new AppDbContext();
        var tasks = db.Tasks.Include(t => t.Client).Where(t => !t.IsDeleted).OrderBy(t => t.DueDate).ToList();

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
        System.Windows.MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportCsv_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv", Title = "Select CSV file to import clients" };
        if (ofd.ShowDialog() is not true) return;

        try
        {
            int importedCount = 0;
            using var db = new AppDbContext();
            using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(ofd.FileName);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(",");

            if (!parser.EndOfData) parser.ReadFields();

            while (!parser.EndOfData)
            {
                string[]? fields = parser.ReadFields();
                if (fields is null || fields.Length < 1) continue;

                string name = fields[0].Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                string phone = fields.Length > 1 ? fields[1].Trim() : "";
                string email = fields.Length > 2 ? fields[2].Trim() : "";
                string notes = fields.Length > 3 ? fields[3].Trim() : "";

                ClientStatus status = ClientStatus.NewReferral;
                if (fields.Length > 4 && Enum.TryParse<ClientStatus>(fields[4].Trim(), true, out var parsedStatus))
                    status = parsedStatus;

                if (db.Clients.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted)) continue;

                var newClient = new Client { Name = name, Phone = phone, Email = email, Notes = notes, Status = status };
                db.Clients.Add(newClient);
                importedCount++;
            }

            if (importedCount > 0)
            {
                db.SaveChanges();
                System.Windows.MessageBox.Show($"Successfully imported {importedCount} clients!", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshClients();
                BtnClients_Click(this, new RoutedEventArgs());
            }
            else System.Windows.MessageBox.Show("No new clients found.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private static string GetDbPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PLCTracker", "tracker.db");

    private void BackupDb_Click(object sender, RoutedEventArgs e)
    {
        var sfd = new SaveFileDialog { Filter = "SQLite Database (*.db)|*.db", FileName = $"PLCTracker_Backup_{DateTime.Now:yyyyMMdd_HHmm}.db" };
        if (sfd.ShowDialog() is true)
        {
            try { File.Copy(GetDbPath(), sfd.FileName, true); System.Windows.MessageBox.Show("Backup successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Backup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private void RestoreDb_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "SQLite Database (*.db)|*.db" };
        if (ofd.ShowDialog() is true)
        {
            var confirm = System.Windows.MessageBox.Show("Overwrite current data?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                File.Copy(ofd.FileName, GetDbPath(), true);
                System.Windows.MessageBox.Show("Restored! App will restart.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Restore failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private void AutoStart_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            string appName = "PLCTracker";

            string exePath = Environment.ProcessPath ?? throw new Exception("Could not get process path.");

            using var key = Registry.CurrentUser.OpenSubKey(runKey, true) ?? throw new Exception("Could not open registry key.");

            if (key.GetValue(appName) is not null) { key.DeleteValue(appName); System.Windows.MessageBox.Show("Auto-start DISABLED.", "Auto-Start"); }
            else { key.SetValue(appName, $"\"{exePath}\""); System.Windows.MessageBox.Show("Auto-start ENABLED!", "Auto-Start"); }
        }
        catch (Exception ex) { System.Windows.MessageBox.Show($"Error: {ex.Message}"); }
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
                _notifier.ShowBalloon("PLC Tracker is still running", "Naa ra ko sa system tray.", System.Windows.Forms.ToolTipIcon.Info, 5000);
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