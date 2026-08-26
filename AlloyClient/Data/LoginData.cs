namespace AlloyClient.Data;

public sealed class LoginData(string username, string password) : IGlobalData {

    public readonly static LoginData Default = new ("", "");
    
    public readonly string Username = username;
    public readonly string Password = password;
    
}