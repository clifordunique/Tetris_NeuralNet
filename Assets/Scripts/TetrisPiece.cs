using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TetrominoConfig
{
    // Static pieces (initial position)
    public static readonly Vector2[] I_Pieces = {
        new Vector2(0, -1),
        new Vector2(0, +0),
        new Vector2(0, +1),
        new Vector2(0, +2)
    };
    public static readonly Vector2[] S_Pieces = {
        new Vector2(-1, +0),
        new Vector2(+0, +0),
        new Vector2(+0, +1),
        new Vector2(+1, +1)
    };
    public static readonly Vector2[] Z_Pieces = {
        new Vector2(+1, +0),
        new Vector2(+0, +0),
        new Vector2(+0, +1),
        new Vector2(-1, +1)
    };
    public static readonly Vector2[] O_Pieces = {
        new Vector2(+0, +0),
        new Vector2(+1, +0),
        new Vector2(+0, +1),
        new Vector2(+1, +1)
    };
    public static readonly Vector2[] L_Pieces = {
        new Vector2(+1, -1),
        new Vector2(+0, -1),
        new Vector2(+0, +0),
        new Vector2(+0, +1)
    };
    public static readonly Vector2[] J_Pieces = {
        new Vector2(-1, -1),
        new Vector2(+0, -1),
        new Vector2(+0, +0),
        new Vector2(+0, +1)
    };
    public static readonly Vector2[] T_Pieces = {
        new Vector2(-1, +0),
        new Vector2(+0, +0),
        new Vector2(+1, +0),
        new Vector2(+0, +1)
    };
}

public enum Tetrominoes
{
    NONE = -1,
    I,
    S,
    Z,
    O,
    L,
    J,
    T,
    TOTAL
}

public class TetrisPiece
{
    // Piece Sequence
    private Tetrominoes[] _MoveSequence = new Tetrominoes[(int)Tetrominoes.TOTAL]
    {
        Tetrominoes.I,
        Tetrominoes.S,
        Tetrominoes.Z,
        Tetrominoes.O,
        Tetrominoes.L,
        Tetrominoes.J,
        Tetrominoes.T
    };
    public Tetrominoes[] _CustomMoveSequence;

    private int _MoveSequence_Index = 0;
    private void ShuffleMoveSequence(ref Tetrominoes[] Sequence)
    {
        for(int i = 0; i < Sequence.Length; i++)
        {
            int random_index = UnityEngine.Random.Range(0, Sequence.Length);
            Tetrominoes Temp = Sequence[i];
            Sequence[i] = Sequence[random_index];
            Sequence[random_index] = Temp;
        }
    }

    // DATA
    public Tetrominoes _Type = Tetrominoes.NONE;
    public Tetrominoes _NextType = Tetrominoes.NONE;
    private void GenerateNextType()
    {
        // Get Next Shape
        if (GameplayManager.GetInstance().SingleTypeMode)
        {
            // Custom pieces mode
            _NextType = _CustomMoveSequence[_MoveSequence_Index];

            // Increase index for next shape
            _MoveSequence_Index++;
            // If reached end of line, shuffle and reset
            if (_MoveSequence_Index == _CustomMoveSequence.Length)
            {
                ShuffleMoveSequence(ref _CustomMoveSequence);
                _MoveSequence_Index = 0;
            }
        }
        else
        {
            // Normal pieces mode
            _NextType = _MoveSequence[_MoveSequence_Index];

            // Increase index for next shape
            _MoveSequence_Index++;
            // If reached end of line, shuffle and reset
            if (_MoveSequence_Index == (int)Tetrominoes.TOTAL)
            {
                ShuffleMoveSequence(ref _MoveSequence);
                _MoveSequence_Index = 0;
            }
        }
    }
    public void BecomeNewPiece()
    {
        // First Value workaround
        if (_NextType == Tetrominoes.NONE)
            GenerateNextType();

        _Type = _NextType;
        GenerateNextType();
        //_Type = Tetrominoes.I;
        UpdateColor();

        // Set Orientation
        if (GameplayManager.GetInstance().SpawnRotationIsRandom)
            _Orientation = (Orientation)UnityEngine.Random.Range(0, 4);
        else
            _Orientation = Orientation.UP;

        _x = TetrisBoard._Width / 2;
        _y = TetrisBoard._Height - 1;

        // Set position randomly instead
        if (GameplayManager.GetInstance().SpawnXPositionIsRandom)
        {
            _x = UnityEngine.Random.Range(0, TetrisBoard._Width - 1);

            // Update Actual Piece Position (ignore min/max)
            UpdatePiece();

            // Min Fix
            if(_WorldMin.x < 0)
                _x -= (int)_WorldMin.x;
            // Max Fix
            if (_WorldMax.x > (TetrisBoard._Width - 1))
                _x -= ((int)_WorldMax.x - (TetrisBoard._Width - 1));
        }

        // Update Actual Piece Position
        UpdatePiece();
    }

