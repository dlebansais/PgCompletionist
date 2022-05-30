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
            RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            Key = Key?.CreateSubKey("Project Gorgon Tools");
            RegistryKey? SettingKey = Key?.CreateSubKey(name);

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
            RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            Key = Key?.CreateSubKey("Project Gorgon Tools");
            RegistryKey? SettingKey = Key?.CreateSubKey(name);

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
}
