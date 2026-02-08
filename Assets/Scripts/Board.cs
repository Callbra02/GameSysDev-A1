using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece prefabPiece;
    public Tilemap tilemap;
    public Tilemap defaultTilemap;
    public Tilemap customSequenceTilemap;
    public TetrominoData[] tetrominos;
    public Vector2Int boardSize;
    public Vector2Int startPosition;

    private Piece activePiece;

    private float dropTime = 0.0f;
    private float dropInterval = 0.5f;

    public int customSequenceIndex = 0;
    // Maps tilemap position to a piece gameObject
    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece> ();
    
    public List<Tetromino> customSequenceList = new List<Tetromino> { Tetromino.I, Tetromino.O, Tetromino.C, Tetromino.T, Tetromino.J, Tetromino.C, Tetromino.L };

    // Getters for board sides -------------------------------------------------
    int left
    {
        get { return -boardSize.x / 2; }
    }

    int right
    {
        get { return boardSize.x / 2; }
    }

    int top
    {
        get { return boardSize.y / 2; }
    }

    int bottom
    {
        get { return -boardSize.y / 2; }
    }
    //  Getters for board sides ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    private void Awake()
    {
        if (tetrisManager.useCustomSequence)
        {
            tilemap = customSequenceTilemap;
        }
        else
        {
            customSequenceTilemap.gameObject.SetActive(false);
            tilemap = defaultTilemap;
        }
    }
    
    private void Update()
    {
        // If game is over, break from board update logic
        if (tetrisManager.gameOver) return;
        
        // Increment drop time
        dropTime += Time.deltaTime;
        
        // If drop time surpasses the interval for drops
        if (dropTime >= dropInterval)
        {
            // Reset drop time
            dropTime = 0.0f;
            
            // Clear the active piece
            Clear(activePiece);
            
            // Move piece down
            bool moveResult = activePiece.Move(Vector2Int.down);
            
            // Set piece
            Set(activePiece);

            // If move fails, piece is stuck/cant move further down
            if (!moveResult)
            {
                // Freeze piece, check board, spawn new piece
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }
    
    public void SpawnPiece()
    {
        // Instatiate new tetromino
        activePiece = Instantiate(prefabPiece);

        Tetromino t;
        
        // If using custom sequence, spawn in tetrominoes from given list
        if (tetrisManager.useCustomSequence)
        {
            if (customSequenceIndex > customSequenceList.Count - 1)
            {
                customSequenceIndex = 0;
                tetrisManager.SetGameOver(true);
                t = Tetromino.C;
            }
            else
            {
                t = customSequenceList[customSequenceIndex];
            }
            
            customSequenceIndex++;
            
            activePiece.Initialize(this, t);

            CheckEndGame();
            Set(activePiece);
            
            return;
        }
        
        // If better randomization is toggled, do LINQ shuffle
        if (tetrisManager.betterRandomization)
        {
            // Re suffle bag, if piece index is over or equal to 8
            if (tetrisManager.currentPieceIndex >= 8)
            {
                tetrisManager.ShuffleBag();
            }
        
            // Set tetromino to random shape
            t = tetrisManager.currentTetrominoSet[tetrisManager.currentPieceIndex];

            tetrisManager.currentPieceIndex++;
        }
        else
        {
            // Randomly select tetromino
            t = (Tetromino)Random.Range(0, tetrominos.Length);
        }
        
        // Initialize and set new tetromino piece
        activePiece.Initialize(this, t);
        
        // Check for end game
        CheckEndGame();
        
        // Set piece
        Set(activePiece);
    }

    private void CheckEndGame()
    {
        // If there is not a valid position for the new piece / game over
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        // gameOver being false means we either started a new game
        // or reset the game
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    private void ResetBoard()
    {
        // Get all pieces
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        // Destroy all pieces
        foreach (Piece piece in foundPieces)
        {
            Destroy(piece.gameObject);
        }

        // Remove activePiece
        activePiece = null;
        
        // Only clear all tiles if not using custome sequence
        if (!tetrisManager.useCustomSequence)
        {
            tilemap.ClearAllTiles();
        }
        
        pieces.Clear();
        
        // Spawn a new piece on start
        SpawnPiece();
    }

    private void SetTile(Vector3Int cellPosition, Piece piece)
    {
        // If the piece is null
        if (piece == null)
        {
            // Set tile to null
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);

            // Creates an association between cell position and piece gameobject
            pieces[cellPosition] = piece;
        }
    }

    public void Clear(Piece piece)
    {
        // Loop through all tetromino cells and set tiles to null
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    // Color piece tiles
    public void Set(Piece piece)
    {
        // Loop through all tetromino sells and set tiles accordingly
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    // Check if line is full
    bool IsLineFull(int y)
    {
        // Loop from left of row to right
        for (int x = left; x < right; x++)
        {
            // Create new position
            Vector3Int cellPosition = new Vector3Int(x, y);

            // Return false if there are any cells that are empty
            if (!tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }

        // Else return true
        return true;
    }

    // Destroy all pieces on a given line
    void DestroyLine(int y)
    {
        // Loop from left to right of row
        for (int x = left; x < right; x++)
        {
            // Set all tiles in row to null
            Vector3Int cellPosition = new Vector3Int(x, y);

            // Cleanup gameobjects here
            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];

                piece.ReduceActiveCount();

                SetTile(cellPosition, null);
            }

            // Remove tile at position if using custom sequence
            if (tetrisManager.useCustomSequence)
            {
                tilemap.SetTile(cellPosition, null);
            }
        }
    }

    // Shift all rows down from a given row
    void ShiftRowsDown(int clearedRow)
    {
        // From the cleared row to the top of the board
        for (int y = clearedRow + 1; y < top; y++)
        {
            // From left to right of the row
            for (int x = left; x < right; x++)
            {
                // Get current cell position
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                {
                    // Save temp piece
                    Piece currentPiece = pieces[cellPosition];

                    // Clear tile
                    SetTile(cellPosition, null);

                    // Move temp tile down
                    cellPosition.y--;

                    // Set tiles
                    SetTile(cellPosition, currentPiece);
                }

            }
        }
    }

    public void CheckBoard()
    {
        // Create new list of destroyed lines
        List<int> destroyedLines = new List<int>();

        // Bottom to Top
        for (int y = bottom; y < top; y++)
        {
            // Check if line is full, if so destroy line and add line y value to destroyed lines list
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShiftedDown = 0;
        // For every line in destroyed lines list, shift all rows above down
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        // Update Score

        // If the last piece in the custom sequence of tetrominos is our new piece, add more score
        if (tetrisManager.useCustomSequence && customSequenceIndex > 0 && customSequenceList[customSequenceIndex - 1] == Tetromino.C)
        {
            int customScore = tetrisManager.CalculateScore(5);
            tetrisManager.ChangeScore(customScore);
            return;
        }
        
        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);
    }

    // Returns if a position is valid for tetromino movement
    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        // Loop through all cells in the tetromino
        for (int i = 0; i < piece.cells.Length;  i++)
        {
            // Get cell position
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // Bounds check
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top)
                return false;
            
            // Return false if this position is invalid
            if (tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }

        return true;
    }
}
