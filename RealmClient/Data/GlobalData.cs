using System;
using System.Collections.Generic;
using RealmClient.State;

namespace RealmClient.Data;

public interface IGlobalData;

public static class GlobalData {
    
    private sealed class DataComparer : IEqualityComparer<IGlobalData> {
        public bool Equals(IGlobalData x, IGlobalData y) => x is not null && y is not null && x.GetType() == y.GetType();
        public int GetHashCode(IGlobalData obj) => obj.GetType().GetHashCode();
    }

    private static readonly HashSet<IGlobalData> Data = new(new DataComparer());

    public static int SelectedCharacterId;
    
    public static ushort CharacterType;

    public static T Get<T>() where T : class, IGlobalData {
        foreach (var data in Data) {
            if (data.GetType() == typeof(T)) {
                return (T) data;
            }
        }
        
        return null;
    }

    public static void Add<T>(T data) where T : class, IGlobalData {
        Console.WriteLine($"{typeof(T).Name}, {data != null}");
        if (data == null)
            return;
        
        if (Data.Contains(data)) {
            Data.Remove(data);
        }

        Data.Add(data);
    }

    public static void Remove<T>() where T : class, IGlobalData {
        foreach (var data in Data) {
            if (data.GetType() == typeof(T)) {
                Data.Remove(data);
            }
        }
    }

    public static void Logout() {
        Data.Clear();
        Settings.SaveLocalAccount();
    }
}