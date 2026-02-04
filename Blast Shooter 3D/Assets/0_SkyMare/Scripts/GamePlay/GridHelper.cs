using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridParse
{
    public static List<int> OnSplitListIDs(string input)
    {
        string[] parts = input.Split('^');

        List<int> result = new List<int>();

        for (int i = 0; i < parts.Length; i++)
        {
            result.Add(int.Parse(parts[i]));
        }

        return result;
    }
    public static (int colorID, int sandAmount) OnSplitID(string input)
    {
        string[] parts = input.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[0], out int colorID) && int.TryParse(parts[1], out int sandAmount))
        {
            return (colorID, sandAmount);
        }

        return (-1, -1);
    }
    public static List<(int colorID, int sandAmount)> OnSplitPipe(string input)
    {
        string[] pairs = input.Split('+');

        List<(int colorID, int sandAmount)> result = new List<(int colorID, int sandAmount)>();

        foreach (var pair in pairs)
        {
            result.Add(OnSplitID(pair));
        }

        return result;
    }
    public static List<string> OnSplitPipeIDs(string input)
    {
        string[] pairs = input.Split('+');


        return pairs.ToList();
    }

    // input dạng "a_b" -> (be, af). Không crash nếu thiếu "_"
    public static void OnSplitBeAf(string input, out string be, out string af)
    {
        int idx = input.IndexOf('_');
        if (idx < 0) { be = input; af = ""; return; }
        be = input.Substring(0, idx);
        af = input[(idx + 1)..];
    }

    // Lấy số sau prefix 1 ký tự, ví dụ "k3" hoặc "l12" -> 3 hoặc 12
    public static int IdAfter1Char(string s) => int.Parse(s.Substring(1));

    // Ký tự đầu là type (giữ logic cũ)
    public static int TypeOf(string s) => int.Parse(s[0].ToString());
}
/// <summary>
/// Gom logic nối key–lock vào 1 chỗ, dùng Dict để tra O(1)
/// </summary>
class KeyLockRegistry
{
    readonly Dictionary<int, KeyTile> _keys = new();
    readonly Dictionary<int, LockTile> _locks = new();

    public void RegisterKey(KeyTile k)
    {
        _keys[k.keyID] = k;
        if (_locks.TryGetValue(k.keyID, out var lk))
        {
            k.target = lk;
            lk.target = k;
        }
    }

    public void RegisterLock(LockTile l)
    {
        _locks[l.lockID] = l;
        if (_keys.TryGetValue(l.lockID, out var ke))
        {
            l.target = ke;
            ke.target = l;
        }
    }
}
class ConnectRegistry
{
    readonly List<ConnectTile> tiles = new();

    public bool RegisterConnect(ConnectTile k)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].ID == k.ID)
            {
                tiles[i].target = k;
                k.target = tiles[i];
                tiles.RemoveAt(i);
                return true;
            }
        }
        tiles.Add(k);
        return false;
    }
}