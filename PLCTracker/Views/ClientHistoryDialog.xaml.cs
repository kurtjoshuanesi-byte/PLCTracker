using System.Windows;
using Microsoft.EntityFrameworkCore;
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
        var tasks = db.Tasks
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.DueDate)
            .ToList();

        HistoryGrid.ItemsSource = tasks;
    }
}