using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class ClientHistoryDialog : Window
{
    public ClientHistoryDialog(Client client)
    {
        InitializeComponent();
        HeaderText.Text = $"History for: {client.Name}";
        LoadHistory(client.Id);
    }

    private void LoadHistory(int clientId)
    {
        using var db = new AppDbContext();
        var logs = db.AuditLogs
            .Where(l => l.ClientId == clientId)
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        HistoryGrid.ItemsSource = logs;
    }
}