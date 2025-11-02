using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";   // Tu escena de juego
    [SerializeField] private string loadingSceneName = "Loading";    // La escena de carga que haremos

    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;

    [Header("Buttons")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnBackFromOptions;

    private void Awake()
    {
        if (btnPlay) btnPlay.onClick.AddListener(OnPlay);
        if (btnOptions) btnOptions.onClick.AddListener(ShowOptions);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuit);
        if (btnBackFromOptions) btnBackFromOptions.onClick.AddListener(ShowMain);

        ShowMain();
    }

    private void ShowMain()
    {
        if (panelMain) panelMain.SetActive(true);
        if (panelOptions) panelOptions.SetActive(false);
        if (btnPlay) btnPlay.Select(); // deja seleccionado "Jugar" para Enter/teclado
    }

    private void ShowOptions()
    {
        if (panelMain) panelMain.SetActive(false);
        if (panelOptions) panelOptions.SetActive(true);
        if (btnBackFromOptions) btnBackFromOptions.Select();
    }

    private void OnPlay()
    {
        if (string.IsNullOrEmpty(loadingSceneName))
        {
            SceneManager.LoadScene(gameSceneName); // fallback
        }
        else
        {
            LoadingPayload.NextScene = gameSceneName;
            SceneManager.LoadScene(loadingSceneName);
        }
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        Debug.Log("[MainMenu] Salir (en Editor no se cierra).");
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (panelOptions && panelOptions.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMain();
        }
    }
}

// Contenedor simple para pasar el nombre de la escena a la pantalla de carga
public static class LoadingPayload
{
    public static string NextScene;
}
