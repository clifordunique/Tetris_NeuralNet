using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public static class SimpleFileDialog
{
    // Opne 1 file as a string
    public static string OpenFile()
    {
        // Open File
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);

        if(paths.Length == 0)
            return "";

        StreamReader reader = new StreamReader(paths[0]);
        string Result = reader.ReadToEnd();
        reader.Close();

        return Result;
    }
    // Open 1 or more files as strings
    public static string[] OpenFiles()
    {
        // Open File
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel("Open File(s)", "", "", true);

        if (paths.Length == 0)
            return new string[0];

        string[] Result = new string[paths.Length];
        for(int i = 0; i < paths.Length; i++)
        {
            StreamReader reader = new StreamReader(paths[i]);
            Result[i] = reader.ReadToEnd();
            reader.Close();
        }
        return Result;
    }

    // Write to file
    public static void WriteFile(string Directory, string Text)
    {
        System.IO.File.WriteAllText(Directory, Text);
    }

    // Select a file and returns path
    public static string SelectFile()
    {
        // Save to File
        var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "");

        if (path.Length == 0)
            return "";

        return path;
    }

    //  // Select a file and overwrite a string to it (Returns path)
    //  public static string SaveFile(string data)
    //  {
    //      // Save to File
    //      var path = SFB.StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "");
    //  
    //      if (path.Length == 0)
    //          return "";
    //  
    //      System.IO.File.WriteAllText(path, data);
    //      return path;
    //  }
    //  
    //  public static void SaveEpochToFile(Epoch E)
    //  {
    //      // Convert Epoch into text
    //      string _epochData = EpochParser.SaveFile(E);
    //      if (_epochData.Length == 0)
    //      {
    //          Debug.LogError("Unable to convert Epoch into a String");
    //          return;
    //      }
    //  
    //      // Save to file
    //      SimpleFileDialog.SaveFile(_epochData);
    //  }
}
