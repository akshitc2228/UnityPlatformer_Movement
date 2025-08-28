using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startGameButton;   
    [SerializeField] private Button quitGameButton;

    private void OnEnable()
    {
        startGameButton.onClick.AddListener(() => GameManager.Instance.StartGame());
        quitGameButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
    }

    private void OnDisable()
    {
        startGameButton.onClick.RemoveAllListeners();
        quitGameButton.onClick.RemoveAllListeners();
    }
}
