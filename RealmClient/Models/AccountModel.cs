using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using Newtonsoft.Json;
using RealmClient.AppEngine;
using RealmClient.State;
using RealmClient.Utils;

namespace RealmClient.Models;

public struct AccountResponse {
    public bool Success;
    public string Message;
}

public static class AccountModel1 {
    
    private const string AccountFileName = "account.json";
    private const string UsernameKey = "username";
    private const string PasswordKey = "password";

    private static readonly Logger Log = new(typeof(AccountModel1));
    
    public static bool IsLoggedIn { get; private set; }

    public static string Username{ get; private set; }

    public static string Password { get; private set; }
    
    public static void LoadLocalAccount() {
        var filePath = GetLocalAccountFilePath();

        if (!File.Exists(filePath)) {
            SetNoAccount();
            Log.Debug("No local account data found");
            return;
        }

        string text;
        try {
            text = File.ReadAllText(filePath);
        } catch (Exception e) {
            SetNoAccount();
            Log.Error($"Failed to read to file {AccountFileName}: {e.Message}");
            return;
        }
        
        var info = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

        if (info == null) {
            SetNoAccount();
            Log.Debug("Failed to parse local account data");
            return;
        }

        var loadedUser = info.TryGetValue(UsernameKey, out var username) && !string.IsNullOrWhiteSpace(username);
        var loadedPass = info.TryGetValue(PasswordKey, out var password) && !string.IsNullOrWhiteSpace(password);

        if (!loadedUser || !loadedPass) {
            SetNoAccount();
            Log.Debug("Incomplete/Invalid local account data");
            return;
        }

        var data = new byte[(password.Length * 3 + 3) / 4];
        
        if (!Convert.TryFromBase64String(password, data.AsSpan(), out var count)) {
            SetNoAccount();
            Log.Debug("Invalid Base64 encoding on password");
            return;
        }

        Username = username;
        Password = Encoding.UTF8.GetString(data.AsSpan(0, count));
    }

    private static void SaveLocalAccount() {
        var bytes = new byte[Encoding.UTF8.GetByteCount(Password.AsSpan())];
        if (!Encoding.UTF8.TryGetBytes(Password.AsSpan(), bytes.AsSpan(), out var byteCount)) {
            Log.Error("Failed to get password bytes");
            return;
        }

        var chars = new char[4 * (Password.Length + 2) / 3];
        if (!Convert.TryToBase64Chars(bytes.AsSpan(0, byteCount), chars.AsSpan(), out var charCount)) {
            Log.Error("Failed to Base64 encode password");
            return;
        }

        var username = Username;
        var password = new string(chars.AsSpan(0, charCount));

        var jsonString = JsonConvert.SerializeObject(BuildAccountRequestData(username, password));

        try {
            File.WriteAllText(GetLocalAccountFilePath(), jsonString);
        } catch (Exception e) {
            Log.Error($"Failed to write to file {AccountFileName}: {e.Message}");
        }
    }

    public static void Logout() {
        IsLoggedIn = false;
        Username = string.Empty;
        Password = string.Empty;
        SaveLocalAccount();
    }

    private static Dictionary<string, string> GetAccountRequestData() => new() {{UsernameKey, Username}, {PasswordKey, Password}};
    private static Dictionary<string, string> BuildAccountRequestData(string username, string password) => new() {{UsernameKey, username}, {PasswordKey, password}};

    private static void SetNoAccount() => Username = Password = string.Empty;

    private static string GetLocalAccountFilePath() => Path.Combine(Settings.LocalFolderPath, AccountFileName);
}