using System;
using System.Windows;

namespace PLCTracker.Views;

public partial class DateInputDialog : Window
{
    public DateTime SelectedDate { get; private set; }

    public DateInputDialog(string prompt)
    {
        InitializeComponent();
        PromptText.Text = prompt;
        DatePickerControl.SelectedDate = DateTime.Today.AddDays(7);
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        // KINI ANG PINAKA-MODERNO NGA NULL CHECK
        if (DatePickerControl.SelectedDate is not DateTime selectedDate)
        {
            MessageBox.Show("Pili og date.");
            return;
        }

        SelectedDate = selectedDate;
        DialogResult = true;
        Close();
    }
}