using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoadHelper {
    public static void LoadScene(string sceneName, int fallbackBuildIndex = -1) {
        if (!string.IsNullOrWhiteSpace(sceneName)) {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (fallbackBuildIndex >= 0) {
            SceneManager.LoadScene(fallbackBuildIndex);
        } else {
            Debug.LogError("SceneLoadHelper.LoadScene: sceneName is empty and fallbackBuildIndex < 0.");
        }
    }

    public static void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

