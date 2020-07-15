using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LineClear
{
    NONE,
    CLEAR_1,
    CLEAR_2,
    CLEAR_3,
    CLEAR_4,
    CLEAR_4_Combo
}

public class Tetris
{
    // Instance
    private static Tetris _instance;
    public static Tetris GetInstance()
    {
        if (_instance == null) _instance = new Tetris();
        return _instance;
    }

    private OutputPerceptronHistogram OPH = null;

    // Current Piece being used
    public static TetrisPiece _Piece = new TetrisPiece();

    // Score
    public static float _Fitness = 0;
    public static int   _Lines = 0;
    public static int   _TotalLines = 0;
    public static int   _PiecesUsed = 0;

    public static void IncreaseFitness(float value)
    {
        _Fitness += value;
    }
    public static void ClearScore()
    {
        _Fitness = 0;
    }

    // Tick Counter
    private int _TickCounter = 0;
    public int GetTickCounter()
    {
        return _TickCounter;
    }
    private void UpdateTickCounter()
    {
        _TickCounter ++;
    }

    // Control
    public enum Control
    {
        PLAYER,
        NEURAL_NETWORL
    }
    public Control _Controller;

    // Current Move
    // Current Move
    public enum Move
    {
        None = -1,
        WAIT,
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_DOWN,
        COLLAPSE,
        ROTATE_CW,
        ROTATE_CCW,
        TOTAL
    }
    private Move _CurrentMove = Move.None;
    private void SetMove(Move M)
    {
        _CurrentMove = M;
    }
    public Move GetMove()
    {
        return _CurrentMove;
    }


    // ACCESSOR PARTS
    //static bool UpdateOverclock = true;

    public void Setup()
    {
        TetrisBoard.Setup();

        // Get Histogram
        OPH = MonoBehaviour.FindObjectOfType<OutputPerceptronHistogram>();
        if (OPH == null)
            Debug.LogError("Missing Output Perceptron Histogram");
    }
	public void Update ()
    {
        // LOGIC
        int Frames = _Controller == Control.NEURAL_NETWORL ? GameplayManager.GetInstance().UpdatesPerFrame : 1;

       // if (Input.GetKeyDown(KeyCode.LeftControl))
       //     UpdateOverclock = !UpdateOverclock;

        for (int i = 0; i < Frames; i++)
        {
            UpdateMovement();
            Tick();
        }

        // Update Histogram
        if(OPH != null)
        {
            float[] Outputs = TetrisPerceptronManager.GetEpochManager().GetCurrentEpoch().CurrentOutputs;

            if (Outputs != null && Outputs.Length == (int)Tetris.Move.TOTAL)
            {
                for (int i = 0; i < Outputs.Length; i++)
                {
                    OPH.UpdateOutput(i, Outputs[i]);
                }
            }
        }

        // GRAPHICS
        TetrisBoardColors.UpdateColors();
    }

