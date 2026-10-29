using System;
using System.IO;
using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class AlarmWindow : Window
{
    private readonly TaskItem _task;
    private System.Windows.Media.MediaPlayer? _mediaPlayer; // Gigamit para mo-play sa MP3/WAV

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
        bool customPlayed = false;
        try
        {
            var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PLCTracker");
            var soundPath = Path.Combine(appFolder, "alarm_sound.txt");

            if (File.Exists(soundPath))
            {
                string customSound = File.ReadAllText(soundPath).Trim();
                if (!string.IsNullOrEmpty(customSound) && File.Exists(customSound))
                {
                    _mediaPlayer = new System.Windows.Media.MediaPlayer();
                    _mediaPlayer.Open(new Uri(customSound));
                    _mediaPlayer.Play();
                    customPlayed = true;
                }
            }
        }
        catch
        {
            // Kung naay error sa custom sound, mo-fallback sa default
        }

        if (!customPlayed)
        {
            for (int i = 0; i < 3; i++)
            {
                System.Media.SystemSounds.Exclamation.Play();
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }
    }

    private void Snooze_Click(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.Stop(); // Pundota ang tingog kung nag-play
        using var db = new AppDbContext();
        var dbTask = db.Tasks.Find(_task.Id);
        if (dbTask is not null)
        {
            dbTask.DueDate = DateTime.Now.AddMinutes(5);
            dbTask.IsNotified = false;
            db.SaveChanges();
        }
        Close();
    }

    private void Dismiss_Click(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.Stop(); // Pundota ang tingog kung nag-play
        Close();
    }
}