using System;
using System.Collections.Generic;
using System.Text;
using AlloyClient.Diagnostics;

namespace AlloyClient.Game.Components.Hud.Chat;

public delegate void ClientCommandHandler(ClientCommandContext command);

public sealed class ClientCommandContext {

    public readonly string Name;
    public readonly string RawText;
    public readonly string[] Arguments;

    public string Response = string.Empty;

    public ClientCommandContext(string name, string rawText, string[] arguments) {
        Name = name;
        RawText = rawText;
        Arguments = arguments;
    }

    public void Reply(string response) {
        Response = response;
    }
}

public static class ClientCommands {

    private readonly static Dictionary<string, ClientCommandHandler> Handlers = new(StringComparer.OrdinalIgnoreCase);

    static ClientCommands() {
        Register("renderstress", RenderStress.HandleCommand);
    }

    public static void Register(string name, ClientCommandHandler handler) {
        name = NormalizeName(name);
        if (!Handlers.TryAdd(name, handler)) {
            throw new InvalidOperationException($"Client command '{name}' is already registered.");
        }
    }

    public static bool Remove(string name, ClientCommandHandler handler) {
        name = NormalizeName(name);
        if (!Handlers.TryGetValue(name, out var registeredHandler) || registeredHandler != handler) {
            return false;
        }

        return Handlers.Remove(name);
    }

    public static bool TryDispatch(string text, out string response) {
        response = string.Empty;
        if (string.IsNullOrWhiteSpace(text) || text[0] != '/') {
            return false;
        }

        var nameEnd = 1;
        while (nameEnd < text.Length && !char.IsWhiteSpace(text[nameEnd])) {
            nameEnd++;
        }

        if (nameEnd == 1) {
            return false;
        }

        var name = text[1..nameEnd];
        if (!Handlers.TryGetValue(name, out var handler)) {
            return false;
        }

        var arguments = ParseArguments(text, nameEnd);
        var command = new ClientCommandContext(name, text, arguments);
        handler(command);
        response = command.Response;
        return true;
    }

    private static string NormalizeName(string name) {
        name = name.Trim();
        if (name.StartsWith('/')) {
            name = name[1..];
        }

        if (name.Length == 0 || name.Contains(' ')) {
            throw new ArgumentException("Client command names cannot be empty or contain spaces.", nameof(name));
        }

        return name;
    }

    private static string[] ParseArguments(string text, int startIndex) {
        var arguments = new List<string>();
        var argument = new StringBuilder();
        var quoted = false;
        var hasValue = false;

        for (var i = startIndex; i < text.Length; i++) {
            var value = text[i];
            if (value == '"') {
                quoted = !quoted;
                hasValue = true;
                continue;
            }

            if (value == '\\' && i + 1 < text.Length && text[i + 1] is '"' or '\\') {
                argument.Append(text[++i]);
                hasValue = true;
                continue;
            }

            if (char.IsWhiteSpace(value) && !quoted) {
                if (hasValue) {
                    arguments.Add(argument.ToString());
                    argument.Clear();
                    hasValue = false;
                }

                continue;
            }

            argument.Append(value);
            hasValue = true;
        }

        if (hasValue) {
            arguments.Add(argument.ToString());
        }

        return arguments.ToArray();
    }
}