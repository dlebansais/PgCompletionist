using System.Threading.Tasks;

namespace System.Windows;

public static class DialogBox
{
    public static async Task<MessageBoxResult> Show(string messageBoxText)
    {
        await Task.CompletedTask;
        return MessageBox.Show(messageBoxText);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string caption)
    {
        await Task.CompletedTask;
        return MessageBox.Show(messageBoxText, caption);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string caption, MessageBoxButton button)
    {
        await Task.CompletedTask;
        return MessageBox.Show(messageBoxText, caption, button);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        await Task.CompletedTask;
        return MessageBox.Show(messageBoxText, caption, button, icon);
    }
}