    // DATA
    public int _x;
    public int _y;
    private int _ghost_y; // Draw ghost of where piece will be placed this many y values away
    public Color _Color;

    // Pieces
    public Vector2 _LocalMin = Vector2.zero;
    public Vector2 _LocalMax = Vector2.zero;
    public Vector2 _WorldMin = Vector2.zero;
    public Vector2 _WorldMax = Vector2.zero;

    private List<Vector2> _PiecesLocal = new List<Vector2>();
    private Vector2[]     _PiecesWorld = null;

    // Get Pieces
    public Vector2[] GetPieces()
    {
        return _PiecesWorld;
    }
    public float     GetPieceGhostY(int index)
    {
        return _PiecesWorld[index].y - (_y - _ghost_y);
    }

    // Get Pieces (Special)
    public Vector2 GetPieceLocalAverageLocation()
    {
        Vector2 R = Vector2.zero;
        for (int i = 0; i < _PiecesLocal.Count; i++)
        {
            R += _PiecesLocal[i];
        }
        return R / (float)(_PiecesLocal.Count);
    }
    public Vector2 GetPieceWorldAverageLocation()
    {
        Vector2 R = Vector2.zero;
        for(int i = 0; i < _PiecesWorld.Length; i++)
        {
            R += _PiecesWorld[i];
        }
        return R / (float)(_PiecesWorld.Length);
    }

    public Vector2[] GetPieces_LowestOnes()
    {
        List<Vector2> Pieces = new List<Vector2>();

        for (int i = 0; i < _PiecesWorld.Length; i++)
        {
            Vector2 Piece = _PiecesWorld[i];
            // Compare to list
            bool match = false;
            for(int p = 0; p < Pieces.Count; p++)
            {
                //Debug.Log(p);
                // If x matches, update lowest y
                if (Pieces[p].x == Piece.x)
                {
                    match = true;
                    Pieces[p] = new Vector2(Pieces[p].x, Math.Min(Pieces[p].y, Piece.y));
                }
            }
            if(!match)
                Pieces.Add(Piece);
        }
        return Pieces.ToArray();
    }

    public void AddPiece(Vector2 v, float rotation)
    {
        v = Quaternion.Euler(0, 0, -rotation) * v;
        v.x = Mathf.RoundToInt(v.x);
        v.y = Mathf.RoundToInt(v.y);

        // Add Local Piece
        _PiecesLocal.Add(v);
    }
    public void AddPieces(Vector2[] v)
    {
        float rotation = (float)_Orientation * 90.0f;

        for (int i = 0; i < v.Length; i++)
        {
            AddPiece(v[i], rotation);
        }
    }

    public enum Orientation
    {
        UP,
        RIGHT,
        DOWN,
        LEFT,
        TOTAL
    }
    public Orientation _Orientation = Orientation.UP;

