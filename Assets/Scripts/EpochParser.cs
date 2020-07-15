using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
//#if UNITY_EDITOR
//    using UnityEditor;
//#endif

public class StringMaker
{
    private string str = "";

    public void clear()
    {
        str = "";
    }
    public void Add(char s)
    {
        str += s;
    }
    public void Add(string s)
    {
        str += s;
    }
    public void AddLine(string s)
    {
        str += s + '\n';
    }
    public void AddNewLine()
    {
        str += '\n';
    }
    public string GetString()
    {
        return str;
    }
}


static public class EpochParser
{
    public static string SaveFile(Epoch E)
    {
        int err = 0;

        StringMaker str = new StringMaker();
        // All things that need to be saved to a file
        List<Perceptron> input_layer = E.GetInputLayer();
        List<List<Perceptron>> hidden_layers = E.GetHiddenLayers();
        List<Perceptron> output_layers = E.GetOutputLayers();

        // Then with total hidden layer count
        str.Add(FloatByteConverter.FloatToStringOfBytes((float)hidden_layers.Count));
        // Then hidden layer size
        if(hidden_layers.Count == 0)
            str.Add(FloatByteConverter.FloatToStringOfBytes(0f));
        else
            str.Add(FloatByteConverter.FloatToStringOfBytes((float)hidden_layers[0].Count));

        // Then hidden layer weight count
        for (int i = 0; i < hidden_layers.Count; i++)
        {
            for (int j = 0; j < hidden_layers[i].Count; j++)
            {
                float values_layer = hidden_layers[i][j].GetWeights().Length;
                str.Add(FloatByteConverter.FloatToStringOfBytes(values_layer));
            }
        }
        
        // Then output layer size
        str.Add(FloatByteConverter.FloatToStringOfBytes((float)output_layers.Count));
        // Then output weight count
        float values = output_layers[0].GetWeights().Length;
        str.Add(FloatByteConverter.FloatToStringOfBytes(values));

        // Add all bias and weights of each hidden layers perceptrons
        if (hidden_layers.Count > 0)
        {
            for (int i = 0; i < hidden_layers.Count; i++)
            {
                for (int j = 0; j < hidden_layers[i].Count; j++)
                {
                    float[] weights = hidden_layers[i][j].GetWeights();
                    float bias = hidden_layers[i][j].GetBias();

                    //BIAS
                    str.Add(FloatByteConverter.FloatToStringOfBytes(bias));
                    //WEIGHTS
                    for (int c = 0; c < weights.Length; c++)
                    {
                        str.Add(FloatByteConverter.FloatToStringOfBytes(weights[c]));

                        SavingThread.m_Saving_Layer = (hidden_layers.Count + 1) - i;
                        SavingThread.m_Saving_Perceptron = (hidden_layers[i].Count) - j;
                        SavingThread.m_Saving_Weight = (weights.Length) - c;
                    }
                }
            }
        }

        // Add all bias and weights of each output layer perceptron
        for (int i = 0; i < output_layers.Count; i++)
        {
            // Save Me
            float[] weights = output_layers[i].GetWeights();
            float bias = output_layers[i].GetBias();
        
            //BIAS
            str.Add(FloatByteConverter.FloatToStringOfBytes(bias));
            //WEIGHTS
            for (int c = 0; c < weights.Length; c++)
            {
                str.Add(FloatByteConverter.FloatToStringOfBytes(weights[c]));

                SavingThread.m_Saving_Layer = 1;
                SavingThread.m_Saving_Perceptron = (output_layers.Count) - i;
                SavingThread.m_Saving_Weight = (weights.Length) - c;
            }
        }

        // Add all bias and weights of each input layer perceptron
        for (int i = 0; i < input_layer.Count; i++)
        {
            // Save Me
            float weight = input_layer[i].GetWeight(0);
            float bias = input_layer[i].GetBias();

            //BIAS
            str.Add(FloatByteConverter.FloatToStringOfBytes(bias));
            //WEIGHT
            str.Add(FloatByteConverter.FloatToStringOfBytes(weight));

            SavingThread.m_Saving_Layer = 0;
            SavingThread.m_Saving_Perceptron = (input_layer.Count) - i;
            SavingThread.m_Saving_Weight = (input_layer.Count) - i;

        }

        return str.GetString();
    }

