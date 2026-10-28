using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class TaskDialog : Window
{
    private readonly int? _taskId = null;

    public TaskDialog() : this(TaskType.Custom, DateTime.Now.AddHours(1), "") { } // For ADD

    public TaskDialog(TaskType type, DateTime due, string notes)
    {
        InitializeComponent();
        LoadClients();
        TypeCombo.SelectedIndex = (int)type;
        DueDatePicker.SelectedDate = due.Date;
        DueTimeBox.Text = due.ToString("HH:mm");
        NotesBox.Text = notes;
    }

    public TaskDialog(TaskItem existingTask) // For EDIT
    {
        InitializeComponent();
        _taskId = existingTask.Id;
        LoadClients();

        // Set selected client
        using var db = new AppDbContext();
        var client = db.Clients.Find(existingTask.ClientId);
        ClientCombo.SelectedItem = client;

        TypeCombo.SelectedIndex = (int)existingTask.Type;
        DueDatePicker.SelectedDate = existingTask.DueDate.Date;
        DueTimeBox.Text = existingTask.DueDate.ToString("HH:mm");
        NotesBox.Text = existingTask.Notes;
    }

    private void LoadClients()
    {
        using var db = new AppDbContext();
        ClientCombo.ItemsSource = db.Clients.OrderBy(c => c.Name).ToList();
        if (ClientCombo.Items.Count > 0 && _taskId == null) ClientCombo.SelectedIndex = 0;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (ClientCombo.SelectedItem is not Client client)
        {
            MessageBox.Show("Pili og client.");
            return;
        }

        if (!TimeSpan.TryParse(DueTimeBox.Text, out var time))
        {
            MessageBox.Show("Time format: HH:mm (e.g. 14:30)");
            return;
        }

        var due = DueDatePicker.SelectedDate!.Value.Date + time;
        using var db = new AppDbContext();

        if (_taskId == null) // ADD
        {
            db.Tasks.Add(new TaskItem
            {
                ClientId = client.Id,
                Type = (TaskType)TypeCombo.SelectedIndex,
                DueDate = due,
                Notes = NotesBox.Text.Trim()
            });
        }
        else // EDIT
        {
            var task = db.Tasks.Find(_taskId);
            if (task != null)
            {
                task.ClientId = client.Id;
                task.Type = (TaskType)TypeCombo.SelectedIndex;
                task.DueDate = due;
                task.Notes = NotesBox.Text.Trim();
                task.IsNotified = false; // Reset notification
            }
        }

        db.SaveChanges();
        DialogResult = true;
        Close();
    }
}