    // UPDATERS
    public  void UpdatePiece(bool UpdateMinMax = true)
    {
        UpdatePieceLocal();
        UpdatePieceWorld();

        UpdatePieceGhost();

        if(UpdateMinMax)
            UpdateMinMaxValues();
    } 
    private void UpdatePieceLocal()
    {
        _PiecesLocal.Clear();
        switch (_Type)
        {
            case Tetrominoes.I:
                AddPieces(TetrominoConfig.I_Pieces);
                break;

            case Tetrominoes.J:
                AddPieces(TetrominoConfig.J_Pieces);
                break;

            case Tetrominoes.L:
                AddPieces(TetrominoConfig.L_Pieces);
                break;

            case Tetrominoes.O:
                AddPieces(TetrominoConfig.O_Pieces);
                break;

            case Tetrominoes.S:
                AddPieces(TetrominoConfig.S_Pieces);
                break;

            case Tetrominoes.T:
                AddPieces(TetrominoConfig.T_Pieces);
                break;

            case Tetrominoes.Z:
                AddPieces(TetrominoConfig.Z_Pieces);
                break;

        }
    }
    private void UpdatePieceWorld()
    {
        // Update Global Pieces
        _PiecesWorld = new Vector2[_PiecesLocal.Count];

        for (int i = 0; i < _PiecesLocal.Count; i++)
        {
            _PiecesWorld[i] = (new Vector2(_x + _PiecesLocal[i].x, _y + _PiecesLocal[i].y));
        }
    }
    private void UpdatePieceGhost()
    {
        int original_y = _y;
        _ghost_y = 0;
        for (int i = 0; i < original_y; i--)// most amount of checks possible
        {
            //Debug.Log("Check: " + i);
            _y--;
            UpdatePieceLocal();
            UpdatePieceWorld();
            // Check if solid
            if (CheckOverlapsSolid() || CheckFallBelowGrid())
            {
                _ghost_y = _y + 1;
                _y = original_y;
                UpdatePieceLocal();
                UpdatePieceWorld();
                return;
            }

            // Limit
            if (_y < -4)
                break;
        }
        // No ghost, ghost is same spot as player
        _y = original_y;
        _ghost_y = _y;
        UpdatePieceLocal();
        UpdatePieceWorld();
    }
    private void UpdateMinMaxValues()
    {
        // Update Local Min Max Values
        _LocalMin = new Vector2(+Mathf.Infinity, +Mathf.Infinity);
        _LocalMax = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

        for (int i = 0; i < _PiecesLocal.Count; i++)
        {
            if (_PiecesLocal[i].x < _LocalMin.x)
                _LocalMin.x = (int)_PiecesLocal[i].x;

            if (_PiecesLocal[i].x > _LocalMax.x)
                _LocalMax.x = (int)_PiecesLocal[i].x;

            if (_PiecesLocal[i].y < _LocalMin.y)
                _LocalMin.y = (int)_PiecesLocal[i].y;

            if (_PiecesLocal[i].y > _LocalMax.y)
                _LocalMax.y = (int)_PiecesLocal[i].y;
        }

        // Update world Min Max Values
        _WorldMin = new Vector2(+Mathf.Infinity, +Mathf.Infinity);
        _WorldMax = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

        for (int i = 0; i < _PiecesWorld.Length; i++)
        {
            if (_PiecesWorld[i].x < _WorldMin.x)
                _WorldMin.x = (int)_PiecesWorld[i].x;

            if (_PiecesWorld[i].x > _WorldMax.x)
                _WorldMax.x = (int)_PiecesWorld[i].x;

            if (_PiecesWorld[i].y < _WorldMin.y)
                _WorldMin.y = (int)_PiecesWorld[i].y;

            if (_PiecesWorld[i].y > _WorldMax.y)
                _WorldMax.y = (int)_PiecesWorld[i].y;
        }

    }
    // Update Display Color
    private void UpdateColor()
    {
        switch (_Type)
        {
            case Tetrominoes.I:
                _Color = new Color(1, 0, 0);
                break;

            case Tetrominoes.J:
                _Color = new Color(1, 0, 1);
                break;

            case Tetrominoes.L:
                _Color = new Color(1, 1, 0);
                break;

            case Tetrominoes.O:
                _Color = new Color(0, 1, 1);
                break;

            case Tetrominoes.S:
                _Color = new Color(0, 0, 1);
                break;

            case Tetrominoes.T:
                _Color = new Color(0.6f, 0.35f, 0.12f);
                break;

            case Tetrominoes.Z:
                _Color = new Color(0, 1, 0);
                break;
        }
    }

    // ACTIONS
    public bool MoveLeft()
    {
        _x--;
        UpdatePiece();

        // Check if can't move
        if (CheckLeaveGridHorizontally() != 0 || CheckOverlapsSolid())
        {
            _x++;
            UpdatePiece();
            return false;
        }
        return true;
    }
    public bool MoveRight()
    {
        _x++;
        UpdatePiece();
        if (CheckLeaveGridHorizontally() != 0 || CheckOverlapsSolid())
        {
            _x--;
            UpdatePiece();
            return false;
        }
        return true;
    }
    public bool MoveDown()
    {
        _y--;
        UpdatePiece();
        if (CheckIllegalPosition())
        {
            return false;
        }
        return true;
    }
    public int  Collapse()
    {
        _y = _ghost_y;
        int score = (_y - _ghost_y);
        return score;
    }

