using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public Transform RecentCheckpoint { get; private set; }
    public GameObject PlayerInstance { get; private set; }

    [SerializeField] private GameObject playerPrefab;

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

    public void SetCheckpoint(Transform checkpoint)
    {
        RecentCheckpoint = checkpoint;
    }

    public void RespawnPlayerAtCheckpoint()
    {
        if (playerPrefab == null) throw new MissingComponentException();
        if (RecentCheckpoint != null)
        {
            PlayerInstance = Instantiate(playerPrefab, RecentCheckpoint.transform.position, Quaternion.identity);
            OnPlayerRespawned?.Invoke(PlayerInstance);
        }
        else
            throw new MissingReferenceException("recent checkpoint was not found");
    }

    public event System.Action<GameObject> OnPlayerRespawned;
}
