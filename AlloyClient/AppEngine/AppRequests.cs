using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using AlloyClient.Data;
using AlloyClient.State;

namespace AlloyClient.AppEngine;

public struct AppResponse {
    public bool Success;
    public string Message;
}

public static class AppRequests {
    
    private const string UsernameKey = "username";
    private const string PasswordKey = "password";

    public static async Task Startup() {
        await VerifyAsync();
        await GetCharList();
    }
    
    public static async Task<AppResponse> VerifyAsync() {
        var login = GlobalData.Get<LoginData>() ?? LoginData.Default;
        return await VerifyAsync(login.Username, login.Password);
    }

    public static async Task<AppResponse> VerifyAsync(string username, string password, bool saveInfo = false) {
        var response = await AppEngineClient.SendRequest("/account/verify", BuildAccountRequestData(username, password), retries: 3);
        
        if (response == null) {
            return new AppResponse { Success = false, Message = "Failed to contact server." };
        }

        if (response.Contains("Invalid")) {
            return new AppResponse { Success = false, Message = "Invalid account credentials." };
        }

        var xml = XElement.Parse(response);
        GlobalData.Add(new AccountData(xml));
        GlobalData.Add(new LoginData(username, password));
        
        if (saveInfo) {
            Settings.SaveLocalAccount();
        }

        return new AppResponse { Success = true };
    }
    
    public static async Task<AppResponse> Register(string username, string password) {
        var data = new Dictionary<string, string> {{"newUsername", username}, {"newPassword", password}};
        var request = await AppEngineClient.SendRequest("/account/register", data, retries: 3);
        
        if (request == null) {
            return new AppResponse { Success = false, Message = "Failed to contact server." };
        }

        var result = XElement.Parse(request).Value;

        if (result != string.Empty) {
            return new AppResponse { Success = false, Message = result };
        }

        return await VerifyAsync(username, password, true);
    }
    
    public static async Task<AppResponse> PurchaseCharSlot() {
        var login = GlobalData.Get<LoginData>();

        if (login is null) {
            return new AppResponse { Success = false, Message = "Not logged in" };
        }
        var response = await AppEngineClient.SendRequest("/account/purchaseCharSlot", BuildAccountRequestData(login.Username, login.Password), retries: 3);
        
        if (response == null) {
            return new AppResponse { Success = false, Message = "Failed to contact server." };
        }

        var result = XElement.Parse(response).Value;
        
        return result == string.Empty ? new AppResponse { Success = true } : new AppResponse { Success = false, Message = result };
    }
    
    // Delete
    
    //Fame

    public static async Task<AppResponse> GetCharList() {
        var login = GlobalData.Get<LoginData>() ?? LoginData.Default;
        var response = await AppEngineClient.SendRequest("/char/list", BuildAccountRequestData(login.Username, login.Password), retries: 3);
        
        if (response == null) {
            return new AppResponse { Success = false, Message = "Failed to contact server." };
        }
        
        var xml = XElement.Parse(response);
        
        GlobalData.Add(new AccountData(xml.Element("Account")));
        GlobalData.Add(new CharacterListData(xml));
        GlobalData.Add(new NewsData(xml.Elements("NewsItem")));
        GlobalData.Add(new ServerListData(xml.Element("Servers")));
        
        return new AppResponse{ Success = true };
    }
    
    private static Dictionary<string, string> BuildAccountRequestData(string username, string password) => new() {{UsernameKey, username}, {PasswordKey, password}};
}