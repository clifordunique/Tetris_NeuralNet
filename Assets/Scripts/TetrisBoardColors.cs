using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TetrisBoardColors
{
    // BOARD COLOR
    public static Color GetBackgroundColor(int x, int y)
    {
        // Top 2 rows = really dark
        if (y >= TetrisBoard._Height - 2)
        {
            if (x % 2 == 0)
                return new Color(0.27f, 0.27f, 0.27f);
            else
                return new Color(0.30f, 0.30f, 0.30f);
        }
        else
        {
            if (x % 2 == 0)
                return new Color(0.60f, 0.60f, 0.60f);
            else
                return new Color(0.57f, 0.57f, 0.57f);
        }
    }
    public static void UpdateColors()
    {
        // BG COLOR
        for (int y = 0; y < TetrisBoard._Height; y++)
        {
            for (int x = 0; x < TetrisBoard._Width; x++)
            {
                // Background Color
                Color C = GetBackgroundColor(x, y);
                // Check if piece is occupied
                if (TetrisBoard._Board[y, x]._Solid)
                    C = TetrisBoard._Board[y, x]._SolidColor;

                TetrisBoard._Board[y, x]._SolidColor = C;
            }
        }
        // Game Piece Color
        Vector2[] PieceLocations = Tetris._Piece.GetPieces();
        if (PieceLocations != null)
        {
            for (int i = 0; i < PieceLocations.Length; i++)
            {
                int x = (int)PieceLocations[i].x;
                int y = (int)PieceLocations[i].y;
                int ghost_y = (int)Tetris._Piece.GetPieceGhostY(i);

                if (x >= 0 && x < TetrisBoard._Width)
                {
                    // Ghost Color
                    if (ghost_y >= 0 && ghost_y < TetrisBoard._Height && !TetrisBoard._Board[ghost_y, x]._Solid)
                    {
                        TetrisBoard._Board[ghost_y, x]._SolidColor = Color.Lerp(TetrisBoard._Board[ghost_y, x]._SolidColor, Tetris._Piece._Color, 0.2f);
                    }

                    // Piece Color
                    if (y >= 0 && y < TetrisBoard._Height && !TetrisBoard._Board[y, x]._Solid)
                        TetrisBoard._Board[y, x]._SolidColor = (Tetris._Piece._Color);
                }
            }
        }

        // Update Color
        for (int y = 0; y < TetrisBoard._Height; y++)
        {
            for (int x = 0; x < TetrisBoard._Width; x++)
            {
                TetrisBoard._Board[y, x].UpdateColor();
            }
        }
    }
}