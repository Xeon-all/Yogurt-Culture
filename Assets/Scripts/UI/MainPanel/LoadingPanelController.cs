using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanelController : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private Slider progressBar;

    [Header("Scenes")]
    [Tooltip("主界面场景名（推荐填写）。留空则使用 buildIndex。")]
    [SerializeField] private string mainSceneName = "Main";
    [Tooltip("当 mainSceneName 为空时使用的 buildIndex。")]
    [SerializeField] private int mainSceneBuildIndex = -1;

    [Header("Behavior")]
    [Tooltip("避免一闪而过的最短显示时间（秒）。")]
    [SerializeField] private float minDisplaySeconds = 0.2f;

    private void Reset() {
        progressBar = GetComponentInChildren<Slider>(true);
    }

    private void Awake() {
        if (progressBar != null) {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }
    }

    private void Start() {
        StartCoroutine(LoadMainSceneCoroutine());
    }

    private IEnumerator LoadMainSceneCoroutine() {
        float startTime = Time.realtimeSinceStartup;

        AsyncOperation op;
        if (!string.IsNullOrWhiteSpace(mainSceneName)) {
            op = SceneManager.LoadSceneAsync(mainSceneName);
        } else if (mainSceneBuildIndex >= 0) {
            op = SceneManager.LoadSceneAsync(mainSceneBuildIndex);
        } else {
            Debug.LogError("LoadingPanelController: mainSceneName is empty and mainSceneBuildIndex < 0.");
            yield break;
        }

        op.allowSceneActivation = false;

        while (!op.isDone) {
            // progress 到 0.9 代表加载完成，剩余 0.1 等待激活
            float normalized = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar != null) progressBar.value = normalized;

            bool loaded = op.progress >= 0.9f;
            bool minTimeReached = (Time.realtimeSinceStartup - startTime) >= minDisplaySeconds;
            if (loaded && minTimeReached) {
                if (progressBar != null) progressBar.value = 1f;
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

