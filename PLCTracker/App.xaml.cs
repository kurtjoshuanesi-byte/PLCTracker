using System;
using System.Windows;
using PLCTracker.Views;

namespace PLCTracker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Pugngan ang app sa pag-exit kung mo-close ang Login Window
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            var login = new LoginWindow();
            bool? result = login.ShowDialog();

            if (result == true)
            {
                var main = new MainWindow();
                MainWindow = main;
                main.Show();

                // Ibalik sa normal nga shutdown mode kung ma-close ang Main Window
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}\n\nSiguroa nga gi-delete nimo ang daan nga tracker.db sa AppData!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}