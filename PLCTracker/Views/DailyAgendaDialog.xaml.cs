using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PLCTracker.Data;

namespace PLCTracker.Views;

public partial class DailyAgendaDialog : Window
{
    public DailyAgendaDialog()
    {
        InitializeComponent();
        LoadAgenda();
    }

    private void LoadAgenda()
    {
        using var db = new AppDbContext();

        var tomorrow = DateTime.Today.AddDays(1);

        var tasks = db.Tasks
            .Include(t => t.Client)
            .Where(t => !t.IsCompleted && t.DueDate < tomorrow)
            .OrderBy(t => t.DueDate)
            .ToList();

        // Giusab dinhi: Gikuha ang "TypeLabel =" ug "Notes =" kay automatic na siya ma-infer sa C#
        var displayList = tasks.Select(t => new
        {
            TimeString = t.DueDate < DateTime.Now ? "OVERDUE" : t.DueDate.ToString("hh:mm tt"),
            t.TypeLabel,
            ClientName = t.Client?.Name ?? "Unknown",
            t.Notes
        }).ToList();

        AgendaList.ItemsSource = displayList;

        var overdueCount = tasks.Count(t => t.DueDate < DateTime.Now);
        var todayCount = tasks.Count(t => t.DueDate.Date == DateTime.Today && t.DueDate >= DateTime.Now);

        if (tasks.Count == 0)
        {
            SummaryText.Text = "Wala kay tasks for today. Free ka! 🎉";
            AgendaList.Visibility = Visibility.Collapsed;
        }
        else
        {
            SummaryText.Text = $"Naa kay {overdueCount} overdue ug {todayCount} tasks nga kinahanglan buhaton karon.";
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}