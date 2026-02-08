using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TetrisManager tetrisManager;

    public GameObject gameOverPanel;

    private bool loadBetterShuffle = false;


    public void UpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score:n0}";
    }

    public void UpdateGameOver()
    {
        gameOverPanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        if (tetrisManager.useCustomSequence)
        {
            LoadCustomGamemode();
        }
        
        // Reset game
        tetrisManager.SetGameOver(false);
    }

    public void LoadCustomGamemode()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadNormalGamemode()
    {
        if (loadBetterShuffle)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            SceneManager.LoadScene(1); 
        }
    }

    public void UpdateRandomShuffle()
    {
        loadBetterShuffle = !loadBetterShuffle;
    }
}
