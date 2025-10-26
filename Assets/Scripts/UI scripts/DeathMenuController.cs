using UnityEngine;
using UnityEngine.UI;

public class DeathMenuController : ControlMenuModal
{
    //will need to fetch new instance of playerAnimator post respawn
    [SerializeField] private PlayerAnimator PlayerAnimator;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    //local instance holder
    private PlayerAnimator playerAnimator;

    protected override void Awake()
    {
        base.Awake();
        playerAnimator = PlayerAnimator;
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (playerAnimator != null)
            playerAnimator.ShowGameOverMenu += OnGameOver;

        GameManager.Instance.OnPlayerRespawned += ReloadNewAnimator;

        retryButton.onClick.AddListener(() =>
        {
            ResumeTime();
            GameManager.Instance.RespawnPlayerAtCheckpoint();
            HideModal();
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

        GameManager.Instance.OnPlayerRespawned -= ReloadNewAnimator;

        retryButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
    }

    private void ReloadNewAnimator(GameObject newPlayer)
    {
        playerAnimator = newPlayer.GetComponent<PlayerAnimator>();
        playerAnimator.ShowGameOverMenu += OnGameOver;
    }

    private void OnGameOver()
    {
        ShowModal();
        Time.timeScale = 0f;
    }

    private void ResumeTime() => Time.timeScale = 1f;
}

