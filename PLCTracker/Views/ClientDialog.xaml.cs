using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class ClientDialog : Window
{
    private readonly int? _clientId = null;

    public ClientDialog() // For ADDING
    {
        InitializeComponent();
        StatusCombo.SelectedIndex = 0;
    }

    public ClientDialog(Client existingClient) // For EDITING
    {
        InitializeComponent();
        _clientId = existingClient.Id;
        NameBox.Text = existingClient.Name;
        PhoneBox.Text = existingClient.Phone;
        EmailBox.Text = existingClient.Email;
        NotesBox.Text = existingClient.Notes;
        StatusCombo.SelectedIndex = (int)existingClient.Status;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Kinahanglan naay name ang client.");
            return;
        }

        using var db = new AppDbContext();

        if (_clientId == null) // ADD
        {
            db.Clients.Add(new Client
            {
                Name = NameBox.Text.Trim(),
                Phone = PhoneBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                Notes = NotesBox.Text.Trim(),
                Status = (ClientStatus)StatusCombo.SelectedIndex
            });
        }
        else // EDIT
        {
            var client = db.Clients.Find(_clientId);
            if (client != null)
            {
                client.Name = NameBox.Text.Trim();
                client.Phone = PhoneBox.Text.Trim();
                client.Email = EmailBox.Text.Trim();
                client.Notes = NotesBox.Text.Trim();
                client.Status = (ClientStatus)StatusCombo.SelectedIndex;
            }
        }

        db.SaveChanges();
        DialogResult = true;
        Close();
    }
}