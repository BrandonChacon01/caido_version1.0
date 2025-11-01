using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // <-- importante

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtProgress; // <-- ahora es TMP

    private void Start()
    {
        string target = LoadingPayload.NextScene;
        if (string.IsNullOrEmpty(target))
        {
            target = "SampleScene";
        }
        StartCoroutine(LoadAsync(target));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        yield return null;
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (txtProgress) txtProgress.text = $"Cargando… {(int)(p * 100)}%";

            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
