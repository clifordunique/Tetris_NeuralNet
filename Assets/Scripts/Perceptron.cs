using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Perceptron
{
    // General Weight Limits
    public static readonly float e = 2.71828182845904523536f;
    public static readonly float _MinimumValues = -1.0f;
    public static readonly float _MaximumValues = +1.0f;
    public static float _WeightScale = 0.2f;
    public static float _BiasScale = 3.0f;

    // Data
    private float[] Weights;
    private float bias = 0.0f;

    // Utility
    public static float Sigmoid(float val)
    {
        //Debug.Log("S: " + 1.0f + Mathf.Pow(e, -val));
        //Debug.Log("1/S: " + 1.0f / (1.0f + Mathf.Pow(e, -val)));

        //return (1.0f / (1.0f + Mathf.Pow(e, -val)));
        return (2.0f / (1.0f + Mathf.Pow(e, -val))) - 1.0f;
    }

    // Constructors
    public Perceptron(int totalWeights)
    {
        Weights = new float[totalWeights];
        RandomizeValues();
    }
    public Perceptron(float[] _Weights, float _bias)
    {
        Weights = _Weights;
        bias = _bias;
    }

    // Mess With Values
    public void RandomizeValues()
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = UnityEngine.Random.Range(_MinimumValues * _WeightScale, _MaximumValues * _WeightScale);
        }
        bias = UnityEngine.Random.Range(_MinimumValues * _BiasScale, _MaximumValues * _BiasScale);
    }
    public void MutateValues(float amount)
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] += UnityEngine.Random.Range(-amount * _WeightScale, amount * _WeightScale);
        }
        bias += UnityEngine.Random.Range(-amount * _BiasScale, amount * _BiasScale);
    }
    
    // Weights
    public void ClearWeights()
    {
        SetWeights(0);
    }
    public void SetWeights(float value)
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = value;
        }
    }
    public void SetWeight(int index, float value)
    {
        Weights[index] = value;
    }
    public float GetWeight(int index)
    {
        return Weights[index];
    }
    public float[] GetWeights()
    {
        return Weights;
    }
    public int GetTotalWeights()
    {
        return Weights.Length;
    }
    // Bias
    public void SetBias(float value)
    {
        bias = value;
    }
    public float GetBias()
    {
        return bias;
    }

     // Evaluate One Input
    public float Evaluate(float SingleInput)
    {
        if (1 != GetTotalWeights())
        {
            Debug.Log("BAD_1");
            return 0.0f;
        }

        float result = SingleInput * Weights[0];

        return Sigmoid(result + bias);
    }
    // Evaluate Array of inputs
    public float Evaluate(float[] InputArray)
    {
        if (InputArray.Length != GetTotalWeights())
        {
            Debug.Log("BAD_2");
            return 0.0f;
        }

        float result = 0.0f;
        for(int i = 0; i < InputArray.Length; i++)
        {
            result += InputArray[i] * Weights[i];
        }
        return Sigmoid(result + bias);
    }
    // Crossover
    public static Perceptron CrossOver(Perceptron p1, Perceptron p2, float p1_weight = 0.5f, float p2_weight = 0.5f)
    {
        int WeightCount = p1.GetTotalWeights();
        Perceptron Result = new Perceptron(WeightCount);

        for (int i = 0; i < WeightCount; i++)
        {
            Result.SetWeight(i, (p1.GetWeight(i) * p1_weight) + (p2.GetWeight(i) * p2_weight));
        }
        Result.SetBias((p1.GetBias() * p1_weight) + (p2.GetBias() * p2_weight));

        return Result;
    }
    // Copy
    public void Copy(Perceptron p)
    {
        for(int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = p.Weights[i];
        }

        bias = p.bias;
    }

}

