namespace AlloyClient.Data;

public class AppRequestFailedFlag(string message) : IGlobalData {
    public readonly string Message = message;
}