using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public void StartGame()
    {
        CurrentState = GameState.Playing;
        SceneManager.LoadScene("MainLevel");
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame(bool pause)
    {
        CurrentState = pause ? GameState.Paused : GameState.Playing;
        Time.timeScale = pause ? 0 : 1;
    }
}
