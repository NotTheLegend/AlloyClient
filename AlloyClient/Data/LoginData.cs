using System;

namespace AlloyClient.Data;

public sealed class LoginData(string username, string password, bool loggedIn = true) : IGlobalData, IEquatable<LoginData> {

    public static readonly LoginData Default = new ("", "", false);
    
    public readonly string Username = username;
    public readonly string Password = password;
    public readonly bool LoggedIn = loggedIn;

    public bool Equals(LoginData other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Username == other.Username && Password == other.Password && LoggedIn == other.LoggedIn;
    }

    public override bool Equals(object obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoginData) obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Username, Password, LoggedIn);
    }

    public static bool operator ==(LoginData left, LoginData right) {
        if (left is null && right is not null)
            return false;
        if (left is not null && right is null)
            return false;
        if (ReferenceEquals(left, right))
            return true;
        return left.Equals(right);
    }

    public static bool operator !=(LoginData left, LoginData right) => !(left == right);
}