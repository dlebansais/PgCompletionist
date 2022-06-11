namespace WpfLayout;

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class FileDialogButton
{
    private static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached("Mode", typeof(FileDialogMode), typeof(FileDialogButton), new PropertyMetadata(new PropertyChangedCallback(OnModeChanged)));

    public static FileDialogMode GetMode(DependencyObject obj)
    {
        return (FileDialogMode)obj.GetValue(ModeProperty);
    }

    public static void SetMode(DependencyObject obj, FileDialogMode value)
    {
        obj.SetValue(ModeProperty, value);
    }

    private static void OnModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is Button AsButton)
        {
            AsButton.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AsButton.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        }
    }

    private static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(string), typeof(FileDialogButton), new PropertyMetadata(new PropertyChangedCallback(OnFilterChanged)));

    public static string? GetFilter(DependencyObject obj)
    {
        return obj.GetValue(FilterProperty) as string;
    }

    public static void SetFilter(DependencyObject obj, string? value)
    {
        obj.SetValue(FilterProperty, value);
    }

    private static void OnFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
    }

    private static readonly DependencyProperty InitialDirectoryProperty = DependencyProperty.RegisterAttached("InitialDirectory", typeof(string), typeof(FileDialogButton), new PropertyMetadata(new PropertyChangedCallback(OnInitialDirectoryChanged)));

    public static string? GetInitialDirectory(DependencyObject obj)
    {
        return obj.GetValue(InitialDirectoryProperty) as string;
    }

    public static void SetInitialDirectory(DependencyObject obj, string? value)
    {
        obj.SetValue(InitialDirectoryProperty, value);
    }

    private static void OnInitialDirectoryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs args)
    {
        if (sender is Button AsButton)
        {
            FileDialogMode Mode = GetMode(AsButton);

            switch (Mode)
            {
                case FileDialogMode.Open:
                    args.Handled = true;
                    ShowDialogAndReportClick(new OpenFileDialog(), AsButton, Mode);
                    break;

                case FileDialogMode.OpenMultiple:
                    args.Handled = true;
                    ShowDialogAndReportClick(new OpenFileDialog() { Multiselect = true }, AsButton, Mode);
                    break;

                case FileDialogMode.Save:
                    args.Handled = true;
                    ShowDialogAndReportClick(new SaveFileDialog(), AsButton, Mode);
                    break;

                default:
                    break;
            }
        }
    }

    private static string GetInitialDirectoryPath(Button button)
    {
        string StartingFolder = string.Empty;

        string? InitialDirectory = GetInitialDirectory(button);
        if (InitialDirectory is not null)
        {
            int StartIndex = InitialDirectory.IndexOf("/");

            string GuidString = StartIndex > 0 ? InitialDirectory.Substring(0, StartIndex) : InitialDirectory;
            string RemainingPath = StartIndex > 0 ? InitialDirectory.Substring(StartIndex + 1) : string.Empty;
            
            if (Guid.TryParse(GuidString, out Guid KnownFolderId))
                StartingFolder = NativeMethods.GetKnownFolderPath(KnownFolderId);

            if (RemainingPath.Length > 0)
                StartingFolder = Path.Combine(StartingFolder, RemainingPath);

            StartingFolder = Path.GetFullPath(StartingFolder);
        }

        return StartingFolder;
    }

    private static void ShowDialogAndReportClick(FileDialog dlg, Button button, FileDialogMode mode)
    {
        string? Filter = GetFilter(button);
        if (Filter is not null)
            dlg.Filter = Filter;

        dlg.InitialDirectory = GetInitialDirectoryPath(button);

        bool? DialogResult = dlg.ShowDialog();

        if (DialogResult.HasValue && DialogResult.Value == true)
        {
            switch (mode)
            {
                case FileDialogMode.Open:
                    ReportOpen(dlg.FileName, button.Command);
                    break;
                case FileDialogMode.OpenMultiple:
                    ReportOpenMultiple(dlg.FileNames, button.Command);
                    break;
                case FileDialogMode.Save:
                    ReportSave(dlg.FileName, button.Command);
                    break;
            }
        }
    }

    private static void ReportOpen(string? fileName, ICommand command)
    {
        if (fileName is not null)
        {
            string Content = string.Empty;
            bool IsFileCopied;

            try
            {
                using FileStream FileStream = new(fileName, FileMode.Open, FileAccess.Read);
                using StreamReader Reader = new(FileStream);
                Content = Reader.ReadToEnd();
                IsFileCopied = true;
            }
            catch
            {
                IsFileCopied = false;
            }

            if (IsFileCopied)
            {
                FileDialogResult Result = new(FileDialogMode.Open, fileName, Content);
                command.Execute(Result);
            }
        }
    }

    private static void ReportOpenMultiple(string[] fileNames, ICommand command)
    {
        Dictionary<string, string?> FileContentTable = new();

        foreach (string FileName in fileNames)
        {
            string? Content;

            try
            {
                using FileStream FileStream = new(FileName, FileMode.Open, FileAccess.Read);
                using StreamReader Reader = new(FileStream);
                Content = Reader.ReadToEnd();
            }
            catch
            {
                Content = null;
            }

            FileContentTable.Add(FileName, Content);
        }

        if (FileContentTable.Count > 0)
        {
            FileDialogResult Result = new(FileDialogMode.Open, FileContentTable);
            command.Execute(Result);
        }
    }

    private static void ReportSave(string? fileName, ICommand command)
    {
        if (fileName is not null)
        {
            FileDialogResult Result = new(FileDialogMode.Save, fileName);
            command.Execute(Result);

            try
            {
                using FileStream FileStream = new(fileName, FileMode.Create, FileAccess.Write);
                using StreamWriter Writer = new(FileStream);
                Writer.Write(Result.Content);
                Writer.Flush();
            }
            catch
            {
            }
        }
    }
}
