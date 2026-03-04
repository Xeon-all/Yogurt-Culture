using UnityEngine;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [Tooltip("加载界面场景名（推荐填写）。留空则使用 buildIndex。")]
    [SerializeField] private string loadingSceneName = "Loading";
    [Tooltip("当 loadingSceneName 为空时使用的 buildIndex。")]
    [SerializeField] private int loadingSceneBuildIndex = -1;

    private void Reset() {
        var buttons = GetComponentsInChildren<Button>(true);
        if (buttons != null) {
            if (buttons.Length > 0) startButton = buttons[0];
            if (buttons.Length > 1) quitButton = buttons[1];
        }
    }

    private void OnEnable() {
        if (startButton != null) startButton.onClick.AddListener(OnClickStart);
        if (quitButton != null) quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnDisable() {
        if (startButton != null) startButton.onClick.RemoveListener(OnClickStart);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnClickQuit);
    }

    public void OnClickStart() {
        SceneLoadHelper.LoadScene(loadingSceneName, loadingSceneBuildIndex);
    }

    public void OnClickQuit() {
        SceneLoadHelper.QuitGame();
    }
}

