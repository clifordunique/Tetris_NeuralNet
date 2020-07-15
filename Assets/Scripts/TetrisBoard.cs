using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TetrisBoard
{
    // Output Information
    public enum OutputMode
    {
        EntireArray,
        AllRows,
        AllColumns,
        AllRowsAndColumns
    }
    public static OutputMode _OutputMode = OutputMode.EntireArray;
    public static bool _OutputPieceID = true;

    // Values that fill the 1D array to send to the Neural network
    public static float BOARD_DATA_EMPTY = +0.0f;
    public static float BOARD_DATA_SOLID = -1.0f;
    public static float BOARD_DATA_PIECE = +50.0f;
    public static float BOARD_DATA_GHOST = +500.0f;

    // Pixels for each spot in the board
    public static readonly int _Width = 12;
    public static readonly int _Height = 24;
    public static readonly int _TotalCells = _Width * _Height;
    public static Pixel[,] _Board = new Pixel[_Height, _Width];
    // Solid Counts
    public static int[] ColumnSolids = new int[_Width];
    public static int[] ColumnHighestSolid = new int[_Width];
    private static void UpdateHighestSolidInColumn(int x)
    {
        for(int y = _Height - 1; y >= 0; y--)
        {
            if(_Board[y, x]._Solid)
            {
                ColumnHighestSolid[x] = y;
                return;
            }
            else if(y == 0)
            {
                // No solids
                ColumnHighestSolid[x] = -1;
            }
        }
    }
    private static void UpdateHighestSolidsOnBoard()
    {
        for (int x = 0; x < _Width; x++)
        {
            UpdateHighestSolidInColumn(x);
        }
    }

    private static bool _IsSetup = false;

    // SETUP
    public static void Setup()
    {
        if (_IsSetup)
            return;

        for (int y = 0; y < _Height; y++)
        {
            for (int x = 0; x < _Width; x++)
            {
                _Board[y, x] = new Pixel(x, y);
            }
        }

        // Some default
        UpdateHighestSolidsOnBoard();

        _IsSetup = true;
    }
    // RESET
    public static void Reset()
    {
        for (int x = 0; x < _Width; x++)
        {
            ColumnSolids[x] = 0;
            ColumnHighestSolid[x] = -1;
            for (int y = 0; y < _Height; y++)
            {
                _Board[y, x]._Solid = false;
            }
        }
    }

    // Empty = 1 // Solid = 2  // Piece = 4
    public static int GetDataSize()
    {
        int result = 0;
        switch(_OutputMode)
        {
            case OutputMode.EntireArray:
                result = _TotalCells;
                break;

            case OutputMode.AllRows:
                result = _Height;
                break;

            case OutputMode.AllColumns:
                result = _Width;
                break;

            case OutputMode.AllRowsAndColumns:
                result = _Height + _Width;
                break;

            default:
                Debug.LogError("Enum Error Type");
                break;
        }

        if (_OutputPieceID)
            result += 2;

        return result;
        
    }
    // Get Entire Board as a 1D array for Neural Network
    public static float[] GetBoard1D()
    {
        float[] Data = new float[_TotalCells];

        // Base Board
        for (int i = 0; i < _TotalCells; ++i)
        {
            int x = i % _Width;
            int y = i / _Width;
            Data[i] = _Board[y, x]._Solid ? BOARD_DATA_SOLID : BOARD_DATA_EMPTY;
        }
        // Check piece
        Vector2[] PieceLocations = Tetris._Piece.GetPieces();
        if (PieceLocations != null)
        {
            for (int i = 0; i < PieceLocations.Length; i++)
            {
                int x = (int)PieceLocations[i].x;
                int y = (int)PieceLocations[i].y;
                int ghost_y = (int)Tetris._Piece.GetPieceGhostY(i);

                // Check X range
                if (x >= 0 && x < _Width)
                {
                    // Ghost
                    if (ghost_y >= 0 && ghost_y < _Height)
                        Data[(x + ghost_y * _Width)] = BOARD_DATA_GHOST;
                    // Piece
                    if (y >= 0 && y < _Height)
                        Data[(x + y * _Width)] = BOARD_DATA_PIECE;
                }
            }
        }

        return Data;
    }
    public static float[] GetData()
    {
        int DataSize = GetDataSize();
        float[] Data = new float[DataSize];

        switch(_OutputMode)
        {
            case OutputMode.EntireArray:
                {
                    float[] Board = GetBoard1D();
                    System.Array.Copy(Board, Data, Board.Length);
                }
                break;

            case OutputMode.AllRows:
                {
                    float[] Board = GetBoard1D();
                    for(int y = 0; y < _Height; ++y)
                    {
                        float row_amount = 0.0f;
                        for (int x = 0; x < _Width; x++)
                        {
                            row_amount += Board[x + y * _Width];
                        }
                        Data[y] = row_amount;
                    }
                }
                break;

            case OutputMode.AllColumns:
                {
                    float[] Board = GetBoard1D();
                    for (int x = 0; x < _Width; ++x)
                    {
                        float column_amount = 0.0f;
                        for (int y = 0; y < _Height; y++)
                        {
                            column_amount += Board[x + y * _Width];
                        }
                        Data[x] = column_amount;
                    }
                }
                break;

            case OutputMode.AllRowsAndColumns:
                {
                    float[] Board = GetBoard1D();
                    for (int y = 0; y < _Height; ++y)
                    {
                        float row_amount = 0.0f;
                        for (int x = 0; x < _Width; x++)
                        {
                            row_amount += Board[x + y * _Width];
                        }
                        Data[y] = row_amount;
                        
                    }
                    for (int x = 0; x < _Width; ++x)
                    {
                        float column_amount = 0.0f;
                        for (int y = 0; y < _Height; y++)
                        {
                            column_amount += Board[x + y * _Width];
                        }
                        Data[_Height + x] = column_amount;
                    }
                }
                break;
        }

        // Extra data for current piece and next piece as simple numbers
        Data[DataSize - 2] = 0.0f;//float)Tetris._Piece._Type;
        Data[DataSize - 1] = 0.0f;//(float)Tetris._Piece._Orientation;

        return Data;
    }

    // Check In Range
    public static bool CheckInBoardRange(int x, int y)
    {
        return x >= 0 && x < _Width && y >= 0 && y < _Height;
    }

    // Check Solids in Board
    public static bool CheckSolid(int x, int y)
    {
        return _Board[y, x]._Solid;
    }
    public static bool CheckRowIsOnlySolids(int y)
    {
        return CountSolidsInRow(y) == _Width;
    }
    public static bool CheckColumnIsOnlySolids(int x)
    {
        return CountSolidsInColumn(x) == _Height;
    }
    // Count Solids in Board
    public static int  CountSolidsInColumn(int x)
    {
        return ColumnSolids[x];
    }
    public static int  CountSolidsInColumn(int x1, int delta_x)
    {
        int result = 0;
        for (int i = x1; i < x1 + delta_x; i++)
        {
            result += ColumnSolids[i];
        }
        return result;
    }
    public static int  CountSolidsInRow(int y)
    {
        int result = 0;
        for (int x = 0; x < _Width; x++)
        {
            if (_Board[y, x]._Solid)
                result++;
        }
        return result;
    }
    public static int  CountSolidsInBoard()
    {
        int count = 0;
        for (int y = 0; y < _Height; y++)
        {
            for (int x = 0; x < _Width; x++)
            {
                if (_Board[y, x]._Solid)
                    count++;
            }
        }
        return count;
    }
    public static int  CountPiecesInRow(int y)
    {
        int result = 0;
        Vector2[] Pieces = Tetris._Piece.GetPieces();
        for (int i = 0; i < Pieces.Length; i++)
        {
            if (Pieces[i].y == y)
                result++;
        }
        return result;
    }
    public static int  CountPiecesInColumn(int x)
    {
        int result = 0;
        Vector2[] Pieces = Tetris._Piece.GetPieces();
        for (int i = 0; i < Pieces.Length; i++)
        {
            if (Pieces[i].x == x)
                result++;
        }
        return result;
    }

    // Add Solids To Board
    public static void AddSolidToBoard(Vector2 Loc)
    {
        int x = Mathf.RoundToInt(Loc.x);
        int y = Mathf.RoundToInt(Loc.y);

        // Pieces may accidentally be out of range at initial placement
        if(!CheckInBoardRange(x, y))
            return;

        _Board[y, x]._Solid = true;
        _Board[y, x]._SolidColor = Tetris._Piece._Color * 0.5f;
        ColumnSolids[x]++;
        UpdateHighestSolidInColumn(x);
    }
    public static void AddSolidsToBoardFromPiece()
    {
        Vector2[] Pieces = Tetris._Piece.GetPieces();
        for (int i = 0; i < Pieces.Length; i++)
        {
            AddSolidToBoard(Pieces[i]);
        }
    }

    // Combo Reminder
    private static bool _Combo = false;

    // Line Remover Stuff
    public static LineClear UpdateCompletedLines()
    {
        // Check Quadruple lines
        for (int y = 0; y < _Height - 3; y++)
        {
            if (
                CheckRowIsOnlySolids(y + 0) &&
                CheckRowIsOnlySolids(y + 1) &&
                CheckRowIsOnlySolids(y + 2) &&
                CheckRowIsOnlySolids(y + 3)
                )
            {
                RemoveLineTetris(y + 0);
                RemoveLineTetris(y + 1);
                RemoveLineTetris(y + 2);
                RemoveLineTetris(y + 3);

                LowerField(y, 4);

                if (_Combo)
                    return LineClear.CLEAR_4_Combo;
                else
                {
                    _Combo = true;
                    return LineClear.CLEAR_4;
                }

            }
        }
        // Check Triple lines
        for (int y = 0; y < _Height - 2; y++)
        {
            if (
                CheckRowIsOnlySolids(y + 0) &&
                CheckRowIsOnlySolids(y + 1) &&
                CheckRowIsOnlySolids(y + 2)
                )
            {
                RemoveLineTetris(y + 0);
                RemoveLineTetris(y + 1);
                RemoveLineTetris(y + 2);

                LowerField(y, 3);

                _Combo = false;

                return LineClear.CLEAR_3;
            }
        }
        // Check Double lines
        for (int y = 0; y < _Height - 1; y++)
        {
            if (
                CheckRowIsOnlySolids(y + 0) &&
                CheckRowIsOnlySolids(y + 1)
                )
            {
                RemoveLineTetris(y + 0);
                RemoveLineTetris(y + 1);

                LowerField(y, 2);

                _Combo = false;

                return LineClear.CLEAR_2;
            }
        }
        // Check Single lines
        for (int y = 0; y < _Height; y++)
        {
            if (
                CheckRowIsOnlySolids(y + 0)
                )
            {
                RemoveLineTetris(y + 0);

                LowerField(y, 1);

                _Combo = false;

                return LineClear.CLEAR_1;
            }
        }

        return LineClear.NONE;
    }
    public static void LowerField(int y_min, int amount)
    {
        for (int y = y_min; y < _Height; y++)
        {
            LowerLine(y, amount);
        }
    }
    public static void LowerLine(int y, int amount)
    {
        // Offscreen deletion
        if (y + amount >= _Height)
        {
            for (int x = 0; x < _Width; x++)
            {
                _Board[y, x]._Solid = false;
            }
        }

        // Lower Normally
        else
        {
            for (int x = 0; x < _Width; x++)
            {
                // Lower one becomes top one
                _Board[y, x]._Solid = _Board[y + amount, x]._Solid;
                _Board[y, x]._SolidColor = _Board[y + amount, x]._SolidColor;
            }
        }
    }
    public static void RemoveLineTetris(int y)
    {
        // Update winning lines
        if (!GameplayManager.GetInstance().LinesDoNotClear)
        {
            for (int x = 0; x < _Width; x++)
            {
                _Board[y, x]._Solid = false;
            }
            Tetris._Lines++;
            Tetris._TotalLines++;
        }
    }

    // Check Height of piece
    public static int CheckApproxPieceHeight()
    {
        int X_index = Mathf.RoundToInt(Tetris._Piece.GetPieceWorldAverageLocation().x);
        return ColumnHighestSolid[X_index];
    }
    // Check if Piece is in empty section // 0 = normal, -1 = empty, +1 = crowded
    public static int CheckPieceInCrowd(int sections)
    {
        if (sections == 0)
            return 0;

        // Divide grid into x columns worth of points
        int[] column_solids = new int[sections];

        int width_of_section = _Width / sections;

        for (int i = 0; i < sections; i++)
        {
            column_solids[i] += CountSolidsInColumn(i * width_of_section, width_of_section);
        }

        // Find smallest and biggest section
        int min_index = 0;
        int max_index = 0;
        Utility.GetArrayMinMaxIndices(column_solids, ref min_index, ref max_index);

        //Debug.Log("MIN INDEX: " + min_index);
        //Debug.Log("MAX INDEX: " + max_index);

        // Figure out which column the piece is ons
        float AverageX = Tetris._Piece.GetPieceWorldAverageLocation().x;
        int X_index = (int)((AverageX / _Width) * sections);
        // In case it reaches the maxima
        if (X_index >= sections) X_index = sections - 1;

        //
        if (X_index == min_index)
            return -1;
        else if (X_index == max_index)
            return +1;

        return 0;
    }

}