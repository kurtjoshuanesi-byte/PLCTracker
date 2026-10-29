using System.Windows;
using PLCTracker.Data;
using PLCTracker.Models;

namespace PLCTracker.Views;

public partial class ClientDialog : Window
{
    private readonly int? _clientId = null;
    public int SavedClientId { get; private set; }

    public ClientDialog()
    {
        InitializeComponent();
        StatusCombo.SelectedIndex = 0;
    }

    public ClientDialog(Client existingClient)
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

        if (_clientId is null)
        {
            var newClient = new Client
            {
                Name = NameBox.Text.Trim(),
                Phone = PhoneBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                Notes = NotesBox.Text.Trim(),
                Status = (ClientStatus)StatusCombo.SelectedIndex
            };
            db.Clients.Add(newClient);
            db.SaveChanges();
            SavedClientId = newClient.Id;
        }
        else
        {
            var client = db.Clients.Find(_clientId);
            if (client is not null)
            {
                client.Name = NameBox.Text.Trim();
                client.Phone = PhoneBox.Text.Trim();
                client.Email = EmailBox.Text.Trim();
                client.Notes = NotesBox.Text.Trim();
                client.Status = (ClientStatus)StatusCombo.SelectedIndex;
                db.SaveChanges();
                SavedClientId = client.Id;
            }
        }

        DialogResult = true;
        Close();
    }
}