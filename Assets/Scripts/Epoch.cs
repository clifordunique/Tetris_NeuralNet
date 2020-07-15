using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Epoch
{
    // Score
    private float _Score = 0;
    public void SetScore(float s)
    {
        _Score = s;
    }
    public float GetScore()
    {
        return _Score;
    }

    // Input Layer
    private List<Perceptron> _InputLayer = new List<Perceptron>();
    // Hidden Layers
    private List<List<Perceptron>> _HiddenLayers = new List<List<Perceptron>>();
    private int _HiddenLayerSize = 0;
    private int _FirstInputLayerSize = 0;
    // Output Layer
    private List<Perceptron> _OutputLayer = new List<Perceptron>();

    // CONSTRUCTOR
    public Epoch(int first_inputlayer_size, int hidden_layers, int hidden_layer_size, int output_layer_size)
    {
        // Sizes
        _FirstInputLayerSize = first_inputlayer_size;
        _HiddenLayerSize = hidden_layer_size;

        // Input Layer
        for(int i = 0; i < first_inputlayer_size; i++)
        {
            _InputLayer.Add(new Perceptron(1));
        }
        // Hidden Layers
        for (int a = 0; a < hidden_layers; a++)
        {
            List<Perceptron> new_layer = new List<Perceptron>();
            for (int b = 0; b < hidden_layer_size; b++)
            {
                // If first layer, weights are equal to board data
                if (a == 0)
                    new_layer.Add(new Perceptron(first_inputlayer_size));
                // else, equal to previous layer size
                else
                    new_layer.Add(new Perceptron(hidden_layer_size));
            }
            _HiddenLayers.Add(new_layer);
        }

        // Create Output Layer
        for (int i = 0; i < output_layer_size; i++)
        {
            // If no hidden layers, it connects directly to the input
            if (hidden_layers == 0)
                _OutputLayer.Add(new Perceptron(first_inputlayer_size));
            // Else, it is the size of previous layers
            else
                _OutputLayer.Add(new Perceptron(hidden_layer_size));
        }
    }

    
    
    // Set Layer Data
    public void SetInputLayerData(List<Perceptron> perceptrons)
    {
        _InputLayer = perceptrons;
    }
    public void SetHiddenLayerData(List<List<Perceptron>> all_perceptrons)
    {
        _HiddenLayers = all_perceptrons;
    }
    public void SetHiddenLayerData(int hiddenlayer_index, List<Perceptron> perceptrons)
    {
        _HiddenLayers[hiddenlayer_index] = perceptrons;
    }
    public void SetOutputLayerData(List<Perceptron> perceptrons)
    {
        _OutputLayer = perceptrons;
    }

    // Get Layer Data
    public List<Perceptron> GetInputLayer()
    {
        return _InputLayer;
    }
    public List<List<Perceptron>> GetHiddenLayers()
    {
        return _HiddenLayers;
    }
    public List<Perceptron> GetOutputLayers()
    {
        return _OutputLayer;
    }

    // Copy Layers
    public void CopyData(Epoch E)
    {
        // Check Sizes
        if(_InputLayer.Count != E._InputLayer.Count)
        {
            Debug.LogError("Cannot Copy Epoch, Size Mismatch (Input Layers)");
        }
        if(_HiddenLayers.Count != E._HiddenLayers.Count)
        {
            Debug.LogError("Cannot Copy Epoch, Size Mismatch (Hidden Layers)");
        }
        if (_OutputLayer.Count != E._OutputLayer.Count)
        {
            Debug.LogError("Cannot Copy Epoch, Size Mismatch (Output Layer)");
        }

        // Input Layers
        for(int i = 0; i < _InputLayer.Count; i++)
        {
            _InputLayer[i].Copy(E._InputLayer[i]);
        }

        // Hidden Layers
        for (int a = 0; a < _HiddenLayers.Count; a++)
        {
            for (int b = 0; b < _HiddenLayers[a].Count; b++)
            {
                if (_HiddenLayers[a].Count != E._HiddenLayers[a].Count)
                {
                    Debug.LogError("Cannot Copy Epoch, Size Mismatch (Hidden Layer Individual)");
                }

                _HiddenLayers[a][b].Copy(E._HiddenLayers[a][b]);
            }
        }
        // Output Layer
        for (int i = 0; i < _OutputLayer.Count; i++)
        {
            _OutputLayer[i].Copy(E._OutputLayer[i]);
        }
    }

    // Jitter All Layers
    public void MutateValues(float amount)
    {
        // Jitter Input Layer
        for (int i = 0; i < _InputLayer.Count; i++)
        {
            _InputLayer[i].MutateValues(amount);
        }
        // Jitter Hidden Layers
        for (int a = 0; a < _HiddenLayers.Count; a++)
        {
            for (int b = 0; b < _HiddenLayers[a].Count; b++)
            {
                _HiddenLayers[a][b].MutateValues(amount);
            }
        }
        // Jitter Output Layer
        for (int i = 0; i < _OutputLayer.Count; i++)
        {
            _OutputLayer[i].MutateValues(amount);
        }
    }

    private float[] EvaluateInputLayer(float[] InputArray, List<Perceptron> Perceptrons)
    {
        float[] Values = new float[Perceptrons.Count];
        for (int i = 0; i < Perceptrons.Count; i++)
        {
            Values[i] = Perceptrons[i].Evaluate(InputArray[i]);
        }
        return Values;
    }
    // Returns a list of all values for a layer of Perceptrons
    private float[] EvaluateLayer(float[] InputArray, List<Perceptron> Perceptrons)
    {
        float[] Values = new float[Perceptrons.Count];
        for(int i = 0; i < Perceptrons.Count; i++)
        {
            Values[i] = Perceptrons[i].Evaluate(InputArray);
        }
        return Values;
    }

    // Evaluate all layers and return list of all output values
    private float[] EvaluateOutput(float[] InputArray)
    {
        // Evaluate Input Layer
        float[] InputResults = EvaluateInputLayer(InputArray, _InputLayer);

        // If no hidden layers, only connect output with input
        if (_HiddenLayers.Count == 0)
        {
            return EvaluateLayer(InputResults, _OutputLayer);
        }
        // Else If hidden layers

        // Get all values from first layer
        float[] TempLayer = EvaluateLayer(InputResults, _HiddenLayers[0]);
        // Go through each layer
        for (int i = 1; i < _HiddenLayers.Count; i++)
        {
            TempLayer = EvaluateLayer(TempLayer, _HiddenLayers[i]);
        }

        // Output Layer at the end
        return EvaluateLayer(TempLayer, _OutputLayer);
    }

    // Get Biggest Number in Array
    private void GetArrayData(float[] Array, ref float BiggestNumber, ref int Index)
    {
        BiggestNumber = Array[0];
        Index = 0;

        for (int i = 1; i < Array.Length; i++)
        {
            if (Array[i] > BiggestNumber)
            {
                BiggestNumber = Array[i];
                Index = i;
            }
        }
    }

   
    public float[] CurrentOutputs = null;

    // Get Best Output
    public int GetBestOutput(float[] InputArray)
    {
        // Evaluate Outputs of entire Epoch into one array
        CurrentOutputs = EvaluateOutput(InputArray);
        if (CurrentOutputs == null)
            return 0;

        if(GameplayManager.GetInstance()._DebugDisplayOutputFloats)
        {
            Debug.Log("~~~");
            for (int i = 0; i < CurrentOutputs.Length; i++)
            {
                Debug.Log(CurrentOutputs[i]);
            }
        }

        // Get Best Value of output and return its index
        float BiggestValue = CurrentOutputs[0];
        int Index = 0;
        GetArrayData(CurrentOutputs, ref BiggestValue, ref Index);
        return Index;
    }

   
    // CrossOver
    public static Epoch CrossOver(Epoch a, Epoch b)
    {
        // Simple Size Checks
        if (a._HiddenLayers.Count != b._HiddenLayers.Count)
            return null;
        else if (a._OutputLayer.Count != b._OutputLayer.Count)
            return null;

        // Create new epoch based on existing one sizes
        Epoch E = new Epoch(a._FirstInputLayerSize, a._HiddenLayers.Count, a._HiddenLayerSize, a._OutputLayer.Count);

        float R = 0.5f;
        if(GameplayManager.GetInstance().CrossOverWeighthingIsBiased)
            R = UnityEngine.Random.Range(0.0f, 1.0f);

        // Set Perceptrons of Input
        for (int i = 0; i < E._InputLayer.Count; i++)
        {
            E._InputLayer[i] = Perceptron.CrossOver(a._InputLayer[i], b._InputLayer[i], R , 1.0f - R);
        }
        // Set Perceptrons of Output
        for (int i = 0; i < E._OutputLayer.Count; i++)
        {
            E._OutputLayer[i] = Perceptron.CrossOver(a._OutputLayer[i], b._OutputLayer[i], R, 1.0f - R);
        }
        // Set Perceptrons of all Hidden Layers
        for (int i = 0; i < E._HiddenLayers.Count; i++)
        {
            for (int j = 0; j < E._HiddenLayers[i].Count; j++)
            {
                E._HiddenLayers[i][j] = Perceptron.CrossOver(a._HiddenLayers[i][j], b._HiddenLayers[i][j], R, 1.0f - R);
            }
        }

        return E;
    }
}
