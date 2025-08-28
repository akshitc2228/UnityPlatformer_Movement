using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : ControlMenuModal
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button crossIcon;
    [SerializeField] private FreezeInputEventSO freezeInputEvent;

    private bool isModalVisible = false;

    private void OnEnable()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance.ReturnToMenu();
        });

        quitGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.QuitGame();
        });

        crossIcon.onClick.AddListener(() => HidePauseModal());
    }

    private void OnDisable()
    {
        mainMenuButton.onClick.RemoveAllListeners();
        quitGameButton.onClick.RemoveAllListeners();
        crossIcon.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isModalVisible)
                ShowPauseModal();
            else
                HidePauseModal();
        }
    }

    private void ShowPauseModal()
    {
        Time.timeScale = 0f;
        base.ShowModal();
        isModalVisible = true;

        freezeInputEvent?.Raise(true);
    }

    private void HidePauseModal()
    {
        Time.timeScale = 1f;
        base.HideModal();
        isModalVisible = false;

        freezeInputEvent?.Raise(false);
    }
}
