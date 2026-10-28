using System.Windows;

namespace PLCTracker.Views;

public partial class DateInputDialog : Window
{
    public DateTime SelectedDate { get; private set; }

    public DateInputDialog(string prompt)
    {
        InitializeComponent();
        PromptText.Text = prompt;
        DatePickerControl.SelectedDate = DateTime.Today.AddDays(7); // default +7 days
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (DatePickerControl.SelectedDate == null)
        {
            MessageBox.Show("Pili og date.");
            return;
        }

        SelectedDate = DatePickerControl.SelectedDate.Value;
        DialogResult = true;
        Close();
    }
}