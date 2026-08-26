using System;
using System.Collections.Generic;
using NativeFileDialogs.Net;

namespace AlloyClient.Editor;

internal static class NativeFileDialog {
    private readonly static Dictionary<string, string> MapFilters = new() {
        ["JSON Map"] = "jm",
        ["World Map"] = "wmap",
        ["All Map Files"] = "jm,wmap",
    };

    public static string OpenMap() {
        return Nfd.OpenDialog(out var path, MapFilters) == NfdStatus.Ok ? path : null;
    }

    public static string SaveMap(string suggestedName, bool wmap) {
        var extension = wmap ? "wmap" : "jm";
        var fileName = suggestedName.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase)
            ? suggestedName
            : $"{suggestedName}.{extension}";
        var filters = new Dictionary<string, string> {
            [wmap ? "World Map" : "JSON Map"] = extension,
        };
        return Nfd.SaveDialog(out var path, filters, fileName) == NfdStatus.Ok ? path : null;
    }
}
