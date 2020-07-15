using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TetrisPerceptronManager
{
    // Instance
    private static TetrisPerceptronManager _instance;
    public static TetrisPerceptronManager GetInstance()
    {
        if (_instance == null) _instance = new TetrisPerceptronManager();
        return _instance;
    }

    // Setup
    public TetrisPerceptronManager()
    {
        _EpochManager = new EpochManager(
            GameplayManager.GetInstance().HiddenLayers,
            GameplayManager.GetInstance().HiddenLayerSize,
            (int)Tetris.Move.TOTAL
            );
    }

    // Epoch Manager
    private EpochManager _EpochManager = null;
    // Get Epoch Manager
    public static EpochManager GetEpochManager()
    {
        return GetInstance()._EpochManager;
    }

    // Winning Move
    public static Tetris.Move _AI_Move = Tetris.Move.WAIT;

    // Called to figure out next move of AI
    public void UpdateAI()
    {
        // Update Input
        float[] InputArray = TetrisBoard.GetData();

        Tetris.Move Choice = Tetris.Move.WAIT;

        // Update Epoch
        int BestOutput = _EpochManager.GetCurrentEpochBestOutput(InputArray);
        if (BestOutput != -1)
        {
            Choice = (Tetris.Move)BestOutput;
        }

        // Set Winning Move
        _AI_Move = Choice;
    }
    // Called when AI is finished
    public void FinishCurrentAI(float score)
    {
        _EpochManager.FinishEpoch(score);
    }

    // Get Current Epoch
    public Epoch GetCurrentEpoch()
    {
        return _EpochManager.GetCurrentEpoch();
    }
        

}
