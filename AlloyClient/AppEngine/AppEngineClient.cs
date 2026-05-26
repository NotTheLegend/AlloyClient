using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AlloyClient.State;
using AlloyClient.Utils;
using Alloy.Common;
using AlloyClient.Data;

namespace AlloyClient.AppEngine;

public static class AppEngineClient {
    private static readonly Logger Log = new("AppEngine");

    private static readonly HttpClient Client;

    static AppEngineClient() {
        Client = new HttpClient();
        Client.BaseAddress = new Uri(Settings.AppEngineUrl);
    }

    public static async Task<string> SendRequest(string endpoint, Dictionary<string, string> data = null, uint retries = 0) {
        return await SendClientRequest(Client, endpoint, data, retries);
    }

    private static async Task<string> SendClientRequest(HttpClient client, string endpoint, Dictionary<string, string> data = null, uint retries = 0) {
        if (GlobalData.Contains<AppRequestFailedFlag>()) {
            Log.Error($"Aborting {endpoint} early. AppEngine failure!");
            return null;
        }
        
        var cancellationTokenSource = new CancellationTokenSource(Settings.AppEngineTimeout);
        var content = data == null ? null : new FormUrlEncodedContent(data);

        for (var i = 1; i <= retries + 1; i++) {
            Log.Trace($"Sending request to {endpoint}. Attempt {i} of {retries + 1}.");

            try {
                var response = await client.PostAsync(endpoint, content, cancellationTokenSource.Token);
                return await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            } catch (HttpRequestException) {
                Log.Error($"Attempt {i} of {retries} for {endpoint} failed. Server offline!");
                GlobalData.Add(new AppRequestFailedFlag("Server offline!"));
                return null;
            } catch (OperationCanceledException) {
                Log.Warn($"Attempt {i} timed out.");
            } catch (Exception e) {
                Log.Error($"Attempt {i + 1} failed: {e}");
                GlobalData.Add(new AppRequestFailedFlag("Unknown Exception"));
                return null;
            }
        }
        
        Log.Error("All attempts failed.");
        GlobalData.Add(new AppRequestFailedFlag("Server timed out!"));
        return null;
    }
}