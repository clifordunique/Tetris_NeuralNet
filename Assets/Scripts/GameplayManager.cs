using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    // Get instance
    private static GameplayManager _instance;
    public static GameplayManager GetInstance()
    {
        if (_instance == null) _instance = FindObjectOfType<GameplayManager>();
        return _instance;
    }

    // INPUT DATA
    public GameObject _PixelPrefab = null;
    public Canvas _GameCanvas = null;
    public Text _TickText = null;
    public Text _CurrentPiece = null;
    public Text _NextPiece = null;
    public Text _MoveText = null;
    public Text _GenerationText = null;
    public Text _EpochText = null;
    public Text _LinesText = null;
    public Text _TotalLinesText = null;
    public Text _FitnessText = null;
    public Text _PiecesUsedText = null;
    public Text _BestSoFar = null;
    public Text _SimulationSpeed = null;
    public Text _TimeText = null;

    public CanvasHistogram _Histogram = null;

    public bool _DebugDisplayOutputFloats = false;


    [Header("CONTROLLER")]
    public Tetris.Control _Controller_Override;

    [Header("Perceptron Settings")]
    public float WeightSize = 0.2f;
    public float BiasSize = 2.0f;

    [Header("Feature Vector Settings")]
    public float FV_EMPTY = +0.0f;
    public float FV_SOLID = -1.0f;
    public float FV_PIECE = +10.0f;
    public float FV_GHOST = +100.0f;
    public TetrisBoard.OutputMode FeatureVectorMode = TetrisBoard.OutputMode.AllRows;

    [Header("Neural Net Settings")]

    public bool CrossOverWeighthingIsBiased = false;

    public int HiddenLayers = 2;
    public int HiddenLayerSize = 24;

    public int GenerationSize = 15;

    // What percentage is checked for scores to breed
    [Range(0.0f, 1.0f)]
    public float PercentageBreeding = 0.1f;
    
    // Percentage of children needed to be created for the next generation
    // Children replace the weakest percentage of the epochs
    [Range(0.0f, 1.0f)]
    public float PercentageChildrenPerGeneration = 0.3f;

    // Children Mutation Chance
    [Range(0.0f, 1.0f)]
    public float PercentageChildMutationChance = 0.05f;

    // Children Mutation Amount
    [Range(0.0f, 10.0f)]
    public float MutationAmount = 0.2f;

    // Weakest Value gets jittered at end of generation
    public bool MutateOneWeakestByALot = true;


    [Header("Game Settings")]

    public int  LimitPieces = 500;
    public bool LinesDoNotClear = false;

    public bool SpawnXPositionIsRandom = true;
    public bool SpawnRotationIsRandom = true;

    public bool CollapseDoesNothing = false;

    public bool RotationCantCauseMovement = false;

    public bool SingleTypeMode = false;
    public Tetrominoes[] SingleTypes = new Tetrominoes[1];

    [Header("AI SETTINGS")]

    public int RandomSeed = 0;
    public int UpdatesPerFrame = 1;
    
    // SETUP
    private void Awake()
    {
        // Set Feature Vector Data
        TetrisBoard._OutputMode = FeatureVectorMode;
        TetrisBoard.BOARD_DATA_EMPTY = FV_EMPTY;
        TetrisBoard.BOARD_DATA_SOLID = FV_SOLID;
        TetrisBoard.BOARD_DATA_PIECE = FV_PIECE;
        TetrisBoard.BOARD_DATA_GHOST = FV_GHOST;
        // Set Perceptron Global Data
        Perceptron._WeightScale = WeightSize;
        Perceptron._BiasScale   = BiasSize;

        if (SingleTypeMode)
        {
            // Check if single types has a null value
            for(int i = 0; i < SingleTypes.Length; i++)
            {
                if (SingleTypes[i] == Tetrominoes.NONE || SingleTypes[i] == Tetrominoes.TOTAL)
                {
                    Debug.LogError("SINGLE TYPE MODE INCORRECT!!");
                }
            }

            Tetris._Piece._CustomMoveSequence = SingleTypes;
        }

        // ERROR CHECKERS
        if (_Histogram == null)
            Debug.LogError("Missing Histogram");
        if (_PixelPrefab == null)
            Debug.LogError("Missing Pixel Prefab");
        if (_GameCanvas == null)
            Debug.LogError("Missing Game Canvas");
        if (_TickText == null)
            Debug.LogError("Missing Tick Text");
        if (_CurrentPiece == null)
            Debug.LogError("Missing Current Piece Text");
        if (_NextPiece == null)
            Debug.LogError("Missing Next Piece Text");
        if (_MoveText == null)
            Debug.LogError("Missing Move Text");
        if (_GenerationText == null)
            Debug.LogError("Missing Generation Text");
        if (_EpochText == null)
            Debug.LogError("Missing Epoch Text");
        if (_LinesText == null)
            Debug.LogError("Missing Lines Text");
        if (_TotalLinesText == null)
            Debug.LogError("Missing Total Lines Text");
        if (_FitnessText == null)
            Debug.LogError("Missing Fitness Text");
        if (_PiecesUsedText == null)
            Debug.LogError("Missing Pieces Used Text");
        if (_BestSoFar == null)
            Debug.LogError("Missing Best So Far Text");
        if (_SimulationSpeed == null)
            Debug.LogError("Missing Simulation Speed Text");
        if (_TimeText == null)
            Debug.LogError("Missing Time Text");

        //
        UnityEngine.Random.InitState(RandomSeed);
        //
        Tetris.GetInstance()._Controller = _Controller_Override;

        // Default
        Pixel._Width = _PixelPrefab.GetComponent<RectTransform>().rect.width;
        Pixel._Height = _PixelPrefab.GetComponent<RectTransform>().rect.height;

        Tetris.GetInstance().Setup();
    }

    // Update Text
    private void UpdateText()
    {
        // Objects
        Tetris T = Tetris.GetInstance();
        TetrisPerceptronManager TPM = TetrisPerceptronManager.GetInstance();
        
        // Text setters
        _TickText.text          = T.GetTickCounter().ToString();
        _CurrentPiece.text      = Tetris._Piece._Type.ToString();
        _NextPiece.text         = Tetris._Piece._NextType.ToString();
        _GenerationText.text    = TetrisPerceptronManager.GetEpochManager().GetGeneration().ToString();
        _EpochText.text         = (TetrisPerceptronManager.GetEpochManager().GetEpochIndex().ToString() + "/" + TetrisPerceptronManager.GetEpochManager().GetMaxEpochs().ToString());
        _LinesText.text         = Tetris._Lines.ToString();
        _TotalLinesText.text    = Tetris._TotalLines.ToString();
        _FitnessText.text       = Tetris._Fitness.ToString();
        // Move Text is updated internally
        if(LimitPieces <= 0)
            _PiecesUsedText.text = Tetris._PiecesUsed.ToString();
        else
            _PiecesUsedText.text = Tetris._PiecesUsed.ToString() + '/' + LimitPieces.ToString();

        _BestSoFar.text = _Histogram.GetMaxValue().ToString();
        _SimulationSpeed.text = UpdatesPerFrame.ToString();

        if(!PauseMenu.GetInstance().getActive())
        {
            int time = (int)(Time.time - PauseMenu.GetInstance()._PauseTime);
            string seconds = (time % 60).ToString("00");
            string minutes = (time / 60 % 60).ToString("00");
            string hours = (time / 3600).ToString("0000");

            _TimeText.text = hours + ':' + minutes + ':' + seconds;
        }

            
    }

    // UPDATE CALL
    void Update ()
    {
        // Only update is not paused and saving thread is not running
        if(!PauseMenu.GetInstance().getActive() &&
            SavingThread.GetInstance().checkThreadRunning() == false
            )
        {
            Tetris.GetInstance().Update();
        }

        UpdateText();

        // Change Simulation Speed
        if(Input.GetKey(KeyCode.Alpha1))
            UpdatesPerFrame = 1;
        else if (Input.GetKey(KeyCode.Alpha2))
            UpdatesPerFrame = 10;
        else if (Input.GetKey(KeyCode.Alpha3))
            UpdatesPerFrame = 100;
        else if (Input.GetKey(KeyCode.Alpha4))
            UpdatesPerFrame = 500;
        else if (Input.GetKey(KeyCode.Alpha5))
            UpdatesPerFrame = 1000;
        else if (Input.GetKey(KeyCode.Alpha6))
            UpdatesPerFrame = 5000;
    }
}
