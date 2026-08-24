using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AlloyClient.Data;

public interface IGlobalData;

public static class GlobalData {

    private static readonly ConcurrentDictionary<Type, IGlobalData> DataStorage = [];

    public static int SelectedCharacterId;
    
    public static ushort CharacterType;

    public static ushort CharacterSkin;

    public static T Get<T>() where T : class, IGlobalData {
        if (DataStorage.TryGetValue(typeof(T), out var data)) {
            return (T)data;
        }

        return null;
    }

    public static bool TryGet<T>(out T data) where T : class, IGlobalData {
        if (DataStorage.TryGetValue(typeof(T), out var data1)) {
            data = (T) data1;
            return true;
        }

        data = null;
        return false;
    }

    public static bool TryRemove<T>(out T data) where T : class, IGlobalData {
        if (DataStorage.Remove(typeof(T), out var data1)) {
            data = (T) data1;
            return true;
        }

        data = null;
        return false;
    }

    public static bool Contains<T>() where T : class, IGlobalData {
        return DataStorage.ContainsKey(typeof(T));
    }

    public static void Add<T>(T data) where T : class, IGlobalData {
        DataStorage[typeof(T)] = data;
    }

    public static void Remove<T>() where T : class, IGlobalData {
        DataStorage.Remove(typeof(T), out _);
    }

    public static void Logout() {
        DataStorage.Clear();
        Settings.SaveLocalAccount();
    }
}
