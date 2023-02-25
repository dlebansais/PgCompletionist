namespace System.IO;

using System;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

public static class LocalStorage
{
    public static async Task<T?> GetItemAsync<T>(string name)
        where T : class, new()
    {
        T? Result = null;

        try
        {
            string KeyName = TypeSettingToName<T>(name);
            RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            Key = Key?.CreateSubKey("Project Gorgon Tools");
            RegistryKey? SettingKey = Key?.CreateSubKey(KeyName);

            string? JsonString = SettingKey?.GetValue("Content") as string;
            Result = JsonString is not null ? JsonSerializer.Deserialize<T>(JsonString) : null;

            await Task.Run(() => { });
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }

        return Result;
    }

    public static async Task SetItemAsync<T>(string name, T value)
    {
        string JsonString = JsonSerializer.Serialize(value);

        try
        {
            string KeyName = TypeSettingToName<T>(name);
            RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            Key = Key?.CreateSubKey("Project Gorgon Tools");
            RegistryKey? SettingKey = Key?.CreateSubKey(KeyName);

            SettingKey?.SetValue("Content", JsonString, RegistryValueKind.String);

            await Task.Run(() => { });
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
    }

    private static string TypeSettingToName<T>(string name)
    {
        string KeyName = typeof(T).FullName;
        KeyName = KeyName.Replace(".", "\\");
        if (name != string.Empty)
            KeyName += $"\\{name}";

        return KeyName;
    }
}
