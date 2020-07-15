using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class ScoreSystem
{
    [Header("InGame Score:")]
    [Space()]
    // Score gained for creating lines
    public float Line_Single = 1000;
    public float Line_Double = 4000;
    public float Line_Triple = 9000;
    public float Line_Quad   = 16000;
    public float Line_Combo  = 32000;

    [Space()]
    public float Move_Left  = -0.1f;
    public float Move_Right = -0.1f;
    public float Move_Down  = -0.1f;
    [Tooltip("Score increase when rotate")]
    public float Rotate = -0.2f;
    public float RotatingCube = -1000;
    [Tooltip("Score increase per tile moved when dropping down")]
    public float DropDown_Per_Tile = 0;


    [Space()]
    [Header("Place Piece Score:")]

    [Space()]
    [Tooltip("Score when piece enters bottom row")]
    public float Bottom_Row = 10;
    [Tooltip("Score when piece enters top 2 rows")]
    public float Top_2_Rows = -10;

    [Tooltip("Score = lerp of height (bottom = 0, top = 1")]
    public float Height_Lerp_Bot = +10;
    [Tooltip("Score = lerp of height (bottom = 0, top = 1")]
    public float Height_Lerp_Top = -10;

    [Space()]
    [Tooltip("Score when piece enters MOST populated section of board")]
    public float Crowded_Section = -5;
    [Tooltip("Score when piece enters LEAST populated section of board")]
    public float NonCrowded_Section = 5;
    [Tooltip("Divides width of board into that many sections for crowded related scoring")]
    public int   Section_Counts_As_Width_Divided_By = 3;

    [Space()]
    [Tooltip("When a piece is placed, add this score for every hole that it creates")]
    public float HoleAdded = -1;
    [Tooltip("When a piece is placed, increase score by overall bumpiness")]
    public float BumpAverage = -1;
    [Tooltip("When a piece is placed, increase score by change in bumpiness * this value")]
    public float BumpFixMultiplier = -1;
    [Tooltip("When a piece is placed, increase score by aggregate height * this value")]
    public float AggregateHeightChangedMultiplier = -1;

    [Space()]
    [Header("End Game Score:")]

    [Space()]
    [Tooltip("Score for empty slots after losing")]
    public float GameOver_EmptySlots = 0;
    [Tooltip("Score for placing a piece")]
    public float Per_Piece_Placed = 1;


}