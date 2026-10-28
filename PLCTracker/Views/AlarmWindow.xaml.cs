using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class AlarmWindow : Window
{
    private readonly TaskItem _task;

    public AlarmWindow(TaskItem task)
    {
        InitializeComponent();
        _task = task;
        HeaderText.Text = $"⏰ TASK ALARM: {task.TypeLabel}";
        TaskInfoText.Text = $"Client: {task.Client?.Name}\nDue: {task.DueDate:hh:mm tt}";
        NotesText.Text = string.IsNullOrWhiteSpace(task.Notes) ? "No notes." : $"Notes: {task.Notes}";
        PlayAlarmSound();
    }

    private async void PlayAlarmSound()
    {
        for (int i = 0; i < 3; i++)
        {
            System.Media.SystemSounds.Exclamation.Play();
            await System.Threading.Tasks.Task.Delay(1000);
        }
    }

    private void Snooze_Click(object sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var dbTask = db.Tasks.Find(_task.Id);
        if (dbTask != null)
        {
            dbTask.DueDate = DateTime.Now.AddMinutes(5);
            dbTask.IsNotified = false;
            db.SaveChanges();
        }
        Close();
    }

    private void Dismiss_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}