    // Rotation 
    public bool Rotate(bool clockwise)
    {
        // Do nothing for 0
        if (_Type == Tetrominoes.O)
            return true;

        Orientation Original = _Orientation;
        Orientation New_orientation;

        if (clockwise)
            New_orientation = RotateOrientationCW(_Orientation);
        else
            New_orientation = RotateOrientationCCW(_Orientation);

        // Change Orientation Temporarily
        _Orientation = New_orientation;
        // Update
        UpdatePiece();

        // If go below grid, revert and leave
        if(CheckFallBelowGrid())
        {
            _Orientation = Original;
            UpdatePiece();
            return false;
        }

        int start_x = _x;
        int nudgeAmount = 0;

        // Check if overlapping, if true, revert back
        if (CheckOverlapsSolid())
        {
            // Check if can't Nudge
            if(GameplayManager.GetInstance().RotationCantCauseMovement)
            {
                // Return to normal
                _Orientation = Original;
                UpdatePiece();
                return false;
            }

            // Nudge position
            nudgeAmount = RotationXNudge();

            // Failed to nudge
            if (nudgeAmount == 0)
            {
                // Failed to rotate, return to normal
                _Orientation = Original;
                UpdatePiece();
                return false;
            }

            // Nudge wins
            _x += nudgeAmount;
            UpdatePiece();
        }

        // [ RECHECK ]
        // If go below grid, revert and leave
        if (CheckFallBelowGrid())
        {
            _Orientation = Original;
            UpdatePiece();
            return false;
        }

        // Check if go past side limits
        int Offset = CheckLeaveGridHorizontally();
        // Attempt to Fix offset
        if(Offset != 0)
        {
            _x -= Offset;
            UpdatePiece();
            if (CheckOverlapsSolid())
            {
                // If failed, revert back to normal
                _x = start_x;
                _Orientation = Original;
                UpdatePiece();
                return false;
            }
        }
        return true;
    }
    private int RotationXNudge()
    {
        // Safety Check 
        if (CheckInSafeArea(+1, 0))
        {
            return 1;
        }
        else if(CheckInSafeArea(-1, 0))
        {
            return -1;
        }
        else if (CheckInSafeArea(+2, 0))
        {
            return 2;
        }
        else if(CheckInSafeArea(-2, 0))
        {
            return -2;
        }
        return 0;
    }

    public static Orientation RotateOrientationCW(Orientation Orient)
    {
        int New_orientation = (int)Orient + 1;

        if (New_orientation >= (int)Orientation.TOTAL)
            New_orientation = 0;

        return (Orientation)New_orientation;
    }
    public static Orientation RotateOrientationCCW(Orientation Orient)
    {
        int New_orientation = (int)Orient - 1;

        if (New_orientation < 0)
            New_orientation = 3;

        return (Orientation)New_orientation;
    }


    // CHECKERS
    public bool CheckIllegalPosition() // If piece has reached its end
    {
        return CheckOverlapsSolid() || CheckFallBelowGrid();
    }
    public bool CheckOverlapsSolid()
    {
        for (int i = 0; i < _PiecesWorld.Length; i++)
        {
            int x = (int)_PiecesWorld[i].x;
            int y = (int)_PiecesWorld[i].y;

            // Make sure x,y fit in board
            if (x >= 0 && x < TetrisBoard._Width && y >= 0 && y < TetrisBoard._Height)
            {
                // if piece is solid then you are in dead mode
                if (TetrisBoard._Board[y, x]._Solid)
                    return true;
            }
        }
        return false;
    }
    public int  CheckLeaveGridHorizontally()
    {
        for(int i = 0; i < _PiecesWorld.Length; i++)
        {
            if (_PiecesWorld[i].x < 0)
                return (int)_PiecesWorld[i].x;
            else if (_PiecesWorld[i].x >= TetrisBoard._Width)
                return (int)(_PiecesWorld[i].x - TetrisBoard._Width) + 1;
        }
        return 0;
    }
    public bool CheckFallBelowGrid()
    {
        for (int i = 0; i < _PiecesWorld.Length; i++)
        {
            if (_PiecesWorld[i].y < 0)
                return true;
        }
        return false;
    }
    public bool CheckInSafeArea(int x_offset, int y_offset)
    {
        _x += x_offset;
        _y += y_offset;
        UpdatePiece();
        bool check = (!CheckOverlapsSolid() && CheckLeaveGridHorizontally() == 0);
        _x -= x_offset;
        _y -= y_offset;
        UpdatePiece();
        return check;
    }


}
