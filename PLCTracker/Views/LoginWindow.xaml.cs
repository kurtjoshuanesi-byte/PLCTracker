using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PLCTracker.Views;

public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly string _pinFilePath;

    public LoginWindow()
    {
        InitializeComponent();

        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PLCTracker");
        Directory.CreateDirectory(appFolder);
        _pinFilePath = Path.Combine(appFolder, "pin.txt");

        if (!File.Exists(_pinFilePath)) File.WriteAllText(_pinFilePath, "1234");

        PinBox.Focus();
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        var savedPin = File.ReadAllText(_pinFilePath).Trim();

        if (PinBox.Password == savedPin)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ErrorText.Text = "Incorrect PIN. Try again.";
            PinBox.Clear();
            PinBox.Focus();
        }
    }

    private void PinBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter) Unlock_Click(sender, e);
    }

    private void ForgotPin_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show("Reset PIN to default ('1234')?", "Forgot PIN", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            File.WriteAllText(_pinFilePath, "1234");
            System.Windows.MessageBox.Show("PIN reset to '1234'. Try logging in again.", "Reset Successful");
        }
    }
}