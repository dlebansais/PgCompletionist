namespace PgCompletionist;

using System.Windows;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        MainWindow MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
