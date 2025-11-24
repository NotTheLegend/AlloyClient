namespace AlloyClient.Data;

public sealed class LoginData(string username, string password) : IGlobalData {

    public static readonly LoginData Default = new ("", "");
    
    public readonly string Username = username;
    public readonly string Password = password;
    
}