using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.AppEngine;

public static class AppEngineClient {
    private static readonly Logger Log = new("AppEngine");

    private static readonly HttpClient Client;
    private static readonly HttpClient VerifyClient;

    static AppEngineClient() {
        Client = new HttpClient();
        Client.BaseAddress = new Uri(Settings.AppEngineUrl);

        VerifyClient = new HttpClient();
        VerifyClient.BaseAddress = new Uri(Settings.AppEngineVerifyUrl);

        Client.DefaultRequestHeaders.Add("Pragma", "no-cache");
        Client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows; U; en-US) AppleWebKit/533.19.4 (KHTML, like Gecko) AdobeAIR/50.0");
        Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
        Client.DefaultRequestHeaders.Add("Referer", "app:/Main.dll");
        Client.DefaultRequestHeaders.Add("x-flash-version", "50,0,1,2");


        VerifyClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
        VerifyClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows; U; en-US) AppleWebKit/533.19.4 (KHTML, like Gecko) AdobeAIR/50.0");
        VerifyClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
        VerifyClient.DefaultRequestHeaders.Add("Referer", "app:/Main.dll");
        VerifyClient.DefaultRequestHeaders.Add("x-flash-version", "50,0,1,2");
    }

    public static async Task<string> SendRequest(string endpoint, Dictionary<string, string> data = null,
        int retries = 0) {
        return await SendClientRequest(Client, endpoint, data, retries);
    }

    public static async Task<string> SendVerifyRequest(string endpoint, Dictionary<string, string> data = null,
        int retries = 0) {
        return await SendClientRequest(VerifyClient, endpoint, data, retries);
    }

    private static async Task<string> SendClientRequest(HttpClient client, string endpoint,
        Dictionary<string, string> data = null, int retries = 0) {
        var cancellationTokenSource = new CancellationTokenSource(Settings.AppEngineTimeout);
        var content = data == null ? null : new FormUrlEncodedContent(data);

        for (var i = 0; i <= retries; i++) {
            Log.Trace($"Sending request to {endpoint}." + (i == 0 ? "" : $" Attempt {i + 1} of {retries + 1}."));

            try {
                var response = await client.PostAsync(endpoint, content, cancellationTokenSource.Token);
                return await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            }
            catch (Exception e) {
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