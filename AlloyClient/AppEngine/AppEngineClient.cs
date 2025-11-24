using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AlloyClient.State;
using AlloyClient.Utils;

namespace AlloyClient.AppEngine;

public static class AppEngineClient {
    private static readonly Logger Log = new("AppEngine");

    private static readonly HttpClient Client;

    static AppEngineClient() {
        Client = new HttpClient();
        Client.BaseAddress = new Uri(Settings.AppEngineUrl);

        Client.DefaultRequestHeaders.Add("Pragma", "no-cache");
        Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows; U; en-US) AppleWebKit/533.19.4 (KHTML, like Gecko) AdobeAIR/50.0");
        Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
        Client.DefaultRequestHeaders.Add("Referer", "app:/Main.dll");
        Client.DefaultRequestHeaders.Add("x-flash-version", "50,0,1,2");
    }

    public static async Task<string> SendRequest(string endpoint, Dictionary<string, string> data = null, int retries = 0) {
        return await SendClientRequest(Client, endpoint, data, retries);
    }

    private static async Task<string> SendClientRequest(HttpClient client, string endpoint, Dictionary<string, string> data = null, int retries = 0) {
        var cancellationTokenSource = new CancellationTokenSource(Settings.AppEngineTimeout);
        var content = data == null ? null : new FormUrlEncodedContent(data);

        for (var i = 0; i <= retries; i++) {
            Log.Trace($"Sending request to {endpoint}." + (i == 0 ? "" : $" Attempt {i + 1} of {retries + 1}."));

            try {
                var response = await client.PostAsync(endpoint, content, cancellationTokenSource.Token);
                return await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            } catch (Exception e) {
                Log.Error($"Attempt {i + 1} failed: {e}");

                if (i != retries) {
                    continue;
                }

                Log.Error("All attempts failed.");

                return null;
            }
        }

        return null;
    }
}