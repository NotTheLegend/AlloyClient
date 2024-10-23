using System.Collections.Generic;
using System.Threading.Tasks;
using MonoClient.AppEngine;

namespace MonoClient.Data;

public static class AppInit {
    public static async Task LoadAsync() {
        var response = await AppEngineClient.SendRequest("/app/init", new Dictionary<string, string> {
            { "guid", Account.Email },
            { "password", Account.Password }
        }, retries: 3);
        if (response == null) {
            // TODO: Queue error message
            return;
        }
        
        // TODO: Load potion inventory model, forge recipes, etc.
        
    }
}