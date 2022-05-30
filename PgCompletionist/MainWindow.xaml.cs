namespace PgCompletionist;
using PgObjects;
using WpfLayout;

public partial class MainWindow : MainWindowUI
{
    public MainWindow()
    {
        TitleText = $"Project: Gorgon - Completionist";
        StatusText = $"Downloaded content {Groups.Version} copyright © 2022, Elder Game, LLC";

        TaskDispatcher = TaskDispatcher.Create(this);
        IconTools.Initialize(this, Groups.Version, 3467, "PgCompletionist");
    }

    private TaskDispatcher TaskDispatcher;
}