    public static Epoch LoadFile(string Data)
    {
        int _ExpectedLength = 8;

        // Expect 2 variables
        if (Data.Length < _ExpectedLength)
        {
            Debug.LogError("File too small (a)");
            return null;
        }

        // File index
        int FileIndex = 0;

        // Important Data from start of file
        int hidden_layers = (int)FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
        FileIndex += 4;
        Debug.Log("hidden layers: " + hidden_layers);

        int hidden_layers_size = (int)FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
        FileIndex += 4;
        Debug.Log("hidden layer sizes: " + hidden_layers_size);

        // Expect (hidden_layers * hidden_layers_size * 4)
        // Expect 2 more variables (output size + output weight count)
        _ExpectedLength += (hidden_layers * hidden_layers_size * 4) + 8;
        if (Data.Length < _ExpectedLength)
        {
            Debug.LogError("File too small (b)");
            return null;
        }

        List<int> hidden_layer_weight_count = new List<int>();
        for(int i = 0; i < hidden_layers; i++)
        {
            for (int j = 0; j < hidden_layers_size; j++)
            {
                hidden_layer_weight_count.Add((int)FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4)));
                FileIndex += 4;
                //Debug.Log("Hidden Layer: " + (i + 1) + ", Perceptron: " + (j + 1) + ", Size: " + hidden_layer_weight_count[j]);
            }
        }

        int output_layer_size = (int)FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
        FileIndex += 4;
        Debug.Log("output layer size: " + output_layer_size);

        int output_layer_weightcount = (int)FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
        FileIndex += 4;
        Debug.Log("Output Layer, Perceptron Size: " + output_layer_weightcount);

        Debug.Log("~~~");

        // Check if file is correct size

        // Expect
        for (int i = 0; i < hidden_layers; i++)
        {
            for (int j = 0; j < hidden_layers_size; j++)
            {
                _ExpectedLength += 4;
                _ExpectedLength += hidden_layer_weight_count[j] * 4;
            }
        }
        for (int i = 0; i < output_layer_size; i++)
        {
            _ExpectedLength += 4;
            _ExpectedLength += output_layer_weightcount * 4;
        }
        for (int i = 0; i < TetrisBoard.GetDataSize(); i++)
        {
            _ExpectedLength += 8;
        }
        if (Data.Length < _ExpectedLength)
        {
//#if UNITY_EDITOR
//            string Message = "File Too Small\n";
//            Message += "File Count: " + Data.Length.ToString() + "\n";
//            Message += "Expected Count: " + _ExpectedLength.ToString() + "\n";
//            EditorUtility.DisplayDialog("File Size", Message, "Continue");
//#endif
            return null;
        }
        else if (Data.Length > _ExpectedLength)
        {
//#if UNITY_EDITOR
//            string Message = "File Too Big\n";
//            Message += "File Count: " + Data.Length.ToString() + "\n";
//            Message += "Expected Count: " + _ExpectedLength.ToString() + "\n";
//            EditorUtility.DisplayDialog("File Size", Message, "Continue");
//#endif
            return null;
        }
//#if UNITY_EDITOR
//        else
//        {
//            EditorUtility.DisplayDialog("File Size", "Correct Size", "Continue");
//        }
//#endif


        // Read Hidden Layers
        List<List<Perceptron>> All_HiddenLayer = new List<List<Perceptron>>();
        for (int i = 0; i < hidden_layers; i++)
        {
            List<Perceptron> HiddenLayer = new List<Perceptron>();
            for (int j = 0; j < hidden_layers_size; j++)
            {
                // Get Bias
                float bias = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
                FileIndex += 4;
                //Debug.Log("Layer: " + (i + 1) + ", Perceptron: " + (j + 1) + ", Bias: " + bias);
                // Get all weights
                float[] weights = new float[hidden_layer_weight_count[j]];
                for(int k = 0; k < hidden_layer_weight_count[j]; k++)
                {
                    weights[k] = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
                    FileIndex += 4;
                    //Debug.Log("Layer: " + (i + 1) + ", Perceptron: " + (j + 1) + ", Weight[" + (k) + "]: " + weights[k]);
                }
                Perceptron P = new Perceptron(weights, bias);
                HiddenLayer.Add(P);
            }
            All_HiddenLayer.Add(HiddenLayer);
        }

        // Read Output Layer
        List<Perceptron> OutputLayer = new List<Perceptron>();
        for (int i = 0; i < output_layer_size; i++)
        {
            // Get Bias
            float bias = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
            FileIndex += 4;
            //Debug.Log("Output Perceptron: " + (i + 1) + ", Bias: " + bias);
            // Get all weights
            float[] weights = new float[output_layer_weightcount];
            for (int k = 0; k < output_layer_weightcount; k++)
            {
                weights[k] = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
                FileIndex += 4;
                //Debug.Log("Output Perceptron: " + (i + 1) + ", Weight[" + (k) + "]: " + weights[k]);
            }
            Perceptron P = new Perceptron(weights, bias);
            OutputLayer.Add(P);
        }

        // Read Input Layer
        List<Perceptron> InputLayer = new List<Perceptron>();
        for (int i = 0; i < TetrisBoard.GetDataSize(); i++)
        {
            // Get Bias
            float bias = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
            FileIndex += 4;
            //Debug.Log("Output Perceptron: " + (i + 1) + ", Bias: " + bias);
            // Get all weights
            float[] weights = new float[1];
            weights[0] = FloatByteConverter.CharStringOfBytesToFloat(Data.Substring(FileIndex, 4));
            FileIndex += 4;

            Perceptron P = new Perceptron(weights, bias);
            InputLayer.Add(P);
        }

        // Created Epoch
        Epoch NewEpoch = new Epoch(TetrisBoard.GetDataSize(), hidden_layers, hidden_layers_size, (int)Tetris.Move.TOTAL);

        NewEpoch.SetHiddenLayerData(All_HiddenLayer);
        NewEpoch.SetOutputLayerData(OutputLayer);
        NewEpoch.SetInputLayerData(InputLayer);

        return NewEpoch;
    }

    //  static void WriteString(List<String> str)
    //  {
    //      //if (File.Exists(fileName))
    //      //{
    //      //    //Debug.Log(fileName + " already exists.");
    //      //    return;
    //      //}
    //      var sr = File.CreateText(fileName);
    //      for(int c = 0; c < str.Count; ++c)
    //          sr.WriteLine(str[c]);
    //      sr.Close();
    //  }
    //  
    //  static List<String> ReadString()
    //  {
    //      List<String> str = new List<String>();
    //  
    //      //Read the text from directly from the test.txt file
    //      StreamReader reader = new StreamReader(fileName);
    //      str.Add(reader.ReadLine());
    //      //Debug.Log(reader.ReadToEnd());
    //      reader.Close();
    //      return str;
    //  }
}

