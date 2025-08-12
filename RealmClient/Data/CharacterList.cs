using System.Collections.Generic;
using System.Xml;
using Common;
using System.Threading.Tasks;
using RealmClient.AppEngine;
using RealmClient.Data.XmlModels;
using RealmClient.Utils;

namespace RealmClient.Data;

public struct CharListResponse {
    public bool Success;
    public string Message;
}

public static class CharacterList {
    private static readonly Logger Log = new("CharList");

    public static CharacterListModel Model;
    public static BrewingDataModel BrewingData;

    public static async Task<CharListResponse> LoadAsync() {
        var data = new Dictionary<string, string> {
            { "guid", Account.Email },
            { "password", Account.Password }
        };
        var response = await AppEngineClient.SendRequest("/char/list", data, retries: 3);
        if (response == null) {
            return new CharListResponse {
                Success = false,
                Message = "Failed to load character list."
            };
        }

        if (response.Contains("Bad Login")) {
            return new CharListResponse {
                Success = false,
                Message = "Bad Login"
            };
        }
        
        //var doc = new XmlDocument();
        //.LoadXml(response);
        
        Model = XmlSerializer<CharacterListModel>.Deserialize(response);

        Log.Info("Character List Loaded.");
        
        return new CharListResponse {
            Success = true
        };
    }
}