    // Movement
    private void UpdateMovement()
    {
        // Set Move
        if (_Controller == Control.PLAYER)
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                SetMove(Move.MOVE_LEFT);
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                SetMove(Move.MOVE_RIGHT);
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                SetMove(Move.MOVE_DOWN);
            else if (Input.GetKeyDown(KeyCode.Space))
                SetMove(Move.COLLAPSE);
            else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Comma))
                SetMove(Move.ROTATE_CCW);
            else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Period))
                SetMove(Move.ROTATE_CW);
        }
        else if (_Controller == Control.NEURAL_NETWORL)
        {
            // Update Neurons
            TetrisPerceptronManager.GetInstance().UpdateAI();
            // Set Move to its selected move
            SetMove(TetrisPerceptronManager._AI_Move);
        }

    }

    static float _Stall = Mathf.Infinity;
    // Tetris Update data
    private void Tick()
    {
        // slowdown for player since they don't play at framerate speed
        if (_Controller == Control.PLAYER)
        {
            _Stall += Time.deltaTime;
            if (_Stall > 0.04f)
                _Stall = 0.0f;
            else
                return;
        }

        // Check if current piece is empty
        if (_Piece._Type == Tetrominoes.NONE)
        {
            // Piece becomes a random new piece
            _Piece.BecomeNewPiece();
            // Check if overlaps at start
            bool Failure = _Piece.CheckOverlapsSolid();
            if (Failure)
            {
                EndGame();
                return;
            }
        }

        // Overlap
        bool Overlap = false;

        // Do Move
        switch (_CurrentMove)
        {
            case Move.MOVE_LEFT:
                if (_Piece.MoveLeft())
                    IncreaseFitness(ScoreSystemManager.GetCurrent().Move_Left);
                break;

            case Move.MOVE_RIGHT:
                if (_Piece.MoveRight())
                    IncreaseFitness(ScoreSystemManager.GetCurrent().Move_Right);
                break;

            case Move.MOVE_DOWN:
                if (_Piece.MoveDown())
                    IncreaseFitness(ScoreSystemManager.GetCurrent().Move_Down);
                else
                    Overlap = true;
                break;

            case Move.COLLAPSE:
                // Alternate Skip Mode
                if (GameplayManager.GetInstance().CollapseDoesNothing)
                    break;

                float sc = _Piece.Collapse();
                IncreaseFitness(ScoreSystemManager.GetCurrent().DropDown_Per_Tile * sc);
                break;

            case Move.ROTATE_CCW:
                if(_Piece.Rotate(false))
                {
                    IncreaseFitness(ScoreSystemManager.GetCurrent().Rotate);
                    if(_Piece._Type == Tetrominoes.O)
                        IncreaseFitness(ScoreSystemManager.GetCurrent().RotatingCube);
                }
                break;

            case Move.ROTATE_CW:
                if (_Piece.Rotate(true))
                {
                    IncreaseFitness(ScoreSystemManager.GetCurrent().Rotate);
                    if (_Piece._Type == Tetrominoes.O)
                        IncreaseFitness(ScoreSystemManager.GetCurrent().RotatingCube);
                }
                break;
        }

        // Update Move Text
        GameplayManager.GetInstance()._MoveText.text = GetMove().ToString();

        // Reset Move
        _CurrentMove = Move.WAIT;

        // If 5th tick go down by one regardless
        if (_TickCounter % 5 == 0 && !Overlap)
        {
            if (!_Piece.MoveDown())
                Overlap = true;
        }

        // Check if Piece is overlapping
        if (Overlap)
        {
            _Piece._y++;
            _Piece.UpdatePiece();
            AddPieceToBoard();
        }

        // TICK ++
        UpdateTickCounter();

        // Limited Pieces Game
        if (
            GameplayManager.GetInstance().LimitPieces > 0 &&
            _PiecesUsed > GameplayManager.GetInstance().LimitPieces - 1
            )
        {
            EndGame();
        }
    }

    // Add piece to board
    private void AddPieceToBoard()
    {
        // Move piece back up 1
        _Piece._y++;

        // Score from crowded section
        int section = TetrisBoard.CheckPieceInCrowd(ScoreSystemManager.GetCurrent().Section_Counts_As_Width_Divided_By);

        if (section == -1)
            IncreaseFitness(ScoreSystemManager.GetCurrent().NonCrowded_Section);
        else if (section == 1)
            IncreaseFitness(ScoreSystemManager.GetCurrent().Crowded_Section);

        // Current Bumpiness
        float Bump_Old = TetrisHeuristics.CalculateBumpiness();
        // Current Holes
        float Holes_Old = TetrisHeuristics.CountHoles();

        /* */
        /* */

        // Add piece to board
        TetrisBoard.AddSolidsToBoardFromPiece();

        // Remove Horizontal Lines
        LineClear LC = TetrisBoard.UpdateCompletedLines();
        // Give score for those lines
        if (!GameplayManager.GetInstance().LinesDoNotClear)
            IncreaseScoreFromLineClear(LC);

        /* */
        /* */

        // Current Bumpiness
        float Bump = TetrisHeuristics.CalculateBumpiness();
        IncreaseFitness(ScoreSystemManager.GetCurrent().BumpAverage * Bump);

        // Change in Bumpiness
        float Bump_Change = Bump - Bump_Old;
        IncreaseFitness(ScoreSystemManager.GetCurrent().BumpFixMultiplier * Bump_Change);

        // Change in Holes
        float Holes_Change = Mathf.Max(0.0f, TetrisHeuristics.CountHoles() - Holes_Old);
        IncreaseFitness(ScoreSystemManager.GetCurrent().HoleAdded * Holes_Change);
        // Current Aggregate Height Score
        float AggregateHeightScore = TetrisHeuristics.AggregateHeight() * ScoreSystemManager.GetCurrent().AggregateHeightChangedMultiplier;
        IncreaseFitness(AggregateHeightScore);

        // Check Values for Testing
        //Debug.Log("Aggregate Height: " + TetrisHeuristics.AggregateHeight());
        //Debug.Log("Complete Rows: " + TetrisHeuristics.CalculateCompleteRows());
        //Debug.Log("Holes: " + TetrisHeuristics.CountHoles());
        //Debug.Log("Bumpiness: " + TetrisHeuristics.CalculateBumpiness());
        //Debug.Log("~~~");

        // Score from height
        float y_level = (float)(_Piece._WorldMin.y) / 24.0f;
        float height_score = Mathf.Lerp(
            ScoreSystemManager.GetCurrent().Height_Lerp_Bot,
            ScoreSystemManager.GetCurrent().Height_Lerp_Top,
            y_level);
        IncreaseFitness(height_score);

        // Score from bottom row
        float BottomCount = TetrisHeuristics.CountTetrisPiecesRow(0);
        IncreaseFitness(ScoreSystemManager.GetCurrent().Bottom_Row * BottomCount);      

        // Score from top 2 rows
        float TopCount = TetrisHeuristics.CountTetrisPiecesRow(TetrisBoard._Height - 1);
        TopCount += TetrisHeuristics.CountTetrisPiecesRow(TetrisBoard._Height - 2);
        IncreaseFitness(ScoreSystemManager.GetCurrent().Top_2_Rows * TopCount);

        // Reset Piece
        _Piece._Type = Tetrominoes.NONE;

        // Add Total Pieces Added
        _PiecesUsed++;

    }

    // Score from line clear
    private void IncreaseScoreFromLineClear(LineClear LC)
    {
        ScoreSystem G = ScoreSystemManager.GetCurrent();

        switch (LC)
        {
            case LineClear.CLEAR_1:
                IncreaseFitness(G.Line_Single);
                break;

            case LineClear.CLEAR_2:
                IncreaseFitness(G.Line_Double);
                break;

            case LineClear.CLEAR_3:
                IncreaseFitness(G.Line_Triple);
                break;

            case LineClear.CLEAR_4:
                IncreaseFitness(G.Line_Quad);
                break;

            case LineClear.CLEAR_4_Combo:
                IncreaseFitness(G.Line_Combo);
                break;
        }

    }

    // End Current Game
    private void EndGame()
    {
        //Debug.Log("GAMEOVER");
        if (_Controller == Control.NEURAL_NETWORL)
        {
            // Count total empty spaces
            float empty_slots = TetrisBoard._TotalCells - TetrisBoard.CountSolidsInBoard();
            IncreaseFitness(ScoreSystemManager.GetCurrent().GameOver_EmptySlots * empty_slots);

            // Increase Fitness by the 
            IncreaseFitness(ScoreSystemManager.GetCurrent().Per_Piece_Placed * (float)_PiecesUsed);

            // Calculate Fitness from Heuristics
            TetrisPerceptronManager.GetInstance().FinishCurrentAI(_Fitness);
            GameplayManager.GetInstance()._Histogram.AddValue(_Fitness);

        }

        ResetGame();
    }

    // Reset
    public void ResetGame()
    {
        TetrisBoard.Reset();   
        ClearScore();
        _Lines = 0;
        _PiecesUsed = 0;
        _Piece._Type = Tetrominoes.NONE;
    }


}
