using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Common;
using MonoClient.Data.XmlModels;
using MonoClient.AppEngine;
using MonoClient.State;
using MonoClient.Utils;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MonoClient.Display;
using MonoClient.Ui.Components.Dialogs;

namespace MonoClient.Data;

public struct LoginResponse {
    public bool Success;
    public string Message;
}

public static class Account {
    private const string AccountPath = "account.json";

    private static readonly Logger Log = new(typeof(Account));

    public static AccountModel Model;
    public static bool LoggedIn;

    public static int SelectedCharacterId;
    public static ushort CharacterType;

    public static string Username = string.Empty;

    public static string Email = string.Empty;
    public static string Password = string.Empty;

    public static async Task LoadAsync() {
        if (TryLoadLocalDetails()) {
            var response = await LoginAsync(Email, Password);
            if (response.Success) {
                await AppInit.LoadAsync();
                var charListResponse = await CharacterList.LoadAsync();
                if (charListResponse.Success) {
                    return;
                }

                DialogManager.Enqueue(new Dialog("Error", charListResponse.Message, new DialogOption("Ok")));
                return;
            }
            DialogManager.Enqueue(new Dialog("Error", response.Message, new DialogOption("Ok")));
        }

        Email = new Guid().ToString();
        await CharacterList.LoadAsync();
    }

    private static bool TryLoadLocalDetails() {
        try {
            var jsonStr = File.ReadAllText(AccountPath);
            var accountDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            var email = accountDetails["email"];
            if (string.IsNullOrEmpty(email)) {
                return false;
            }

            var pass = accountDetails["password"];
            if (string.IsNullOrEmpty(pass)) {
                return false;
            }

            var passwordBytes = Convert.FromBase64String(pass);
            Password = Encoding.UTF8.GetString(passwordBytes);
            Email = email;

            return true;
        }
        catch (Exception e) {
            Log.Error($"Error loading account details: {e.Message}");
            return false;
        }
    }

    private static void SaveAccountDetails() {
        try {
            var passwordBytes = Encoding.UTF8.GetBytes(Password);
            var password = Convert.ToBase64String(passwordBytes);
            var accountDetails = new Dictionary<string, string> {
                { "email", Email },
                { "password", password }
            };

            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            var jsonStr = JsonConvert.SerializeObject(accountDetails, jsonSettings);
            File.WriteAllText("account.json", jsonStr);
        }
        catch (Exception e) {
            Log.Error($"Error saving account details: {e.Message}");
        }
    }

    public static async Task<LoginResponse> LoginAsync(string email, string password) {
        var response = await AppEngineClient.SendRequest("/account/verify", new Dictionary<string, string> {
            { "guid", email },
            { "password", password }
        }, retries: 3);
        if (response == null) {
            return new LoginResponse { Success = false, Message = "Failed to contact server." };
        }

        if (response.Contains("Bad Login")) {
            return new LoginResponse { Success = false, Message = "Bad login." };
        }

        Model = XmlSerializer<AccountModel>.Deserialize(response);

        Username = Model.Name;

        Log.Trace($"Logged in as {Username}.");

        Email = email;
        Password = password;

        LoggedIn = true;

        SaveAccountDetails();

        return new LoginResponse { Success = true };
    }

    public static async Task<LoginResponse> Register(string emailTextValue, string passwordTextValue) {
        var request = await AppEngineClient.SendRequest("/account/register", new Dictionary<string, string> {
            { "guid", new Guid().ToString() },
            { "newGUID", emailTextValue },
            { "newPassword", passwordTextValue }
        }, retries: 3);
        
        if (request == null) {
            return new LoginResponse { Success = false, Message = "Failed to contact server." };
        }

        var xElement = XElement.Parse(request);
        var error = xElement.Value;

        if (error == "") {
            return new LoginResponse { Success = true };
        }

        return new LoginResponse { Success = false, Message = error };
    }

    public static async Task<LoginResponse> SetName(string name) {
        var request = await AppEngineClient.SendRequest("/account/setName", new Dictionary<string, string>() {
            { "guid", Email },
            { "password", Password },
            { "name", name }
        });
        
        if (request == null) {
            return new LoginResponse { Success = false, Message = "Failed to contact server." };
        }
        
        var xElement = XElement.Parse(request);
        var error = xElement.Value;
        
        if (error == "") {
            Username = name;
            return new LoginResponse { Success = true };
        }
        
        return new LoginResponse { Success = false, Message = error };
    }
    
    public static async Task<LoginResponse> PurchaseCharSlot() {
        var response = await AppEngineClient.SendRequest("/account/purchaseCharSlot", new Dictionary<string, string> {
            { "guid", Email },
            { "password", Password }
        }, retries: 3);
        
        if (response == null) {
            return new LoginResponse { Success = false, Message = "Failed to contact server." };
        }

        var xElement = XElement.Parse(response);
        var error = xElement.Value;
        
        return error == "" ? new LoginResponse { Success = true } : new LoginResponse { Success = false, Message = error };
    }
    
    public static async Task<LoginResponse> PurchaseClassUnlock(ushort classType) {
        var response = await AppEngineClient.SendRequest("/char/purchaseClassUnlock", new Dictionary<string, string> {
            { "guid", Email },
            { "password", Password },
            { "classType", classType.ToString() }
        }, retries: 3);
        
        if (response == null) {
            return new LoginResponse { Success = false, Message = "Failed to contact server." };
        }

        var xElement = XElement.Parse(response);
        var error = xElement.Value;
        
        return error == "" ? new LoginResponse { Success = true } : new LoginResponse { Success = false, Message = error };
    }

    public static void LogOut() {
        LoggedIn = false;
        Username = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        Model = null;
        SaveAccountDetails();
    }
}