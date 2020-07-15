using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisHeuristics
{
    // Calculate height of board by getting highest y value of each
    // column and adding them together
    public static float AggregateHeight()
    {
        float value = 0;
        for (int x = 0; x < TetrisBoard._Width; x++)
        {
            value += (TetrisBoard.ColumnHighestSolid[x] + 1);
        }
        return value;
    }

    // Below the top solid of every column, count everything that's empty
    public static float CountHoles()
    {
        float holes = 0;
        for (int x = 0; x < TetrisBoard._Width; x++)
        {
            for (int y = TetrisBoard.ColumnHighestSolid[x]; y >= 0; y--)
            {
                if (!TetrisBoard._Board[y, x]._Solid)
                    holes++;
            }
        }
        return holes;
    }

    // Count holes that are cause by the new piece added to the board
    // Must be called before piece is added to board
    public static float CountNewHoles()
    {
        float holes = 0;
        // Get Lowest parts of piece
        Vector2[] pieces = Tetris._Piece.GetPieces_LowestOnes();

        // Check if tile below them are solid
        for(int i = 0; i < pieces.Length; i++)
        {
            Vector2 Check = pieces[i] + Vector2.down;

            if (!TetrisBoard.CheckInBoardRange((int)Check.x, (int)Check.y))
                continue;

            // If hole below them, add to counter
            if (!TetrisBoard.CheckSolid((int)Check.x, (int)Check.y))
                holes++;
        }

        return holes;
    }

    // Figure out bumpiness by looking at the absolute difference of every
    // adjacent column height
    public static float CalculateBumpiness()
    {
        float bump = 0;
        for (int x = 0; x < TetrisBoard._Width - 1; x++)
        {
            bump += Mathf.Abs(TetrisBoard.ColumnHighestSolid[x] - TetrisBoard.ColumnHighestSolid[x + 1]);
        }
        return bump;
    }

    // Check how many parts of the tetris piece are in the bottom row
    public static float CountTetrisPiecesRow(int y)
    {
        float count = 0;
        Vector2[] Pieces = Tetris._Piece.GetPieces();
        for(int i = 0; i < Pieces.Length; i++)
        {
            if (Pieces[i].y == y)
                count++;
        }
        return count;
    }
}
