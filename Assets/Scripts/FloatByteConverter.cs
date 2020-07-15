using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class FloatByteConverter
{
    public static string FloatToStringOfBytes(float f)
    {
        byte[] bytes = FloatToBytes(f);
        return ByteToCharString(bytes);
    }

    public static char[] ByteToChars(byte[] bytes)
    {
        List<char> chars = new List<char>();
        foreach (byte number in bytes)
        {
            chars.Add(Convert.ToChar(number));
        }
        return chars.ToArray();
    }
    public static string ByteToCharString(byte[] bytes)
    {
        string chars = "";
        foreach (byte number in bytes)
        {
            chars += (Convert.ToChar(number));
        }
        return chars;
    }
    public static byte[] FloatToBytes(float f)
    {
        return BitConverter.GetBytes(f);
    }

    public static float CharStringOfBytesToFloat(string str)
    {
        byte[] b = CharStringToBytes(str);
        return BytesToFloat(b);
    }
    public static byte[] CharStringToBytes(string str)
    {
        byte[] bytes = new byte[str.Length];
        for(int i = 0; i < str.Length; i++)
        {
            bytes[i] = Convert.ToByte(str[i]);
        }
        return bytes;
    }
    public static float BytesToFloat(byte[] b)
    {
        return BitConverter.ToSingle(b, 0);
    }

    
}
