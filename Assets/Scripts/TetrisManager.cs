using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    public int score {  get; private set; }
    
    public bool gameOver {  get; private set; }

    public bool betterRandomization = false;
    public bool useCustomSequence = false;
    
    
    
    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;
    
    // List for accessing the current set of randomized tetromino
    public List<Tetromino> currentTetrominoSet;

    public int currentPieceIndex = 0;


    void Start()
    {
        // Create new list of tetrominos to spawn
        currentTetrominoSet = new List<Tetromino>();


        
        // Shuffle tetrominos at start
        ShuffleBag();
        
        SetGameOver(false);
    }

    
    
    public void ShuffleBag()
    {
        if (useCustomSequence)
        {
            betterRandomization = false;
        }

        // Reset piece index, as this should only be called after all pieces
        // have been used in current bag
        currentPieceIndex = 0;
        
        // Create temp set
        List<Tetromino> tempTetrominoSet = new List<Tetromino>();
        
        // Loop through temp set, add tetromino pieces from enum
        for (int i = 0; i < 8; i++)
        {
            tempTetrominoSet.Add((Tetromino)i);
        }
        
        // LINQ shuffle temp set
        tempTetrominoSet = tempTetrominoSet.OrderBy(x => Random.Range(0, tempTetrominoSet.Count)).ToList();

        // Clear tetromino set, populate with new shuffle set
        currentTetrominoSet.Clear();
        currentTetrominoSet = tempTetrominoSet;
    }

    public int CalculateScore(int clearedRows)
    {
        switch (clearedRows)
        {
            case 1: return 100;
            case 2: return 300;
            case 3: return 500;
            case 4: return 800;
            case 5: return 4269;
            default: return 0;
        }
    }

    public void ChangeScore(int amount)
    {
        score += amount;
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool gameOver)
    {
        if (!gameOver)
        {
            // When the game over event is false, reset score
            score = 0;
            ChangeScore(0);
        }
        this.gameOver = gameOver;
        OnGameOver.Invoke();
    }

}
