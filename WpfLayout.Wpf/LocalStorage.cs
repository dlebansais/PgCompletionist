namespace System.IO;

using System;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

public static class LocalStorage
{
    public static async Task<T?> GetItemAsync<T>(string keyName)
        where T : class, new()
    {
        T? Result = null;

        try
        {
            using RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            using RegistryKey? PgKey = Key?.CreateSubKey("Project Gorgon Tools");
            using RegistryKey? SettingKey = PgKey?.CreateSubKey(keyName);

            string? JsonString = SettingKey?.GetValue("Content") as string;
            Result = JsonString is not null ? JsonSerializer.Deserialize<T>(JsonString) : null;
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

        await Task.Run(() => { });

        return Result;
    }

    public static async Task SetItemAsync<T>(string keyName, T value)
    {
        string JsonString = JsonSerializer.Serialize(value);

        try
        {
            using RegistryKey? Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            using RegistryKey? PgKey = Key?.CreateSubKey("Project Gorgon Tools");
            using RegistryKey? SettingKey = PgKey?.CreateSubKey(keyName);

            SettingKey?.SetValue("Content", JsonString, RegistryValueKind.String);
            SettingKey?.Flush();
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

        await Task.Run(() => { });
    }
}
