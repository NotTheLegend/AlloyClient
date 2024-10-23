using System.Collections.Generic;
using System.Xml;
using Common;
using MonoClient.Data.XmlModels;
using MonoClient.AppEngine;
using MonoClient.Utils;
using System.Threading.Tasks;

namespace MonoClient.Data;

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
        
        var doc = new XmlDocument();
        doc.LoadXml(response);
        
        var charList = doc.SelectSingleNode("/List/Chars");
        if (charList == null) {
            Log.Error("Character List not found.");
            return new CharListResponse {
                Success = false,
                Message = "Character List not found."
            };
        }
        
        Model = XmlSerializer<CharacterListModel>.Deserialize(charList.OuterXml);
        
        var brewingData = doc.SelectSingleNode("/List/BrewingData");
        if (brewingData == null) {
            Log.Error("Brewing Data not found.");
            return new CharListResponse {
                Success = false,
                Message = "Brewing Data not found."
            };
        }
        
        BrewingData = XmlSerializer<BrewingDataModel>.Deserialize(brewingData.OuterXml);

        Log.Info("Character List Loaded.");
        
        return new CharListResponse {
            Success = true
        };
    }
}