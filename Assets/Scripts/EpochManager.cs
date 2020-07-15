using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EpochManager
{
    // Create Epoch
    public static Epoch CreateEpoch(
        int hidden_layers,
        int hidden_layer_size,
        int output_layer_size)
    {
        return new Epoch(TetrisBoard.GetDataSize(), hidden_layers, hidden_layer_size, output_layer_size);
    }

    // Constructor
    public EpochManager(
        int hidden_layers,
        int hidden_layer_size,
        int output_layer_size
        )
    {
        _MaxEpochs = GameplayManager.GetInstance().GenerationSize;

        for(int i = 0; i < _MaxEpochs; i++)
        {
            Epoch E = CreateEpoch(hidden_layers, hidden_layer_size, output_layer_size);
            _Epoch_All.Add(E);
        }

        _Best = CreateEpoch(hidden_layers, hidden_layer_size, output_layer_size);
        _Best.SetScore(0);

        NextEpoch();


        UpdateChildrenCount();
        UpdateBreedingAmount();
    }

    public void UpdateChildrenCount()
    {
        // How many children are needed
        NewChildrenCount = Mathf.RoundToInt(GameplayManager.GetInstance().PercentageChildrenPerGeneration * _MaxEpochs);
        // Children amount cannot surpass epoch count
        NewChildrenCount = Mathf.Min(NewChildrenCount, _MaxEpochs);
    }
    public void UpdateBreedingAmount()
    {
        // Breeding Amount
        BreedingAmount = Mathf.RoundToInt(GameplayManager.GetInstance().PercentageBreeding * _MaxEpochs);
        // Breeding amount cannot surpass epoch count
        BreedingAmount = Mathf.Min(BreedingAmount, _MaxEpochs);
    }


    // Generation Count
    private int                _Generation = 0;
    // Epoch Data
    private Epoch             _Epoch_Current = null;
    private List<Epoch>       _Epoch_All = new List<Epoch>();
    private int               _Epoch_Index = 0;
    private int               _MaxEpochs;
    // Constant
    private int NewChildrenCount = 0; // New children for next epoch
    private int BreedingAmount = 0;   // How many random epochs will be tested for crossover

    // Best Epoch
    Epoch _Best = null;

    // Reset All Score
    public void ResetScores()
    {
        for (int i = 0; i < _Epoch_All.Count; i++)
        {
            _Epoch_All[i].SetScore(0);
        }
    }

    // Getters
    public int GetGeneration()
    {
        return _Generation;
    }
    public int GetEpochIndex()
    {
        return _Epoch_Index;
    }
    public int GetMaxEpochs()
    {
        return _MaxEpochs;
    }
    public Epoch GetCurrentEpoch()
    {
        return _Epoch_Current;
    }
    public Epoch GetBestEpoch()
    {
        return _Best;
    }
    public Epoch GetBestEpochAcrossGeneration()
    {
        Epoch Winner = _Best;
        
        for(int i = 0; i < _Epoch_All.Count; i++)
        {
            // Ignore current epoch
            if (_Epoch_Current == _Epoch_All[i])
                continue;

            // Null possibility
            if (Winner == null)
            {
                Winner = _Epoch_All[i];
                continue;
            }

            // Update
            if (_Epoch_All[i].GetScore() > Winner.GetScore())
            {
                Winner = _Epoch_All[i];
            }
        }
        return Winner;
    }
        

    // Override Current Epoch
    public void OverrideCurrentEpoch(Epoch E)
    {
        if(E == null)
        {
            Debug.LogError("Parameter Empty");
            return;
        }

        if(_Epoch_Current == null)
        {
            Debug.LogError("No current Epoch :(");
            return;
        }

        _Epoch_Current.CopyData(E);
        _Epoch_Current.SetScore(0);
    }

    // Get Current Best Output
    public int GetCurrentEpochBestOutput(float[] InputArray)
    {
        if (_Epoch_Current == null)
            return -1;

        return _Epoch_Current.GetBestOutput(InputArray);
    }

    // Finish Current Epoch
    public void FinishEpoch(float EpochScore)
    {
        // Store Score
        _Epoch_Current.SetScore(EpochScore);
        // go to next epoch
        NextEpoch();
    }

    // Go to next epoch
    private void NextEpoch()
    {
        // Reached End
        if(_Epoch_Index >= _MaxEpochs)
        {
            _Epoch_Current = null;
            _Epoch_Index = 0;
            GenerationEnd();
            return;
        }

        _Epoch_Current = _Epoch_All[_Epoch_Index];
        _Epoch_Index++;
    }

    // Find Best Epoch from list
    private Epoch FindBestEpochByScore(List<Epoch> epochs, Epoch exclude = null)
    {
        Epoch E = null;
        float Score = -Mathf.Infinity;
        
        for (int i = 0; i < epochs.Count; i++)
        {
            if (exclude == epochs[i])
                continue;
        
            if (E == null || epochs[i].GetScore() > Score)
            {
                E = epochs[i];
                Score = epochs[i].GetScore();
            }
        }
        
        return E;
    }

    // Acquire Random Epochs
    private List<Epoch> AcquireRandomEpochs(int amount)
    {
        List<Epoch> EpochDuplicates = new List<Epoch>(_Epoch_All);

        for(int i = 0; i < amount; i++)
        {
            Epoch Temp = EpochDuplicates[i];
            int randm = UnityEngine.Random.Range(0, EpochDuplicates.Count);
            EpochDuplicates[i] = EpochDuplicates[randm];
            EpochDuplicates[randm] = Temp;
        }
        EpochDuplicates.RemoveRange(amount, EpochDuplicates.Count - amount);
        return EpochDuplicates;
    }

    // Order Epochs by Score [TESTED MULTIPLE TIMES, this function works for sure]
    private List<Epoch> ReorderCurrentEpochsByScore()
    {
        return _Epoch_All.OrderBy(o => o.GetScore()).ToList();
    }

    // Calculate Mutate Chance [TESTED]
    private bool CalculateMutationChance()
    {
        int random = UnityEngine.Random.Range(0, 100);
        return ((float)random / 100.0f < GameplayManager.GetInstance().PercentageChildMutationChance);
    }
    
    // Display All Epochs Score
    public void DisplayScores()
    {
        for(int i = 0; i < _MaxEpochs; i++)
        {
            Debug.Log("Epoch " + i.ToString() + " Score: " + _Epoch_All[i].GetScore().ToString());
        }
        Debug.Log("~~~");
    }

    // Generation End
    private void GenerationEnd()
    {
        // Update Values
        UpdateChildrenCount();
        UpdateBreedingAmount();

        _Generation++;

        // Order all epochs based on score
        _Epoch_All = ReorderCurrentEpochsByScore();

        // Take Best Epoch
        if(_Epoch_All[_MaxEpochs - 1].GetScore() > _Best.GetScore())
        {
            _Best.CopyData(_Epoch_All[_MaxEpochs - 1]);
            _Best.SetScore(_Epoch_All[_MaxEpochs - 1].GetScore());
        }

        // New Children
        Epoch[] NewChildren = new Epoch[NewChildrenCount];

        // Create Children Values
        for (int i = 0; i < NewChildrenCount; i++)
        {
            // Acquire Random Amount of Epochs
            List<Epoch> Randos = AcquireRandomEpochs(BreedingAmount);
            // Find best 2 epochs
            Epoch Best_1 = FindBestEpochByScore(Randos);
            Epoch Best_2 = FindBestEpochByScore(Randos, Best_1);
            // Child Value
            NewChildren[i] = Epoch.CrossOver(Best_1, Best_2);
            //  Debug.Log("Best1 Bias: " + Best_1.GetOutputLayers()[0].GetBias());
            //  Debug.Log("Best2 Bias: " + Best_2.GetOutputLayers()[0].GetBias());
            //  Debug.Log("Child Bias: " + NewChildren[i].GetOutputLayers()[0].GetBias());
            //  Debug.Log("~~~");

            // Chance that child mutates
            if (CalculateMutationChance())
            {
                NewChildren[i].MutateValues(GameplayManager.GetInstance().MutationAmount);
            }
        }

        // Replace weakest ones with children
        for(int i = 0; i < NewChildrenCount; i++)
        {
            _Epoch_All[i].CopyData(NewChildren[i]);
        }

        // Add best epoch back in the list
        _Epoch_All[NewChildrenCount].CopyData(_Best);

        // Weakest one right after children getss jittered a lot for fun
        if (GameplayManager.GetInstance().MutateOneWeakestByALot)
            _Epoch_All[NewChildrenCount].MutateValues(GameplayManager.GetInstance().MutationAmount * 2.0f);

        // Reset all scores
        ResetScores();

        // Start Next Epoch
        NextEpoch();


        //  // Find best 2 Epochs
        //  Epoch Winner_1st = FindBestEpochByScore();
        //  Epoch Winner_2nd = FindBestEpochByScore(Winner_1st);
        //  Epoch Child = Epoch.CrossOver(Winner_1st, Winner_2nd);
        //  
        //  // Error Check
        //  if(Child == null)
        //  {
        //      Debug.LogError("CrossoverChild Error");
        //      NextEpoch();
        //      return;
        //  }
        //  
        //  // Set first two epochs as winner and child
        //  _Epoch_All[0] = Winner_1st;
        //  _Epoch_All[1] = Child;
        //  
        //  // Rest of epochs are just clones of child with jitter
        //  for (int i = 2; i < _MaxEpochs; i++)
        //  {
        //      _Epoch_All[i].CopyLayers(Child);
        //      _Epoch_All[i].JitterLayers(GameplayManager.GetInstance().MutationAmount);
        //  }


    }

}