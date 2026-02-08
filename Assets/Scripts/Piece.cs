using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Set variables
    public TetrominoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    // Freeze bool for preventing piece manipulation
    public bool freeze = false;

    private int activeCellCount = -1;

    // Initializes cell data for a given shape
    public void Initialize(Board board, Tetromino tetromino)
    {
        // Get board reference
        this.board = board;

        // Search and assign tetromino data
        for (int i = 0; i < board.tetrominos.Length; i++)
        {
            if (board.tetrominos[i].tetromino == tetromino)
            {
                this.data = board.tetrominos[i];
                break;
            }
        }

        // Create copy of cell locations
        cells = new Vector2Int[data.cells.Length];

        // Populate new cell array
        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = data.cells[i];
        }

        // Set position to start position
        position = board.startPosition;

        // Set active cell count to the amount of cells the piece has
        activeCellCount = cells.Length;
    }

    private void Update()
    {
        // If the game is over, stop updating piece
        if (board.tetrisManager.gameOver) return;
        
        // If the piece is frozen, break out of update and prevent rest of loop from running
        if (freeze) return;

        // Clear the board every frame
        board.Clear(this);

        // Hard drop has priority
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            // Left / Right movement
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            // Downward movement
            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            // Left / Right Rotation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1);
            }
        }

        // Set board with this tetromino
        board.Set(this);

        // Check board after setting
        if (freeze)
        {
            // check board
            board.CheckBoard();

            // spawn piece
            board.SpawnPiece();
        }
    }

    void HardDrop()
    {
        // Algorithm: repeatedly move down until we cant
        while(Move(Vector2Int.down))
        {
            // do nothing, logic for move is called in the Move() func
        }

        // If piece can't move anymore, freeze piece
        freeze = true;
    }

    // Rotate piece in a given direction
    void Rotate(int direction)
    {
        // Copy cells in case we need to revert back to original
        Vector2Int[] originalCells = new Vector2Int[cells.Length];

        // Loop through all current cells and populate original cells list
        for (int i = 1; i < cells.Length; i++)
        {
            originalCells[i] = cells[i];
        }

        ApplyRotation(direction);

        // Check position after rotation
        if (!board.IsPositionValid(this, position))
        {
            // If position is not valid, attempt wall kick
            if (!TryWallKicks())
            {
                // Revert rotation if wall kicks fail
                RevertRotation(originalCells);
            }
        }
    }

    void RevertRotation(Vector2Int[] originalCells)
    {
        // Loop through all current cells
        for (int i = 0; i < cells.Length; i++)
        {
            // Apply given original cell position data to current cells
            cells[i] = originalCells[i];
        }
    }

    bool TryWallKicks()
    {
        // Create list of wallkick offsets
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>()
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1)
        };

        // If I tetromino, add new offsets
        if (data.tetromino == Tetromino.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);
        }

        // Loop through all offsets
        foreach(Vector2Int offset in wallKickOffsets)
        {
            // If the offet results in a valid move, return true
            if (Move(offset))
            {
                return true;
            }
        }

        return false;
    }

    void ApplyRotation(int direction)
    {
        // Create new rotation variable
        Quaternion rotation = Quaternion.Euler(0, 0, 90 * direction);

        bool isSpecial = data.tetromino == Tetromino.I || data.tetromino == Tetromino.O;
        // Loop through all cell positions 
        for (int i = 0; i < cells.Length; i++)
        {
            // Convert cell location to vec3 for quaternions
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            // Fix origin
            if (isSpecial)
            {
                cellPosition.x -= 0.5f;
                cellPosition.y -= 0.5f;
            }

            // Get result
            Vector3 result = rotation * cellPosition;

            // Apply change
            if (isSpecial)
            {
                // If piece is I or O tetromino, apply ceil to int
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                // Every other piece round to int
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }
        }
    }

    // Move piece 
    public bool Move(Vector2Int translation)
    {
        // Create temp position
        Vector2Int newPosition = position;

        // Add translation to temp position
        newPosition += translation;

        // Store if new position is valid
        bool positionValid = board.IsPositionValid(this, newPosition);

        // If position is valid set position
        if (positionValid)
        {
            position = newPosition;
        }

        // Return true or false
        return positionValid;
    }

    public void ReduceActiveCount()
    {
        // Decrement active cell count
        activeCellCount -= 1;
        
        // If no cells are left, destroy piece gameobject
        if (activeCellCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
