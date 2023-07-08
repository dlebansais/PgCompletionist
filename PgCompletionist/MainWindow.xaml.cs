namespace PgCompletionist;

using PgObjects;
using WpfLayout;

public partial class MainWindow : MainWindowUI
{
    public MainWindow()
    {
        TitleText = $"Project: Gorgon - Completionist";
        StatusText = $"Downloaded content {Groups.Version} copyright © 2023, Elder Game, LLC";

        TaskDispatcher = TaskDispatcher.Create(this);
    }

    private TaskDispatcher TaskDispatcher;
}
