using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AlloyClient.State;
using AlloyClient.Data;
using Microsoft.Extensions.Logging;

namespace AlloyClient.AppEngine;

public static class AppEngineClient {
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(AppEngineClient));

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
            Logger.Log(LogLevel.Error, $"Aborting {endpoint} early. AppEngine failure!");
            return null;
        }
        
        var cancellationTokenSource = new CancellationTokenSource(Settings.AppEngineTimeout);
        var content = data == null ? null : new FormUrlEncodedContent(data);

        for (var i = 1; i <= retries + 1; i++) {
            Logger.Log(LogLevel.Trace, $"Sending request to '{endpoint}'. Attempt {i} of {retries + 1}.");

            try {
                var response = await client.PostAsync(endpoint, content, cancellationTokenSource.Token);
                var text =  await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
                Logger.Log(LogLevel.Trace, $"Request '{endpoint}' succeeded on attempt {i} of {retries + 1}.");
                return text;
            } catch (HttpRequestException) {
                Logger.Log(LogLevel.Error, $"Attempt {i} of {retries} for {endpoint} failed. Server offline!");
                GlobalData.Add(new AppRequestFailedFlag("Server offline!"));
                return null;
            } catch (OperationCanceledException) {
                Logger.Log(LogLevel. Warning, $"Request '{endpoint}' attempt {i} timed out.");
            } catch (Exception e) {
                Logger.Log(LogLevel.Error, $"Request '{endpoint}' attempt {i + 1} failed: {e}");
                GlobalData.Add(new AppRequestFailedFlag("Unknown Exception"));
                return null;
            }
        }
        
        Logger.Log(LogLevel.Error, $"Request '{endpoint}', All attempts failed.");
        GlobalData.Add(new AppRequestFailedFlag("Server timed out!"));
        return null;
    }
}