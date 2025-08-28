using UnityEngine;
using UnityEngine.UI;

public class DeathMenuController : ControlMenuModal
{
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (playerAnimator != null)
            playerAnimator.ShowGameOverMenu += OnGameOver;

        retryButton.onClick.AddListener(() =>
        {
            ResumeTime();
            GameManager.Instance.StartGame();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            ResumeTime();
            GameManager.Instance.ReturnToMenu();
        });
    }

    private void OnDisable()
    {
        if (playerAnimator != null)
            playerAnimator.ShowGameOverMenu -= OnGameOver;

        retryButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
    }

    private void OnGameOver()
    {
        ShowModal();
        Time.timeScale = 0f;
    }

    private void ResumeTime() => Time.timeScale = 1f;
}

